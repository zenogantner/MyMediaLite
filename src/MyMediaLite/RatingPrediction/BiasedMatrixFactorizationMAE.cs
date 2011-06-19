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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>BiasedMatrixFactorization optimized for MAE instead of RMSE</summary>
	public class BiasedMatrixFactorizationMAE : BiasedMatrixFactorization
	{
		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRating - MinRating;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double dot_product = user_bias[u] + item_bias[i] + MatrixUtils.RowScalarProduct(user_factors, u, item_factors, i);
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

				double p = MinRating + sig_dot * rating_range_size;
				double err = ratings[index] - p;

				// the only difference to RMSE optimization is here:
				double gradient_common = Math.Sign(err) * sig_dot * (1 - sig_dot) * rating_range_size;

				// adjust biases
				if (update_user)
					user_bias[u] += LearnRate * (user_bias[u] * gradient_common - BiasReg * user_bias[u]);
				if (update_item)
					item_bias[i] += LearnRate * (item_bias[i] * gradient_common - BiasReg * item_bias[i]);

				// adjust latent factors
				for (int f = 0; f < NumFactors; f++)
				{
				 	double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					if (update_user)
					{
						double delta_u = i_f * gradient_common - RegU * u_f;
						MatrixUtils.Inc(user_factors, u, f, LearnRate * delta_u);
					}
					if (update_item)
					{
						double delta_i = u_f * gradient_common - RegI * i_f;
						MatrixUtils.Inc(item_factors, i, f, LearnRate * delta_i);
					}
				}
			}
		}

		///
		public override double ComputeLoss()
		{
			double mae = 0;
			for (int i = 0; i < ratings.Count; i++)
			{
				int user_id = ratings.Users[i];
				int item_id = ratings.Items[i];
				mae += Math.Abs(Predict(user_id, item_id) - ratings[i]);
			}

			double complexity = 0;
			for (int u = 0; u <= MaxUserID; u++)
			{
				complexity += ratings.CountByUser[u] * RegU * Math.Pow(VectorUtils.EuclideanNorm(user_factors.GetRow(u)), 2);
				complexity += ratings.CountByUser[u] * BiasReg * Math.Pow(user_bias[u], 2);
			}

			for (int i = 0; i <= MaxItemID; i++)
			{
				complexity += ratings.CountByItem[i] * RegI * Math.Pow(VectorUtils.EuclideanNorm(item_factors.GetRow(i)), 2);
				complexity += ratings.CountByItem[i] * BiasReg * Math.Pow(item_bias[i], 2);
			}

			return mae + complexity;
		}

		///
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
								 "BiasedMatrixFactorizationMAE num_factors={0} bias_reg={1} reg_u={2} reg_i={3} learn_rate={4} num_iter={5} bold_driver={6} init_mean={7} init_stdev={8}",
								 NumFactors, BiasReg, RegU, RegI, LearnRate, NumIter, BoldDriver, InitMean, InitStdev);
		}
	}
}

