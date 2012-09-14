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
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval
{
	/// <summary>Fold-in evaluation</summary>
	public static class FoldIn
	{
		/// <summary>Performs user-wise fold-in evaluation</summary>
		/// <returns>the evaluation results</returns>
		/// <param name='recommender'>a rating predictor capable of performing a user fold-in</param>
		/// <param name='update_data'>the rating data used to represent the users</param>
		/// <param name='eval_data'>the evaluation data</param>
		static public RatingPredictionEvaluationResults EvaluateFoldIn(this IFoldInRatingPredictor recommender, IRatings update_data, IRatings eval_data)
		{
			double rmse = 0;
			double mae  = 0;
			double cbd  = 0;

			int rating_count = 0;
			foreach (int user_id in update_data.AllUsers)
				if (eval_data.AllUsers.Contains(user_id))
				{
					var known_ratings = (
						from index in update_data.ByUser[user_id]
						select Tuple.Create(update_data.Items[index], update_data[index])
					).ToArray();
					var items_to_rate = (from index in eval_data.ByUser[user_id] select eval_data.Items[index]).ToArray();
					var predicted_ratings = recommender.ScoreItems(known_ratings, items_to_rate);

					foreach (var pred in predicted_ratings)
					{
						float prediction = pred.Item2;
						float actual_rating = eval_data.Get(user_id, pred.Item1, eval_data.ByUser[user_id]);
						float error = prediction - actual_rating;

						rmse += error * error;
						mae  += Math.Abs(error);
						cbd  += Eval.Ratings.ComputeCBD(actual_rating, prediction, recommender.MinRating, recommender.MaxRating);
						rating_count++;
					}
					Console.Error.Write(".");
				}

			mae  = mae / rating_count;
			rmse = Math.Sqrt(rmse / rating_count);
			cbd  = cbd / rating_count;

			var result = new RatingPredictionEvaluationResults();
			result["RMSE"] = (float) rmse;
			result["MAE"]  = (float) mae;
			result["NMAE"] = (float) mae / (recommender.MaxRating - recommender.MinRating);
			result["CBD"]  = (float) cbd;
			return result;
		}

		/// <summary>Performs user-wise fold-in evaluation, but instead of folding in perform a complete re-training with the new data</summary>
		/// <remarks>
		/// This method can be quite slow.
		/// </remarks>
		/// <returns>the evaluation results</returns>
		/// <param name='recommender'>a rating predictor capable of performing a user fold-in</param>
		/// <param name='update_data'>the rating data used to represent the users</param>
		/// <param name='eval_data'>the evaluation data</param>
		static public RatingPredictionEvaluationResults EvaluateFoldInCompleteRetraining(this RatingPredictor recommender, IRatings update_data, IRatings eval_data)
		{
			double rmse = 0;
			double mae  = 0;
			double cbd  = 0;

			int rating_count = 0;
			foreach (int user_id in update_data.AllUsers)
				if (eval_data.AllUsers.Contains(user_id))
				{
					var local_recommender = (RatingPredictor) recommender.Clone();

					var known_ratings = new RatingsProxy(update_data, update_data.ByUser[user_id]);
					local_recommender.Ratings = new CombinedRatings(recommender.Ratings, known_ratings);
					local_recommender.Train();

					var items_to_rate = (from index in eval_data.ByUser[user_id] select eval_data.Items[index]).ToArray();
					var predicted_ratings = recommender.Recommend(user_id, candidate_items:items_to_rate);

					foreach (var pred in predicted_ratings)
					{
						float prediction = pred.Item2;
						float actual_rating = eval_data.Get(user_id, pred.Item1, eval_data.ByUser[user_id]);
						float error = prediction - actual_rating;

						rmse += error * error;
						mae  += Math.Abs(error);
						cbd  += Eval.Ratings.ComputeCBD(actual_rating, prediction, recommender.MinRating, recommender.MaxRating);
						rating_count++;
					}
					Console.Error.Write(".");
				}

			mae  = mae / rating_count;
			rmse = Math.Sqrt(rmse / rating_count);
			cbd  = cbd / rating_count;

			var result = new RatingPredictionEvaluationResults();
			result["RMSE"] = (float) rmse;
			result["MAE"]  = (float) mae;
			result["NMAE"] = (float) mae / (recommender.MaxRating - recommender.MinRating);
			result["CBD"]  = (float) cbd;
			return result;
		}

		/// <summary>Performs user-wise fold-in evaluation, but instead of folding in perform incremental training with the new data</summary>
		/// <remarks>
		/// </remarks>
		/// <returns>the evaluation results</returns>
		/// <param name='recommender'>a rating predictor capable of performing a user fold-in</param>
		/// <param name='update_data'>the rating data used to represent the users</param>
		/// <param name='eval_data'>the evaluation data</param>
		static public RatingPredictionEvaluationResults EvaluateFoldInIncrementalTraining(this IncrementalRatingPredictor recommender, IRatings update_data, IRatings eval_data)
		{
			double rmse = 0;
			double mae  = 0;
			double cbd  = 0;

			int rating_count = 0;
			foreach (int user_id in update_data.AllUsers)
				if (eval_data.AllUsers.Contains(user_id))
				{
					var local_recommender = (IncrementalRatingPredictor) recommender.Clone();

					// add ratings and perform incremental training
					var user_ratings = new RatingsProxy(update_data, update_data.ByUser[user_id]);
					local_recommender.AddRatings(user_ratings);

					var items_to_rate = (from index in eval_data.ByUser[user_id] select eval_data.Items[index]).ToArray();
					var predicted_ratings = recommender.Recommend(user_id, candidate_items:items_to_rate);

					foreach (var pred in predicted_ratings)
					{
						float prediction = pred.Item2;
						float actual_rating = eval_data.Get(user_id, pred.Item1, eval_data.ByUser[user_id]);
						float error = prediction - actual_rating;

						rmse += error * error;
						mae  += Math.Abs(error);
						cbd  += Eval.Ratings.ComputeCBD(actual_rating, prediction, recommender.MinRating, recommender.MaxRating);
						rating_count++;
					}

					// remove ratings again
					local_recommender.RemoveRatings(user_ratings);

					Console.Error.Write(".");
				}
			
			mae  = mae / rating_count;
			rmse = Math.Sqrt(rmse / rating_count);
			cbd  = cbd / rating_count;

			var result = new RatingPredictionEvaluationResults();
			result["RMSE"] = (float) rmse;
			result["MAE"]  = (float) mae;
			result["NMAE"] = (float) mae / (recommender.MaxRating - recommender.MinRating);
			result["CBD"]  = (float) cbd;
			return result;
		}
	}
}
