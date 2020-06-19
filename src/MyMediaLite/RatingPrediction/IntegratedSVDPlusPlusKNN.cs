// Copyright (C) 2012 Marcelo Manzato, Zeno Gantner
// 
// This file is part of MyMediaLite.
// 
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.Collections.Generic;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Correlation;
using System.Globalization;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Neighborhood model that also takes into account _what_ users have rated (implicit feedback)</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren:
	///         Factorization Meets the Neighborhood: a Multifaceted Collaborative Filtering Model.
	///         KDD 2008.
	///         http://research.yahoo.com/files/kdd08koren.pdf
	/// 		(see Section 5 of the paper)
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>	
	public class IntegratedSVDPlusPlusKNN : KorenImplicitKNN, ITransductiveRatingPredictor
	{	
		///
		public override void Train()
		{
			InitNeighborhoodModel();
			TrainAncestors();
		}
		
		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			user_factors = null; // delete old user factors
			float reg = Regularization; // to limit property accesses			
			
			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];
				
				UpdateSimilarItems(u, i);
				
				float prediction = base.Predict(u, i, false);
				var p_plus_y_sum_vector = y.SumOfRows(items_rated_by_user[u]);
				double norm_denominator = Math.Sqrt(items_rated_by_user[u].Length);
				for (int f = 0; f < p_plus_y_sum_vector.Count; f++)
					p_plus_y_sum_vector[f] = (float) (p_plus_y_sum_vector[f] / norm_denominator + p[u, f]);

				prediction += DataType.MatrixExtensions.RowScalarProduct(item_factors, i, p_plus_y_sum_vector);				
				
				float err = ratings[index] - prediction;

				float user_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByUser[u])) : reg;
				float item_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByItem[i])) : reg;

				// adjust biases
				if (update_user)
					user_bias[u] += BiasLearnRate * current_learnrate * ((float) err - BiasReg * user_reg_weight * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * current_learnrate * ((float) err - BiasReg * item_reg_weight * item_bias[i]);
				
				// adjust item similarities
				foreach (int j in rkiu) 
				{
					float rating  = ratings.Get(u, j, ratings.ByUser[u]);
					w[i, j] += current_learnrate * ((err / (float)Math.Sqrt(rkiu.Count)) * (rating - BasePredict(u, j)) - reg * w[i, j]);	
				}
				foreach (int j in nkiu) 
				{					
					c[i, j] += current_learnrate * ((err / (float)Math.Sqrt(nkiu.Count)) - reg * c[i, j]);	
				}
				
				// adjust factors
				double normalized_error = err / norm_denominator;
				for (int f = 0; f < NumFactors; f++)
				{
					float i_f = item_factors[i, f];

					// if necessary, compute and apply updates
					if (update_user)
					{
						double delta_u = err * i_f - user_reg_weight * p[u, f];
						p.Inc(u, f, current_learnrate * delta_u);
					}
					if (update_item)
					{
						double delta_i = err * p_plus_y_sum_vector[f] - item_reg_weight * i_f;
						item_factors.Inc(i, f, current_learnrate * delta_i);
						double common_update = normalized_error * i_f;
						foreach (int other_item_id in items_rated_by_user[u])
						{
							double delta_oi = common_update - y_reg[other_item_id] * y[other_item_id, f];
							y.Inc(other_item_id, f, current_learnrate * delta_oi);
						}
					}
				}
			}
			
			UpdateLearnRate();
		}
		
		/// <summary>Predict the rating of a given user for a given item</summary>		
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override float Predict(int user_id, int item_id)
		{
			return Predict(user_id, item_id, true);
		}
		
		///
		protected override float Predict(int user_id, int item_id, bool bound)
		{
			double result = global_bias;

			if (user_factors == null)
				PrecomputeUserFactors();

			if (user_id < user_bias.Length)
				result += user_bias[user_id];
			if (item_id < item_bias.Length)
				result += item_bias[item_id];
			if (user_id <= MaxUserID && item_id <= MaxItemID) 
			{
				result += DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);
				
				float r_sum = 0;
				int r_count = 0;
				float n_sum = 0;
				int n_count = 0;
				
				if(bound)
				{
					UpdateSimilarItems(user_id, item_id);	
				}
				
				foreach (int j in rkiu) 
				{
					float rating  = ratings.Get(user_id, j, ratings.ByUser[user_id]);
					r_sum += (rating - BasePredict(user_id, j)) * w[item_id, j];
					r_count++;
				}
				foreach (int j in nkiu) 
				{
					n_sum += c[item_id, j];
					n_count++;
				}				
				
				if (r_count > 0)
					result += r_sum / (float)Math.Sqrt(r_count);				
				if (n_count > 0)
					result += n_sum / (float)Math.Sqrt(n_count);
			}
			
			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;

			return (float) result;
		}
		
		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} regularization={2} bias_reg={3} frequency_regularization={4} learn_rate={5} bias_learn_rate={6} num_iter={7} K={8} decay={9}",
				this.GetType().Name, NumFactors, Regularization, BiasReg, FrequencyRegularization, LearnRate, BiasLearnRate, NumIter, K, Decay);
		}
	}
}
