// Copyright (C) 2012 Zeno Gantner
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
using System.Linq;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>SVD++: Matrix factorization that also takes into account _what_ users have rated</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren:
	///         Factorization Meets the Neighborhood: a Multifaceted Collaborative Filtering Model.
	///         KDD 2008.
	///         http://research.yahoo.com/files/kdd08koren.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class SVDPlusPlus : MatrixFactorization
	{
		// TODO
		// - LoadModel/SaveModel
		// - implement ComputeObjective
		// - handle fold-in
		// - use knowledge of which items will have to be rated ...
		// - implement also with fixed biases (progress prize 2008)
		// - implement integrated model (section 5 of the KDD 2008 paper)
		// - try bold driver heuristics
		// - try learning rate decay
		// - try rating-based weights from http://recsyswiki.com/wiki/SVD%2B%2B
		// - try frequency regularization
		// - try different learn rates/regularization for user and item parameters
		// - try version with sigmoid function
		// - implement parallel learning
		// - implement ALS learning

		float[] user_bias;
		float[] item_bias;
		Matrix<float> y;
		Matrix<float> p;

		int[][] items_rated_by_user;

		/// <summary>bias learn rate</summary>
		public float BiasLearnRate { get; set; }
		/// <summary>regularization constant for biases</summary>
		public float BiasReg { get; set; }

		/// <summary>Default constructor</summary>
		public SVDPlusPlus() : base()
		{
			Regularization = 0.015f;
			LearnRate = 0.001f;
			BiasLearnRate = 0.007f;
			BiasReg = 0.005f;
		}

		///
		public override void Train()
		{
			items_rated_by_user = new int[MaxUserID + 1][];
			for (int u = 0; u <= MaxUserID; u++)
				items_rated_by_user[u] = (from index in ratings.ByUser[u] select ratings.Items[index]).ToArray();

			base.Train();
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			double result = global_bias;

			if (user_id <= MaxUserID)
				result += user_bias[user_id];
			if (item_id <= MaxItemID)
				result += item_bias[item_id];
			if (user_id <= MaxUserID && item_id <= MaxItemID)
				result += MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;

			return (float) result;
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			p = new Matrix<float>(MaxUserID + 1, NumFactors);
			p.InitNormal(InitMean, InitStdDev);
			y = new Matrix<float>(MaxItemID + 1, NumFactors);
			y.InitNormal(InitMean, InitStdDev);

			user_bias = new float[MaxUserID + 1];
			item_bias = new float[MaxItemID + 1];
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			float reg = Regularization; // to limit property accesses
			float lr  = LearnRate;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double prediction = global_bias + user_bias[u] + item_bias[i];
				var u_plus_y_sum_vector = new double[NumFactors];
				foreach (int other_item_id in items_rated_by_user[u])
					for (int f = 0; f < u_plus_y_sum_vector.Length; f++) // TODO vectorize
						u_plus_y_sum_vector[f] += y[other_item_id, f];
				double norm_denominator = Math.Sqrt(ratings.CountByUser[u]);
				for (int f = 0; f < u_plus_y_sum_vector.Length; f++) // TODO get rid of this loop?
					u_plus_y_sum_vector[f] = u_plus_y_sum_vector[f] / norm_denominator + p[u, f];

				prediction += MatrixExtensions.RowScalarProduct(item_factors, i, u_plus_y_sum_vector);

				double err = ratings[index] - prediction;

				// adjust biases
				if (update_user)
					user_bias[u] += BiasLearnRate * ((float) err - BiasReg * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * ((float) err - BiasReg * item_bias[i]);

				// adjust factors -- TODO vectorize
				double x = err / norm_denominator; // TODO better name than x
				for (int f = 0; f < NumFactors; f++)
				{
					float i_f = item_factors[i, f];

					// if necessary, compute and apply updates
					if (update_user)
					{
						double delta_u = err * i_f - reg * p[u, f];
						p.Inc(u, f, lr * delta_u);
					}
					if (update_item)
					{
						double delta_i = err * u_plus_y_sum_vector[f] - reg * i_f;
						item_factors.Inc(i, f, lr * delta_i);

						double common_update = x * i_f;
						foreach (int other_item_id in items_rated_by_user[u])
						{
							double delta_oi = common_update - reg * y[other_item_id, f];
							y.Inc(other_item_id, f, lr * delta_oi);
						}
					}
				}
			}

			// pre-compute complete user factors
			for (int u = 0; u <= MaxUserID; u++)
				PrecomputeFactors(u);
		}

		void PrecomputeFactors(int u)
		{
			// compute
			var factors = new double[NumFactors];
			for (int f = 0; f < factors.Length; f++)
				factors[f] = p[u, f];
			double norm_denominator = Math.Sqrt(ratings.CountByUser[u]);
			foreach (int other_item_id in items_rated_by_user[u])
				for (int f = 0; f < factors.Length; f++) // TODO vectorize
					factors[f] += y[other_item_id, f] / norm_denominator;

			// assign
			for (int f = 0; f < factors.Length; f++)
				user_factors[u, f] = (float) factors[f];
		}

		/// <summary>Updates the latent factors on a user</summary>
		/// <param name="user_id">the user ID</param>
		public override void RetrainUser(int user_id)
		{
			if (UpdateUsers)
			{
				base.RetrainUser(user_id);
				PrecomputeFactors(user_id);
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} regularization={2} bias_reg={3} learn_rate={4} bias_learn_rate={5} num_iter={6} init_mean={7} init_stddev={8}",
				this.GetType().Name, NumFactors, Regularization, BiasReg, LearnRate,  BiasLearnRate, NumIter, InitMean, InitStdDev);
		}
	}
}

