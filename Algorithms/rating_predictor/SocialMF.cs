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
		
		/// <inheritdoc />
		protected override void Iterate(Ratings ratings, bool update_user, bool update_item)
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

				double r = rating.rating;
                double p = MinRatingValue + sig_dot * rating_range_size;
				double err = r - p;

				double gradient_common = err * sig_dot * (1 - sig_dot) * rating_range_size;

				// compute social regularization part
				// (simplified)
				double[] sum_neighbor  = new double[num_features];
				int      num_neighbors = user_neighbors[u].Count;
				foreach (int v in user_neighbors[u])
                	for (int f = 2; f < num_features; f++) // ignore fixed/bias parts
						sum_neighbor[f] += user_feature[v, f];
				double social_penalty = 0;
				for (int f = 2; f < num_features; f++) // ignore fixed/bias parts
					social_penalty += user_feature[u, f] - sum_neighbor[f] / num_neighbors;
				
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
							delta_u -= social_regularization * social_penalty;
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
			                     "SocialMF num_features={0} regularization={1} social_regularization={2} learn_rate={3} num_iter={4} init_mean={5} init_stdev={6}",
				                 NumFeatures, Regularization, LearnRate, NumIter, InitMean, InitStdev);
		}
	}
}
