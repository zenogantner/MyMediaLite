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
	/// Matrix factorization engine with explicit user and item bias.
	/// </summary>
	public class BiasedMatrixFactorization : MatrixFactorization
	{
		/// <summary>
		/// Regularization constant for the bias terms
		/// </summary>
		public double BiasRegularization { get { return bias_regularization; } set { bias_regularization = value; } }
		double bias_regularization = 0;

		double[] user_bias;
		double[] item_bias;
		
		/// <inheritdoc />
        public override void Train()
		{
			// init feature matrices
	       	user_feature = new Matrix<double>(MaxUserID + 1, num_features);
	       	item_feature = new Matrix<double>(MaxItemID + 1, num_features);
	       	MatrixUtils.InitNormal(user_feature, InitMean, InitStdev);
	       	MatrixUtils.InitNormal(item_feature, InitMean, InitStdev);

			// TODO use a Vector datatype
			user_bias = new double[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				user_bias[u] = MyMediaLite.util.Random.GetInstance().NextNormal(InitMean, InitStdev);
			item_bias = new double[MaxItemID + 1];
			for (int i = 0; i <= MaxItemID; i++)
				item_bias[i] = MyMediaLite.util.Random.GetInstance().NextNormal(InitMean, InitStdev);
			
            // learn model parameters
			ratings.Shuffle(); // avoid effects e.g. if rating data is sorted by user or item
			
			// compute global average
			double global_average = 0;
			foreach (RatingEvent r in Ratings.All)
				global_average += r.rating;
			global_average /= Ratings.All.Count;

			// TODO also learn global bias?
            global_bias = Math.Log( (global_average - MinRating) / (MaxRating - global_average) );
            for (int current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate(ratings.All, true, true);
		}

		/// <inheritdoc />
		protected override void Iterate(Ratings ratings, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRating - MinRating;

			foreach (RatingEvent rating in ratings)
            {
            	int u = rating.user_id;
                int i = rating.item_id;

				double dot_product = global_bias + user_bias[u] + item_bias[i];
	            for (int f = 0; f < num_features; f++)
    	            dot_product += user_feature[u, f] * item_feature[i, f];
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

                double p = MinRating + sig_dot * rating_range_size;
				double err = rating.rating - p;

				double gradient_common = err * sig_dot * (1 - sig_dot) * rating_range_size;

				// Adjust biases
				if (update_user)
					user_bias[u] += learn_rate * (gradient_common - bias_regularization * user_bias[u]);
				if (update_item)
					item_bias[i] += learn_rate * (gradient_common - bias_regularization * item_bias[i]);
				
				// Adjust latent features
                for (int f = 0; f < num_features; f++)
                {
                 	double u_f = user_feature[u, f];
                    double i_f = item_feature[i, f];

                    if (update_user)
					{
						double delta_u = gradient_common * i_f - regularization * u_f;
						MatrixUtils.Inc(user_feature, u, f, learn_rate * delta_u);
						// this is faster (190 vs. 260 seconds per iteration on Netflix w/ k=30) than
						//    user_feature[u, f] += learn_rate * delta_u;
					}
                    if (update_item)
					{
						double delta_i = gradient_common * u_f - regularization * i_f;
						MatrixUtils.Inc(item_feature, i, f, learn_rate * delta_i);
						// item_feature[i, f] += learn_rate * delta_i;
					}
                }
            }
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (user_id >= user_feature.dim1 || item_id >= item_feature.dim1)
				return MinRating + ( 1 / (1 + Math.Exp(-global_bias)) ) * (MaxRating - MinRating);

			double dot_product = global_bias + user_bias[user_id] + item_bias[item_id];

            // U*V
            for (int f = 0; f < num_features; f++)
                dot_product += user_feature[user_id, f] * item_feature[item_id, f];

			return MinRating + ( 1 / (1 + Math.Exp(-dot_product)) ) * (MaxRating - MinRating);
        }

		/// <inheritdoc />
		public override string ToString()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';			
			
			return string.Format(ni,
			                     "biased-matrix-factorization num_features={0} bias_regularization={1} regularization={2} learn_rate={3} num_iter={4} init_mean={5} init_stdev={6}",
				                 NumFeatures, BiasRegularization, Regularization, LearnRate, NumIter, InitMean, InitStdev);
		}
	}
}
