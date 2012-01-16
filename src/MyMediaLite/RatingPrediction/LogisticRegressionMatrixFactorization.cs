// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Matrix factorization with explicit user and item bias, learning is performed by stochastic gradient descent, optimized for the log likelihood</summary>
	/// <remarks>
	///   <para>
	///   Implements a simple version Menon and Elkan's LFL model:
	///   Predicts binary labels, no advanced regularization, no side information.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Aditya Krishna Menon, Charles Elkan:
	///         A log-linear model with latent features for dyadic prediction.
	///         ICDM 2010.
	///         http://cseweb.ucsd.edu/~akmenon/LFL-ICDM10.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class LogisticRegressionMatrixFactorization : BiasedMatrixFactorization
	{
		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRating - MinRating;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double dot_product = user_bias[u] + item_bias[i] + MatrixExtensions.RowScalarProduct(user_factors, u, item_factors, i);
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

				float prediction = (float) (min_rating + sig_dot * rating_range_size);

				float gradient_common = ratings[index] - prediction;

				// adjust biases
				if (update_user)
					user_bias[u] += LearnRate * (gradient_common - BiasReg * RegU * user_bias[u]);
				if (update_item)
					item_bias[i] += LearnRate * (gradient_common - BiasReg * RegI * item_bias[i]);

				// adjust latent factors
				for (int f = 0; f < NumFactors; f++)
				{
				 	double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					if (update_user)
					{
						double delta_u = gradient_common * i_f - RegU * u_f;
						user_factors.Inc(u, f, LearnRate * delta_u);
						// this is faster (190 vs. 260 seconds per iteration on Netflix w/ k=30) than
						//    user_factors[u, f] += learn_rate * delta_u;
					}
					if (update_item)
					{
						double delta_i = gradient_common * u_f - RegI * i_f;
						item_factors.Inc(i, f, LearnRate * delta_i);
					}
				}
			}
		}

		///
		public override double ComputeLoss()
		{
			double rating_range_size = MaxRating - MinRating;

			double loss = 0;
			for (int i = 0; i < ratings.Count; i++)
			{
				double prediction = Predict(ratings.Users[i], ratings.Items[i]);

				// map into [0, 1] interval
				prediction = (prediction - MinRating) / rating_range_size;
				if (prediction < 0.0)
					prediction = 0.0;
				if (prediction > 1.0)
					prediction = 1.0;
				double actual_rating = (ratings[i] - MinRating) / rating_range_size;

				loss -= (actual_rating) * Math.Log(prediction);
				loss -= (1 - actual_rating) * Math.Log(1 - prediction);
			}

			double complexity = 0;
			for (int u = 0; u <= MaxUserID; u++)
			{
				complexity += ratings.CountByUser[u] * RegU * Math.Pow(user_factors.GetRow(u).EuclideanNorm(), 2);
				complexity += ratings.CountByUser[u] * BiasReg * Math.Pow(user_bias[u], 2);
			}
			for (int i = 0; i <= MaxItemID; i++)
			{
				complexity += ratings.CountByItem[i] * RegI * Math.Pow(item_factors.GetRow(i).EuclideanNorm(), 2);
				complexity += ratings.CountByItem[i] * BiasReg * Math.Pow(item_bias[i], 2);
			}

			return loss + complexity;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} learn_rate={5} num_iter={6} bold_driver={7} init_mean={8} init_stddev={9}",
				this.GetType().Name, NumFactors, BiasReg, RegU, RegI, LearnRate, NumIter, BoldDriver, InitMean, InitStdDev);
		}
	}
}