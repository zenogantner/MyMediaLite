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
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class BiasedMatrixFactorization : MatrixFactorization
	{
		// TODO make one MF class for both ItemRecommender and RatingPredictor (which need to be turned into interfaces for that ...)
		// TODO think about de-activating/separating regularization for the user and item bias

		/// <inheritdoc />
        protected override void _Init()
		{
			base._Init();
			if (num_features < 2)
				throw new ArgumentException("num_features must be >= 2");
        	this.user_feature.SetColumnToOneValue(0, 1);
			this.item_feature.SetColumnToOneValue(1, 1);
		}

		/// <inheritdoc />
		protected override void Iterate(Ratings ratings, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRatingValue - MinRatingValue;

			foreach (RatingEvent rating in ratings)
            {
            	int u = rating.user_id;
                int i = rating.item_id;

				double dot_product = 0;
	            for (int f = 0; f < num_features; f++)
    	            dot_product += user_feature.Get(u, f) * item_feature.Get(i, f);
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

				double r = rating.rating;
                double p = MinRatingValue + sig_dot * rating_range_size;
				double err = r - p;

				double gradient_common = err * sig_dot * (1 - sig_dot) * rating_range_size;

				// Adjust features
                for (int f = 0; f < num_features; f++)
                {
                 	double u_f = user_feature.Get(u, f);
                    double i_f = item_feature.Get(i, f);

                    if (update_user && f != 0)
					{
						double delta_u = gradient_common * i_f;
						if (f != 1)
							delta_u -= regularization * u_f;
						MatrixUtils.Inc(user_feature, u, f, learn_rate * delta_u);
					}
                    if (update_item && f != 1)
					{
						double delta_i = gradient_common * u_f;
						if (f != 0)
							delta_i -= regularization * i_f;
						MatrixUtils.Inc(item_feature, i, f, learn_rate * delta_i);
					}
                }
            }
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (user_id >= user_feature.dim1)
				return bias;
            if (item_id >= item_feature.dim1)
				return bias;

            double dot_product = 0;

            // U*V
            for (int f = 0; f < num_features; f++)
                dot_product += user_feature.Get(user_id, f) * item_feature.Get(item_id, f);

			double result = MinRatingValue + ( 1 / (1 + Math.Exp(-dot_product)) ) * (MaxRatingValue - MinRatingValue);

            return result;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("biased-matrix-factorization num_features={0}, regularization={1}, learn_rate={2}, num_iter={3}, init_f_mean={4}, init_f_stdev={5}",
				                 num_features, regularization, learn_rate, num_iter, init_f_mean, init_f_stdev);
		}
	}
}
