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
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval.Measures
{
	/// <summary>Utility functions for the logistic loss</summary>
	public static class LogisticLoss
	{
		/// <summary>Computes the logistic loss sum</summary>
		/// <returns>the logistic loss sum</returns>
		/// <param name='recommender'>the recommender to make predictions with</param>
		/// <param name='ratings'>the actual ratings</param>
		/// <param name='min_rating'>the minimal rating</param>
		/// <param name='rating_range_size'>the size of the rating range: max_rating - min_rating</param>
		public static double ComputeSum(
			this IRatingPredictor recommender,
			IRatings ratings,
			float min_rating, float rating_range_size)
		{
			double sum = 0;
			for (int i = 0; i < ratings.Count; i++)
			{
				double prediction = recommender.Predict(ratings.Users[i], ratings.Items[i]);

				// map into [0, 1] interval
				prediction = (prediction - min_rating) / rating_range_size;
				if (prediction < 0.0)
					prediction = 0.0;
				if (prediction > 1.0)
					prediction = 1.0;
				double actual_rating = (ratings[i] - min_rating) / rating_range_size;

				sum -= (actual_rating) * Math.Log(prediction);
				sum -= (1 - actual_rating) * Math.Log(1 - prediction);
			}
			return sum;
		}
	}
}

