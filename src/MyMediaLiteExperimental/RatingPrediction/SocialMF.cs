// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
// Copyright (C) 2011 Zeno Gantner
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
	/// This implementation assumes a binary and symmetrical trust network.
	///
	/// Mohsen Jamali, Martin Ester:
    /// A matrix factorization technique with trust propagation for recommendation in social networks
    /// RecSys '10: Proceedings of the Fourth ACM Conference on Recommender Systems, 2010
	/// </remarks>
	public class SocialMF : BiasedMatrixFactorization, IUserRelationAwareRecommender
	{
        /// <summary>Social network regularization constant</summary>
		public double SocialRegularization { get { return social_regularization; } set { social_regularization = value; } }
        private double social_regularization = 1;

		/*
		/// <summary>
		/// Use stochastic gradient descent instead of batch gradient descent
		/// </summary>
		public bool StochasticLearning { get; set; }
		*/

		///
		public SparseBooleanMatrix UserRelation { get { return this.user_neighbors; } set {	this.user_neighbors = value; } }
		private SparseBooleanMatrix user_neighbors;

		/// <summary>the number of users</summary>
		public int NumUsers { get { return MaxUserID + 1; } }

		///
		protected override void InitModel()
		{
			base.InitModel();
			this.MaxUserID = Math.Max(MaxUserID, user_neighbors.NumberOfRows - 1);
			this.MaxUserID = Math.Max(MaxUserID, user_neighbors.NumberOfColumns - 1);

			// init latent factor matrices
	       	user_factors = new Matrix<double>(NumUsers, NumFactors);
	       	item_factors = new Matrix<double>(ratings.MaxItemID + 1, NumFactors);
	       	MatrixUtils.RowInitNormal(user_factors, InitMean, InitStdev);
	       	MatrixUtils.RowInitNormal(item_factors, InitMean, InitStdev);
			// init biases
			user_bias = new double[NumUsers];
			item_bias = new double[ratings.MaxItemID + 1];
		}

		///
        public override void Train()
		{
			InitModel();

			Console.Error.WriteLine("num_users={0}, num_items={1}", NumUsers, item_bias.Length);

			// compute global average
			double global_average = 0;
			global_average = Ratings.Average;

			// learn model parameters
            global_bias = Math.Log( (global_average - MinRating) / (MaxRating - global_average) );
            for (int current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate(ratings.RandomIndex, true, true);
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			// We ignore the method's arguments.
			IterateBatch();
		}

		private void IterateBatch()
		{
			// I. compute gradients
			var user_factors_gradient = new Matrix<double>(user_factors.dim1, user_factors.dim2);
			var item_factors_gradient = new Matrix<double>(item_factors.dim1, item_factors.dim2);
			var user_bias_gradient    = new double[user_factors.dim1];
			var item_bias_gradient    = new double[item_factors.dim1];

			// I.1 prediction error
			double rating_range_size = MaxRating - MinRating;
			for (int index = 0; index < ratings.Count; index++)
            {
            	int u = ratings.Users[index];
                int i = ratings.Items[index];

				// prediction
				double score = global_bias;
				score += user_bias[u];
				score += item_bias[i];
	            for (int f = 0; f < NumFactors; f++)
    	            score += user_factors[u, f] * item_factors[i, f];
				double sig_score = 1 / (1 + Math.Exp(-score));

                double prediction = MinRating + sig_score * rating_range_size;
				double error      = ratings[index] - prediction;

				double gradient_common = error * sig_score * (1 - sig_score) * rating_range_size;

				// add up error gradient
                for (int f = 0; f < NumFactors; f++)
                {
                 	double u_f = user_factors[u, f];
                    double i_f = item_factors[i, f];

                    if (f != 0)
						MatrixUtils.Inc(user_factors_gradient, u, f, gradient_common * i_f);
                    if (f != 1)
						MatrixUtils.Inc(item_factors_gradient, i, f, gradient_common * u_f);
                }
			}

			// I.2 L2 regularization
			//        biases
			for (int u = 0; u < user_bias_gradient.Length; u++)
				user_bias_gradient[u] += user_bias[u] * Regularization;
			for (int i = 0; i < item_bias_gradient.Length; i++)
				item_bias_gradient[i] += item_bias[i] * Regularization;
			//        latent factors
			for (int u = 0; u < user_factors_gradient.dim1; u++)
				for (int f = 2; f < NumFactors; f++)
					MatrixUtils.Inc(user_factors_gradient, u, f, user_factors[u, f] * Regularization);

			for (int i = 0; i < item_factors_gradient.dim1; i++)
				for (int f = 2; f < NumFactors; f++)
					MatrixUtils.Inc(item_factors_gradient, i, f, item_factors[i, f] * Regularization);

			// I.3 social network regularization
			for (int u = 0; u < user_factors_gradient.dim1; u++)
			{
				// see eq. (13) in the paper
				double[] sum_neighbors    = new double[NumFactors];
				double bias_sum_neighbors = 0;
				int      num_neighbors    = user_neighbors[u].Count;

				// user bias part
				foreach (int v in user_neighbors[u])
					bias_sum_neighbors += user_bias[v];
				if (num_neighbors != 0)
					user_bias_gradient[u] += social_regularization * (user_bias[u] - bias_sum_neighbors / num_neighbors);
				foreach (int v in user_neighbors[u])
					if (user_neighbors[v].Count != 0)
					{
						double trust_v = (double) 1 / user_neighbors[v].Count;
						double diff = 0;
						foreach (int w in user_neighbors[v])
							diff -= user_bias[w];

						diff = diff * trust_v;
						diff += user_bias[v];

						if (num_neighbors != 0)
							user_bias_gradient[u] -= social_regularization * trust_v * diff / num_neighbors;
					}

				// latent factor part
				foreach (int v in user_neighbors[u])
                	for (int f = 0; f < NumFactors; f++)
						sum_neighbors[f] += user_factors[v, f];
				if (num_neighbors != 0)
					for (int f = 0; f < NumFactors; f++)
						MatrixUtils.Inc(user_factors_gradient, u, f, social_regularization * (user_factors[u, f] - sum_neighbors[f] / num_neighbors));
				foreach (int v in user_neighbors[u])
					if (user_neighbors[v].Count != 0)
					{
						double trust_v = (double) 1 / user_neighbors[v].Count;
						for (int f = 0; f < NumFactors; f++)
						{
							double diff = 0;
							foreach (int w in user_neighbors[v])
								diff -= user_factors[w, f];
							diff = diff * trust_v;
							diff += user_factors[v, f];
							if (num_neighbors != 0)
								MatrixUtils.Inc(user_factors_gradient, u, f, -social_regularization * trust_v * diff / num_neighbors);
						}
					}
			}

			// II. apply gradient descent step
			for (int u = 0; u < user_factors_gradient.dim1; u++)
			{
				user_bias[u] += user_bias_gradient[u] * LearnRate;
				for (int f = 2; f < NumFactors; f++)
					MatrixUtils.Inc(user_factors, u, f, user_factors_gradient[u, f] * LearnRate);
			}
			for (int i = 0; i < item_factors_gradient.dim1; i++)
			{
				item_bias[i] += item_bias_gradient[i] * LearnRate;
				for (int f = 2; f < NumFactors; f++)
					MatrixUtils.Inc(item_factors, i, f, item_factors_gradient[i, f] * LearnRate);
			}
		}

		///
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
			                     "SocialMF num_factors={0} regularization={1} social_regularization={2} learn_rate={3} num_iter={4} init_mean={5} init_stdev={6}",
				                 NumFactors, Regularization, SocialRegularization, LearnRate, NumIter, InitMean, InitStdev);
		}
	}
}
