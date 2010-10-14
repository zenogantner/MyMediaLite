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
using System.IO;
using System.Linq;
using System.Text;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	/// <summary>
	/// Social-network-aware matrix factorization
	/// </summary>
	/// <remarks>
	///  Mohsen Jamali, Martin Ester
    ///  A matrix factorization technique with trust propagation for recommendation in social networks
    ///  RecSys '10: Proceedings of the Fourth ACM Conference on Recommender Systems, 2010
	/// </remarks>
	public class SocialMF : BiasedMatrixFactorization, UserAttributeAwareRecommender
	{
        /// <summary>Social network regularization constant</summary>
		public double SocialRegularization {
			get {
				return this.social_regularization;
			}
			set {
				social_regularization = value;
			}
		}		
        private double social_regularization = 1;		

		public bool StochasticLearning { get; set; }
		
		/// <inheritdoc />
		public SparseBooleanMatrix UserAttributes
		{
			set
			{
				this.user_neighbors = value;
				this.MaxUserID = Math.Max(MaxUserID, user_neighbors.NumberOfRows);
			}
		}
		private SparseBooleanMatrix user_neighbors;
		
		/// <inheritdoc />
		public int NumUserAttributes { get; set; }
		
		protected void Iterate(Ratings ratings, bool update_user, bool update_item)
		{
			if (StochasticLearning)
				IterateSGD(ratings, update_user, update_item);
			else
				IterateBatch(ratings);
		}
		
		private void IterateBatch(Ratings ratings)
		{
			// I. compute gradients
			Matrix<double> user_feature_gradient = new Matrix<double>(user_feature.dim1, user_feature.dim2);
			Matrix<double> item_feature_gradient = new Matrix<double>(item_feature.dim1, item_feature.dim2);

			// I.1 prediction error
			double rating_range_size = MaxRatingValue - MinRatingValue;
			foreach (RatingEvent rating in ratings)
            {
            	int u = rating.user_id;
                int i = rating.item_id;

				// prediction
				double dot_product = bias;
	            for (int f = 0; f < num_features; f++)
    	            dot_product += user_feature[u, f] * item_feature[i, f];
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));				

                double prediction = MinRatingValue + sig_dot * rating_range_size;
				double error      = rating.rating - prediction;				
				
				double gradient_common = error * sig_dot * (1 - sig_dot) * rating_range_size;
				
				// add up error gradient
                for (int f = 0; f < num_features; f++)
                {
                 	double u_f = user_feature[u, f];
                    double i_f = item_feature[i, f];

                    if (f != 0)
						MatrixUtils.Inc(user_feature_gradient, u, f, gradient_common * i_f); // TODO check whether standard matrix op works as fine ...
                    if (f != 1)
						MatrixUtils.Inc(item_feature_gradient, i, f, gradient_common * u_f);
                }
			}
			
			// I.2 L2 regularization
			for (int u = 0; u < user_feature_gradient.dim1; u++)
				for (int f = 2; f < num_features; f++)
					MatrixUtils.Inc(user_feature_gradient, u, f, user_feature[u, f] * regularization);
			
			for (int i = 0; i < item_feature_gradient.dim1; i++)
				for (int f = 2; f < num_features; f++)
					MatrixUtils.Inc(item_feature_gradient, i, f, item_feature[i, f] * regularization);
			
			// I.3 social network regularization
			for (int u = 0; u < user_feature_gradient.dim1; u++)
			{

				double[] sum_neighbor  = new double[num_features];
				int      num_neighbors = user_neighbors[u].Count;
				foreach (int v in user_neighbors[u])
                	for (int f = 2; f < num_features; f++) // ignore fixed/bias parts
						sum_neighbor[f] += user_feature[v, f];
				if (num_neighbors != 0)
					for (int f = 2; f < num_features; f++) // ignore fixed/bias parts
						user_feature_gradient[u, f] += social_regularization * (user_feature[u, f] - sum_neighbor[f] / num_neighbors);
			}		
			
			// II. apply gradient descent step
			for (int u = 0; u < user_feature_gradient.dim1; u++)
				for (int f = 2; f < num_features; f++)
					MatrixUtils.Inc(user_feature, u, f, user_feature_gradient[u, f] * -learn_rate);
			
			for (int i = 0; i < item_feature_gradient.dim1; i++)
				for (int f = 2; f < num_features; f++)
					MatrixUtils.Inc(item_feature, i, f, item_feature_gradient[i, f] * -learn_rate);
		}
		
		private void IterateSGD(Ratings ratings, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRatingValue - MinRatingValue;

			foreach (RatingEvent rating in ratings)
            {
            	int u = rating.user_id;
                int i = rating.item_id;

				double dot_product = bias;
	            for (int f = 0; f < num_features; f++)
    	            dot_product += user_feature[u, f] * item_feature[i, f];
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

                double prediction = MinRatingValue + sig_dot * rating_range_size;
				double error      = rating.rating - prediction;

				double gradient_common = error * sig_dot * (1 - sig_dot) * rating_range_size;

				// compute social regularization part
				// (simplified)
				double[] sum_neighbors  = new double[num_features];
				int      num_neighbors = user_neighbors[u].Count;
				foreach (int v in user_neighbors[u])
                	for (int f = 2; f < num_features; f++) // ignore fixed/bias parts
						sum_neighbors[f] += user_feature[v, f];
				
				// Adjust features
                for (int f = 0; f < num_features; f++)
                {
                 	double u_f = user_feature[u, f];
                    double i_f = item_feature[i, f];

                    if (update_user && f != 0)
					{
						double delta_u = gradient_common * i_f;
						if (f != 1) // do not regularize user bias
						{
							delta_u -= regularization * u_f;
							if (num_neighbors != 0)
								delta_u -= social_regularization * sum_neighbors[f] / num_neighbors;
						}
						MatrixUtils.Inc(user_feature, u, f, learn_rate * delta_u);
					}
                    if (update_item && f != 1)
					{
						double delta_i = gradient_common * u_f;
						if (f != 0)  // do not regularize item bias
							delta_i -= regularization * i_f;
						MatrixUtils.Inc(item_feature, i, f, learn_rate * delta_i);
					}
                }
            }
		}

		/// <inheritdoc />
		public override string ToString()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';			
			
			return String.Format(ni,
			                     "SocialMF num_features={0} regularization={1} social_regularization={2} learn_rate={3} num_iter={4} stochastic={5} init_mean={6} init_stdev={7}",
				                 NumFeatures, Regularization, SocialRegularization, LearnRate, NumIter, StochasticLearning, InitMean, InitStdev);
		}
	}
}
