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
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class for rating prediction</summary>
	public static class Ratings
	{
		/// <summary>the evaluation measures for rating prediction offered by the class</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Root_mean_square_error and http://recsyswiki.com/wiki/Mean_absolute_error
		/// </remarks>
		static public ICollection<string> Measures
		{
			get	{
				string[] measures = { "RMSE", "MAE", "NMAE" };
				return new HashSet<string>(measures);
			}
		}

		/// <summary>Format rating prediction results</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Root_mean_square_error and http://recsyswiki.com/wiki/Mean_absolute_error
		/// </remarks>
		/// <param name="result">the result dictionary</param>
		/// <returns>a string containing the results</returns>
		static public string FormatResults(Dictionary<string, double> result)
		{
			return string.Format(
				CultureInfo.InvariantCulture, "RMSE {0:0.#####} MAE {1:0.#####} NMAE {2:0.#####}",
				result["RMSE"], result["MAE"], result["NMAE"]
			);
		}

		/// <summary>Evaluates a rating predictor for RMSE, MAE, and NMAE</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Root_mean_square_error and http://recsyswiki.com/wiki/Mean_absolute_error
		///
		/// For NMAE, see "Eigentaste: A Constant Time Collaborative Filtering Algorithm" by Goldberg et al.
		/// 
		/// If the recommender can take time into account, and the rating dataset provides rating times,
		/// this information will be used for making rating predictions.
		/// </remarks>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">Test cases</param>
		/// <returns>a Dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(this IRatingPredictor recommender, IRatings ratings)
		{
			double rmse = 0;
			double mae  = 0;

			if (recommender == null)
				throw new ArgumentNullException("recommender");
			if (ratings == null)
				throw new ArgumentNullException("ratings");

			if (recommender is ITimeAwareRatingPredictor && ratings is ITimedRatings)
				for (int index = 0; index < ratings.Count; index++)
				{
					var time_aware_recommender = recommender as ITimeAwareRatingPredictor;
					var timed_ratings = ratings as ITimedRatings;
					double error = time_aware_recommender.Predict(timed_ratings.Users[index], timed_ratings.Items[index], timed_ratings.Times[index]) - timed_ratings[index];
					rmse += error * error;
					mae  += Math.Abs(error);
				}
			else
				for (int index = 0; index < ratings.Count; index++)
				{
					double error = recommender.Predict(ratings.Users[index], ratings.Items[index]) - ratings[index];
					rmse += error * error;
					mae  += Math.Abs(error);
				}
			mae  = mae / ratings.Count;
			rmse = Math.Sqrt(rmse / ratings.Count);

			var result = new Dictionary<string, double>();
			result["RMSE"] = rmse;
			result["MAE"]  = mae;
			result["NMAE"] = mae / (recommender.MaxRating - recommender.MinRating);
			return result;
		}
	}
}