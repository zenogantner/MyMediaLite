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
using System.Globalization;
using MyMediaLite.Data;
using MyMediaLite.DataType;

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
	/// 		(see Section 3 of the paper)
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class KorenImplicitKNN : SVDPlusPlus
	{
		/// <summary>Weights in the neighborhood model that represent coefficients relating items based on the existing ratings</summary>
		protected Matrix<float> w;

		/// <summary>Weights in the neighborhood model that represent the implicit feedback from users</summary>
		protected Matrix<float> c;

		/// <summary>the K most relevant items according to given item</summary>
		protected IList<int>[] k_relevant_items;

		/// <summary>Matrix indicating which item is preferred by which user</summary>
		protected SparseBooleanMatrix additional_data_item;

		///
		protected ItemKNN Predictor;

		/// <summary>Number of neighbors to take into account for predictions</summary>
		public uint K { get { return k; } set { k = value; } }
		private uint k;

		/// <summary>Default constructor</summary>
		public KorenImplicitKNN() : base()
		{
			K = 30;
		}

		///
		protected void InitNeighborhoodModel()
		{
			Predictor = new ItemKNN();
			Predictor.Ratings = Ratings;
			Predictor.Correlation = MyMediaLite.Correlation.RatingCorrelationType.Pearson;
			Predictor.Train();

			w = new Matrix<float>(MaxItemID + 1, MaxItemID + 1);
			w.InitNormal(InitMean, InitStdDev);

			c = new Matrix<float>(MaxItemID + 1, MaxItemID + 1);
			c.InitNormal(InitMean, InitStdDev);

			user_bias = new float[MaxUserID + 1];
			item_bias = new float[MaxItemID + 1];

			k_relevant_items = new IList<int>[MaxItemID + 1];
			for (int item_id = 0; item_id <= MaxItemID; item_id++)
			{
				k_relevant_items[item_id] = Predictor.GetMostSimilarItems(item_id, K);
			}

			additional_data_item = new SparseBooleanMatrix();
			var additional_feedback = AdditionalFeedback;
			for (int index = 0; index < additional_feedback.Count; index++)
				additional_data_item[additional_feedback.Items[index], additional_feedback.Users[index]] = true;
		}

		///
		public override void Train()
		{
			InitNeighborhoodModel();

			// learn model parameters
			global_bias = ratings.Average;
			LearnFactors(ratings.RandomIndex, true, true);
		}

		///
		protected void TrainAncestors()
		{
			base.Train();
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			float reg = Regularization; // to limit property accesses

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				float err = ratings[index] - Predict(u, i, false);

				float user_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByUser[u])) : reg;
				float item_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByItem[i])) : reg;

				// adjust biases
				if (update_user)
					user_bias[u] += BiasLearnRate * current_learnrate * ((float) err - BiasReg * user_reg_weight * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * current_learnrate * ((float) err - BiasReg * item_reg_weight * item_bias[i]);

				IList<int> rkiu = new List<int>();
				IList<int> nkiu = new List<int>();
				foreach (int j in k_relevant_items[i])
				{
					if (Predictor.data_item[j, u])
					{
						rkiu.Add(j);
					}
					if (Predictor.data_item[j, u] || additional_data_item[j, u])
					{
						nkiu.Add(j);
					}
				}

				foreach (int j in rkiu)
				{
					float rating  = ratings.Get(u, j, ratings.ByUser[u]);
					w[i, j] += current_learnrate * ((err / (float)Math.Sqrt(rkiu.Count)) * (rating - Predictor.baseline_predictor.Predict(u, j)) - reg * w[i, j]);
				}

				foreach (int j in nkiu)
				{
					c[i, j] += current_learnrate * ((err / (float)Math.Sqrt(nkiu.Count)) - reg * c[i, j]);
				}
			}

			UpdateLearnRate();
		}

		///
		protected override float Predict(int user_id, int item_id, bool bound)
		{
			float result = global_bias;

			if (user_id < user_bias.Length)
				result += user_bias[user_id];
			if (item_id < item_bias.Length)
				result += item_bias[item_id];
			if (user_id <= MaxUserID && item_id <= MaxItemID)
			{
				float r_sum = 0;
				int r_count = 0;
				float n_sum = 0;
				int n_count = 0;
				foreach (int j in k_relevant_items[item_id])
				{
					if (Predictor.data_item[j, user_id])
					{
						float rating  = ratings.Get(user_id, j, ratings.ByUser[user_id]);
						r_sum += (rating - Predictor.baseline_predictor.Predict(user_id, j)) * w[item_id, j];
						r_count++;
					}
					if (Predictor.data_item[j, user_id] || additional_data_item[j, user_id])
					{
						n_sum += c[item_id, j];
						n_count++;
					}
				}
				if (r_count > 0)
					result += r_sum / (float)Math.Sqrt(r_count);
				if (n_count > 0)
					result += n_sum / (float)Math.Sqrt(n_count);
			}

			if (bound)
			{
				if (result > MaxRating)
					return MaxRating;
				if (result < MinRating)
					return MinRating;
			}
			return result;
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
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} K={1} regularization={2} bias_reg={3} frequency_regularization={4} learn_rate={5} bias_learn_rate={6} num_iter={7}",
				this.GetType().Name, K, Regularization, BiasReg, FrequencyRegularization, LearnRate, BiasLearnRate, NumIter);
		}
	}
}
