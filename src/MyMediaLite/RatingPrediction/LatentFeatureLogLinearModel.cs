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
	/// <summary>Latent-feature log linear model</summary>
	/// <remarks>
	///   <para>
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
	public class LatentFeatureLogLinearModel : BiasedMatrixFactorization
	{
		// base category does not need biases and latent factors
		// second category is represented by the inherited parameters
		// all further categories are represented by the following structure
		IList<Matrix<float>> additional_user_factors;
		IList<Matrix<float>> additional_item_factors;
		IList<float> additional_user_biases;
		IList<float> additional_item_biases;
		
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
					user_bias[u] += BiasLearnRate * LearnRate * (gradient_common - BiasReg * RegU * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * LearnRate * (gradient_common - BiasReg * RegI * item_bias[i]);

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
		public override float ComputeObjective()
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

			return (float) (loss + complexity);
		}
		
		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1)
				throw new ArgumentException("Unknown user ID");
			if (item_id >= item_factors.dim1)
				throw new ArgumentException("Unknown item ID");

			double score = user_bias[user_id] + item_bias[item_id] + MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			return (float) (MinRating + ( 1 / (1 + Math.Exp(-score)) ) * (MaxRating - MinRating));
		}
		
		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} learn_rate={5} bias_learn_rate={6} num_iter={7} bold_driver={8} init_mean={9} init_stddev={10}",
				this.GetType().Name, NumFactors, BiasReg, RegU, RegI, LearnRate, BiasLearnRate, NumIter, BoldDriver, InitMean, InitStdDev);
		}
	}
}