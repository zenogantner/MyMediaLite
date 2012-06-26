// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Linq;
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
			get {
				string[] measures = { "RMSE", "MAE", "NMAE", "CBD" };
				return new HashSet<string>(measures);
			}
		}

		/// <summary>Evaluates a rating predictor for RMSE, (N)MAE, and CBD</summary>
		/// <remarks>
		///   <para>
		///     See http://recsyswiki.com/wiki/Root_mean_square_error and http://recsyswiki.com/wiki/Mean_absolute_error
		///   </para>
		///   <para>
		///     For NMAE, see the paper by Goldberg et al.
		///   </para>
		///   <para>
		///     For CBD (capped binomial deviance), see http://www.kaggle.com/c/ChessRatings2/Details/Evaluation
		///   </para>
		///   <para>
		///     If the recommender can take time into account, and the rating dataset provides rating times,
		///     this information will be used for making rating predictions.
		///   </para>
		///   <para>
		///     Literature:
		///     <list type="bullet">
		///       <item><description>
		///         Ken Goldberg, Theresa Roeder, Dhruv Gupta, and Chris Perkins:
		///         Eigentaste: A Constant Time Collaborative Filtering Algorithm.
		///         nformation Retrieval Journal 2001.
		///         http://goldberg.berkeley.edu/pubs/eigentaste.pdf
		///       </description></item>
		///     </list>
		///   </para>
		/// </remarks>
		/// <param name="recommender">rating predictor</param>
		/// <param name="test_ratings">test cases</param>
		/// <param name="training_ratings">the training examples</param>
		/// <returns>a Dictionary containing the evaluation results</returns>
		static public RatingPredictionEvaluationResults Evaluate(this IRatingPredictor recommender, IRatings test_ratings, IRatings training_ratings = null)
		{
			if (recommender == null)
				throw new ArgumentNullException("recommender");
			if (test_ratings == null)
				throw new ArgumentNullException("ratings");

			var all_indices = Enumerable.Range(0, test_ratings.Count).ToArray();
			var results = new RatingPredictionEvaluationResults(Evaluate(recommender, test_ratings, all_indices));
			if (training_ratings != null)
			{
				var new_user_indices = (from index in all_indices
				                        where test_ratings.Users[index] > training_ratings.MaxUserID || training_ratings.CountByUser[test_ratings.Users[index]] == 0
				                        select index).ToArray();
				results.NewUserResults = Evaluate(recommender, test_ratings, new_user_indices);
				var new_item_indices = (from index in all_indices
				                        where test_ratings.Items[index] > training_ratings.MaxItemID || training_ratings.CountByItem[test_ratings.Items[index]] == 0 select index).ToArray();
				results.NewItemResults = Evaluate(recommender, test_ratings, new_item_indices);
				results.NewUserNewItemResults = Evaluate(recommender, test_ratings, Enumerable.Intersect(new_user_indices, new_item_indices).ToArray());
			}
			return results;
		}

		static Dictionary<string, float> Evaluate(IRatingPredictor recommender, IRatings ratings, IList<int> indices)
		{
			if (indices.Count == 0)
				return null;

			double rmse = 0;
			double mae  = 0;
			double cbd  = 0;

			if (recommender is ITimeAwareRatingPredictor && ratings is ITimedRatings)
			{
				var time_aware_recommender = recommender as ITimeAwareRatingPredictor;
				var timed_ratings = ratings as ITimedRatings;
				foreach (int index in indices)
				{
					float prediction = time_aware_recommender.Predict(timed_ratings.Users[index], timed_ratings.Items[index], timed_ratings.Times[index]);
					float error = prediction - ratings[index];

					rmse += error * error;
					mae  += Math.Abs(error);
					cbd  += ComputeCBD(ratings[index], prediction, ratings.Scale.Min, ratings.Scale.Max);
				}
			}
			else
				foreach (int index in indices)
				{
					float prediction = recommender.Predict(ratings.Users[index], ratings.Items[index]);
					float error = prediction - ratings[index];

					rmse += error * error;
					mae  += Math.Abs(error);
					cbd  += ComputeCBD(ratings[index], prediction, ratings.Scale.Min, ratings.Scale.Max);
				}
			mae  = mae / indices.Count;
			rmse = Math.Sqrt(rmse / indices.Count);
			cbd  = cbd / indices.Count;

			var result = new Dictionary<string, float>();
			result["RMSE"] = (float) rmse;
			result["MAE"]  = (float) mae;
			result["NMAE"] = (float) mae / (recommender.MaxRating - recommender.MinRating);
			result["CBD"]  = (float) cbd;
			return result;
		}

		/// <summary>Compute the capped binomial deviation (CBD)</summary>
		/// <remarks>
		///   http://www.kaggle.com/c/ChessRatings2/Details/Evaluation
		/// </remarks>
		/// <returns>The CBD of a given rating and a prediction</returns>
		/// <param name='actual_rating'>the actual rating</param>
		/// <param name='prediction'>the predicted rating</param>
		/// <param name='min_rating'>the lower bound of the rating scale</param>
		/// <param name='max_rating'>the upper bound of the rating scale</param>
		static public double ComputeCBD(double actual_rating, double prediction, double min_rating, double max_rating)
		{
			// map into [0, 1] interval
			prediction    = (prediction - min_rating) / (max_rating - min_rating);
			actual_rating = (actual_rating - min_rating) / (max_rating - min_rating);

			// cap predictions
			if (prediction < 0.01)
				prediction = 0.01;
			if (prediction > 0.99)
				prediction = 0.99;
			return -(actual_rating * Math.Log10(prediction) + (1 - actual_rating) * Math.Log10(1 - prediction));
		}

		/// <summary>Computes the RMSE fit of a recommender on the training data</summary>
		/// <returns>the RMSE on the training data</returns>
		/// <param name='recommender'>the rating predictor to evaluate</param>
		public static double ComputeFit(this RatingPredictor recommender)
		{
			return recommender.Evaluate(recommender.Ratings)["RMSE"];
		}
	}
}