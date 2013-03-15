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
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Item Attribute Aware SVD++: Matrix factorization that also takes into account _what_ users have rated and its attributes.</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Marcelo Manzato:
	///         gSVD++: supporting implicit feedback on recommender systems with metadata awareness.
	///         SAC 2013.
	///
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class GSVDPlusPlus : SVDPlusPlus, IItemAttributeAwareRecommender
	{
		///
		public IBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private IBooleanMatrix item_attributes;

		///
		public int NumItemAttributes { get; private set; }

		/// <summary>item factors (part expressed via the items attributes)</summary>
		protected Matrix<float> x;

		/// <summary>item factors (individual part)</summary>
		protected Matrix<float> q;

		/// <summary>precomputed regularization terms for the x matrix</summary>
		protected float[] x_reg;

		///
		protected internal override void InitModel()
		{
			base.InitModel();

			x = new Matrix<float>(item_attributes.NumberOfColumns, NumFactors);
			x.InitNormal(InitMean, InitStdDev);
			q = new Matrix<float>(MaxItemID + 1, NumFactors);
			q.InitNormal(InitMean, InitStdDev);

			// set factors to zero for items without training examples
			for (int i = 0; i < ratings.CountByItem.Count; i++)
				if (ratings.CountByItem[i] == 0)
					q.SetRowToOneValue(i, 0);
		}

		///
		public override void Train()
		{
			int num_attributes = item_attributes.NumberOfColumns;

			x_reg = new float[num_attributes];
			for (int attribute_id = 0; attribute_id < num_attributes; attribute_id++)
				x_reg[attribute_id] = FrequencyRegularization ? (float) (Regularization / item_attributes.NumEntriesByColumn(attribute_id)) : Regularization;

			base.Train();
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			user_factors = null; // delete old user factors
			item_factors = null; // delete old item factors
			float reg = Regularization; // to limit property accesses

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double prediction = global_bias + user_bias[u] + item_bias[i];
				var p_plus_y_sum_vector = y.SumOfRows(items_rated_by_user[u]);
				double norm_denominator = Math.Sqrt(items_rated_by_user[u].Length);
				for (int f = 0; f < p_plus_y_sum_vector.Count; f++)
					p_plus_y_sum_vector[f] = (float) (p_plus_y_sum_vector[f] / norm_denominator + p[u, f]);

				var q_plus_x_sum_vector = q.GetRow(i);

				if (i < item_attributes.NumberOfRows)
				{
					IList<int> attribute_list = item_attributes.GetEntriesByRow(i);
					double second_norm_denominator = attribute_list.Count;
					var x_sum_vector = x.SumOfRows(attribute_list);
					for (int f = 0; f < x_sum_vector.Count; f++)
						q_plus_x_sum_vector[f] += (float) (x_sum_vector[f] / second_norm_denominator);
				}

				prediction += DataType.VectorExtensions.ScalarProduct(q_plus_x_sum_vector, p_plus_y_sum_vector);

				double err = ratings[index] - prediction;

				float user_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByUser[u])) : reg;
				float item_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByItem[i])) : reg;

				// adjust biases
				if (update_user)
					user_bias[u] += BiasLearnRate * current_learnrate * ((float) err - BiasReg * user_reg_weight * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * current_learnrate * ((float) err - BiasReg * item_reg_weight * item_bias[i]);

				double normalized_error = err / norm_denominator;
				for (int f = 0; f < NumFactors; f++)
				{
					float i_f = q_plus_x_sum_vector[f];

					// if necessary, compute and apply updates
					if (update_user)
					{
						double delta_u = err * i_f - user_reg_weight * p[u, f];
						p.Inc(u, f, current_learnrate * delta_u);
					}
					if (update_item)
					{
						double common_update = normalized_error * i_f;
						foreach (int other_item_id in items_rated_by_user[u])
						{
							double delta_oi = common_update - y_reg[other_item_id] * y[other_item_id, f];
							y.Inc(other_item_id, f, current_learnrate * delta_oi);
						}

						double delta_i = err * p_plus_y_sum_vector[f] - item_reg_weight * q[i, f];
						q.Inc(i, f, current_learnrate * delta_i);

						// adjust attributes
						if (i < item_attributes.NumberOfRows)
						{
							IList<int> attribute_list = item_attributes.GetEntriesByRow(i);
							double second_norm_denominator = attribute_list.Count;
							double second_norm_error = err / second_norm_denominator;

							foreach (int attribute_id in attribute_list)
							{
								double delta_oi = second_norm_error * p_plus_y_sum_vector[f] - x_reg[attribute_id] * x[attribute_id, f];
								x.Inc(attribute_id, f, current_learnrate * delta_oi);
							}
						}
					}
				}
			}

			UpdateLearnRate();
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			double result = global_bias;

			if (user_factors == null)
				PrecomputeUserFactors();

			if (item_factors == null)
				PrecomputeItemFactors();

			if (user_id < user_bias.Length)
				result += user_bias[user_id];
			if (item_id < item_bias.Length)
				result += item_bias[item_id];
			if (user_id <= MaxUserID && item_id <= MaxItemID)
				result += DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;

			return (float) result;
		}

		/// <summary>Precompute all item factors</summary>
		protected void PrecomputeItemFactors()
		{
			if (item_factors == null)
				item_factors = new Matrix<float>(MaxItemID + 1, NumFactors);

			for (int item_id = 0; item_id <= MaxItemID; item_id++)
				PrecomputeItemFactors(item_id);
		}

		/// <summary>Precompute the factors for a given item</summary>
		/// <param name='item_id'>the ID of the item</param>
		protected void PrecomputeItemFactors(int item_id)
		{
			// compute
			var factors = q.GetRow(item_id);
			if (item_id < item_attributes.NumberOfRows)
			{
				IList<int> attribute_list = item_attributes.GetEntriesByRow(item_id);
				double second_norm_denominator = attribute_list.Count;
				var x_sum_vector = x.SumOfRows(attribute_list);
				for (int f = 0; f < x_sum_vector.Count; f++)
					factors[f] += (float) (x_sum_vector[f] / second_norm_denominator);
			}

			// assign
			for (int f = 0; f < factors.Count; f++)
				item_factors[item_id, f] = (float) factors[f];
		}

		///
		protected override float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			throw new NotImplementedException();
		}
	}
}