// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System;
using System.Collections.Generic;
using System.Globalization;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Social-network-aware matrix factorization</summary>
	/// <remarks>
	///   <para>
	///     This implementation expects a binary trust network.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Mohsen Jamali, Martin Ester:
	///         A matrix factorization technique with trust propagation for recommendation in social networks
	///         RecSys 2010
	///         http://portal.acm.org/citation.cfm?id=1864736
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class SocialMF : BiasedMatrixFactorization, IUserRelationAwareRecommender
	{
		/// <summary>Social network regularization constant</summary>
		public float SocialRegularization { get { return social_regularization; } set { social_regularization = value; } }
		private float social_regularization = 1;

		///
		public IBooleanMatrix UserRelation { get { return this.user_connections; } set { this.user_connections = value; } }
		private IBooleanMatrix user_connections;

		/// <summary>the number of users</summary>
		public int NumUsers { get { return MaxUserID + 1; } }

		///
		protected internal override void InitModel()
		{
			if (user_connections == null)
			{
				user_connections = new SparseBooleanMatrix();
				Console.Error.WriteLine("Warning: UserRelation not set.");
			}

			this.MaxUserID = Math.Max(MaxUserID, user_connections.NumberOfRows - 1);
			this.MaxUserID = Math.Max(MaxUserID, user_connections.NumberOfColumns - 1);

			base.InitModel();
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			IterateBatch(rating_indices, update_user, update_item);
		}

		private void IterateBatch(IList<int> rating_indices, bool update_user, bool update_item)
		{
			SetupLoss();
			SparseBooleanMatrix user_reverse_connections = (SparseBooleanMatrix) user_connections.Transpose();

			// I. compute gradients
			var user_factors_gradient = new Matrix<float>(user_factors.dim1, user_factors.dim2);
			var item_factors_gradient = new Matrix<float>(item_factors.dim1, item_factors.dim2);
			var user_bias_gradient    = new float[user_factors.dim1];
			var item_bias_gradient    = new float[item_factors.dim1];

			// I.1 prediction error
			foreach (int index in rating_indices)
			{
				int user_id = ratings.Users[index];
				int item_id = ratings.Items[index];

				// prediction
				float score = global_bias + user_bias[user_id] + item_bias[item_id];
				score += DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);
				double sig_score = 1 / (1 + Math.Exp(-score));

				float prediction = (float) (MinRating + sig_score * rating_range_size);
				float error      = prediction - ratings[index];

				float gradient_common = compute_gradient_common(sig_score, error);

				user_bias_gradient[user_id] += gradient_common;
				item_bias_gradient[item_id] += gradient_common;

				for (int f = 0; f < NumFactors; f++)
				{
					float u_f = user_factors[user_id, f];
					float i_f = item_factors[item_id, f];

					user_factors_gradient.Inc(user_id, f, gradient_common * i_f);
					item_factors_gradient.Inc(item_id, f, gradient_common * u_f);
				}
			}

			// I.2 L2 regularization
			//        biases
			for (int u = 0; u < user_bias_gradient.Length; u++)
				user_bias_gradient[u] += user_bias[u] * RegU * BiasReg;
			for (int i = 0; i < item_bias_gradient.Length; i++)
				item_bias_gradient[i] += item_bias[i] * RegI * BiasReg;
			//        latent factors
			for (int u = 0; u < user_factors_gradient.dim1; u++)
				for (int f = 0; f < user_factors_gradient.dim2; f++)
					user_factors_gradient.Inc(u, f, user_factors[u, f] * RegU);

			for (int i = 0; i < item_factors_gradient.dim1; i++)
				for (int f = 0; f < item_factors_gradient.dim2; f++)
					item_factors_gradient.Inc(i, f, item_factors[i, f] * RegI);

			// I.3 social network regularization -- see eq. (13) in the paper
			if (SocialRegularization != 0)
				for (int u = 0; u < user_factors_gradient.dim1; u++)
				{
					var sum_connections        = new float[NumFactors];
					float bias_sum_connections = 0;
					int num_connections        = user_connections[u].Count;
					foreach (int v in user_connections[u])
					{
						bias_sum_connections += user_bias[v];
						for (int f = 0; f < sum_connections.Length; f++)
							sum_connections[f] += user_factors[v, f];
					}
					if (num_connections != 0)
					{
						user_bias_gradient[u] += social_regularization * (user_bias[u] - bias_sum_connections / num_connections);
						for (int f = 0; f < user_factors_gradient.dim2; f++)
							user_factors_gradient.Inc(u, f, social_regularization * (user_factors[u, f] - sum_connections[f] / num_connections));
					}

					foreach (int v in user_reverse_connections[u])
					{
						float trust_v = (float) 1 / user_connections[v].Count;
						float neg_trust_times_reg = -social_regularization * trust_v;

						float bias_diff = 0;
						var factor_diffs = new float[NumFactors];
						foreach (int w in user_connections[v])
						{
							bias_diff -= user_bias[w];
							for (int f = 0; f < factor_diffs.Length; f++)
								factor_diffs[f] -= user_factors[w, f];
						}

						bias_diff *= trust_v; // normalize
						bias_diff += user_bias[v];
						user_bias_gradient[u] += neg_trust_times_reg * bias_diff;

						for (int f = 0; f < factor_diffs.Length; f++)
						{
							factor_diffs[f] *= trust_v; // normalize
							factor_diffs[f] += user_factors[v, f];
							user_factors_gradient.Inc(u, f, neg_trust_times_reg * factor_diffs[f]);
						}
					}
				}

			// II. apply gradient descent step
			if (update_user)
			{
				for (int user_id = 0; user_id < user_factors_gradient.dim1; user_id++)
					user_bias[user_id] -= user_bias_gradient[user_id] * LearnRate * BiasLearnRate;
				user_factors_gradient.Multiply(-LearnRate);
				user_factors.Inc(user_factors_gradient);
			}
			if (update_item)
			{
				for (int item_id = 0; item_id < item_factors_gradient.dim1; item_id++)
					item_bias[item_id] -= item_bias_gradient[item_id] * LearnRate * BiasLearnRate;
				item_factors_gradient.Multiply(-LearnRate);
				item_factors.Inc(item_factors_gradient);
			}
		}

		///
		public override float ComputeObjective()
		{
			double user_complexity = 0;
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				if (ratings.CountByUser.Count > user_id)
				{
					user_complexity += Math.Pow(VectorExtensions.EuclideanNorm(user_factors.GetRow(user_id)), 2);
					user_complexity += BiasReg * Math.Pow(user_bias[user_id], 2);
				}
			double item_complexity = 0;
			for (int item_id = 0; item_id <= MaxItemID; item_id++)
				if (ratings.CountByItem.Count > item_id)
				{
					item_complexity += Math.Pow(VectorExtensions.EuclideanNorm(item_factors.GetRow(item_id)), 2);
					item_complexity += BiasReg * Math.Pow(item_bias[item_id], 2);
				}
			double complexity = RegU * user_complexity + RegI * item_complexity;

			double social_regularization = 0;
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
			{
				double bias_diff = 0;
				var factor_diffs = new double[NumFactors];
				foreach (int v in user_connections[user_id])
				{
					bias_diff -= user_bias[v];
					for (int f = 0; f < factor_diffs.Length; f++)
						factor_diffs[f] -= user_factors[v, f];
				}

				if (user_connections[user_id].Count > 0)
					bias_diff /= user_connections[user_id].Count;
				bias_diff += user_bias[user_id];
				social_regularization += Math.Pow(bias_diff, 2);

				for (int f = 0; f < factor_diffs.Length; f++)
				{
					if (user_connections[user_id].Count > 0)
						factor_diffs[f] /= user_connections[user_id].Count;
					factor_diffs[f] += user_factors[user_id, f];
					social_regularization += Math.Pow(factor_diffs[f], 2);
				}
			}
			social_regularization *= this.social_regularization;

			return (float) (ComputeLoss() + complexity + social_regularization);
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} reg_u={2} reg_i={3} bias_reg={4} social_regularization={5} learn_rate={6} bias_learn_rate={7} num_iter={8} bold_driver={9} loss={10}",
				this.GetType().Name, NumFactors, RegU, RegI, BiasReg, SocialRegularization, LearnRate, BiasLearnRate, NumIter, BoldDriver, Loss);
		}
	}
}
