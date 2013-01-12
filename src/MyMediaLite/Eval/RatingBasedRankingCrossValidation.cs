// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using System.Collections.Generic;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval
{
	/// <summary>Cross-validation for rating-based ranking</summary>
	public static class RatingBasedRankingCrossValidation
	{
		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="num_folds">the number of folds</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="compute_fit">if set to true measure fit on the training data as well</param>
		/// <param name="show_results">set to true to print results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public EvaluationResults DoRatingBasedRankingCrossValidation(
			this RatingPredictor recommender,
			uint num_folds,
			IList<int> candidate_items,
			CandidateItems candidate_item_mode = CandidateItems.OVERLAP,
			bool compute_fit = false,
			bool show_results = false)
		{
			var split = new RatingCrossValidationSplit(recommender.Ratings, num_folds);
			return recommender.DoRatingBasedRankingCrossValidation(split, candidate_items, candidate_item_mode, compute_fit, show_results);
		}

		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="split">a dataset split</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="compute_fit">if set to true measure fit on the training data as well</param>
		/// <param name="show_results">set to true to print results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public EvaluationResults DoRatingBasedRankingCrossValidation(
			this RatingPredictor recommender,
			ISplit<IRatings> split,
			IList<int> candidate_items,
			CandidateItems candidate_item_mode = CandidateItems.OVERLAP,
			bool compute_fit = false,
			bool show_results = false)
		{
			var avg_results = new ItemRecommendationEvaluationResults();

			Parallel.For(0, (int) split.NumberOfFolds, fold =>
			{
				try
				{
					var split_recommender = (RatingPredictor) recommender.Clone(); // avoid changes in recommender
					split_recommender.Ratings = split.Train[fold];
					split_recommender.Train();

					var test_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(split.Test[fold]);
					var training_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(split.Train[fold]);
					IList<int> test_users = test_data_posonly.AllUsers;
					var fold_results = Items.Evaluate(split_recommender, test_data_posonly, training_data_posonly, test_users, candidate_items, candidate_item_mode);
					if (compute_fit)
						fold_results["fit"] = (float) split_recommender.ComputeFit();

					// thread-safe stats
					lock (avg_results)
						foreach (var key in fold_results.Keys)
							if (avg_results.ContainsKey(key))
								avg_results[key] += fold_results[key];
							else
								avg_results[key] = fold_results[key];

					if (show_results)
						Console.Error.WriteLine("fold {0} {1}", fold, fold_results);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw;
				}
			});

			foreach (var key in Items.Measures)
				avg_results[key] /= split.NumberOfFolds;
			avg_results["num_users"] /= split.NumberOfFolds;
			avg_results["num_items"] /= split.NumberOfFolds;

			return avg_results;
		}

		/// <summary>Evaluate an iterative recommender on the folds of a dataset split, display results on STDOUT</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="num_folds">the number of folds</param>
		/// <param name="test_users">a collection of integers with all test users</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="repeated_events">allow repeated events in the evaluation (i.e. items accessed by a user before may be in the recommended list)</param>
		/// <param name="max_iter">the maximum number of iterations</param>
		/// <param name="find_iter">the report interval</param>
		/// <param name="show_fold_results">if set to true to print per-fold results to STDERR</param>
		static public void DoRatingBasedRankingIterativeCrossValidation(
			this RatingPredictor recommender,
			uint num_folds,
			IList<int> test_users,
			IList<int> candidate_items,
			CandidateItems candidate_item_mode,
			RepeatedEvents repeated_events,
			uint max_iter,
			uint find_iter = 1,
			bool show_fold_results = false)
		{
			var split = new RatingCrossValidationSplit(recommender.Ratings, num_folds);
			recommender.DoRatingBasedRankingIterativeCrossValidation(split, test_users, candidate_items, candidate_item_mode, repeated_events, max_iter, find_iter);
		}

		/// <summary>Evaluate an iterative recommender on the folds of a dataset split, display results on STDOUT</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="split">a positive-only feedback dataset split</param>
		/// <param name="test_users">a collection of integers with all test users</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="repeated_events">allow repeated events in the evaluation (i.e. items accessed by a user before may be in the recommended list)</param>
		/// <param name="max_iter">the maximum number of iterations</param>
		/// <param name="find_iter">the report interval</param>
		/// <param name="show_fold_results">if set to true to print per-fold results to STDERR</param>
		static public void DoRatingBasedRankingIterativeCrossValidation(
			this RatingPredictor recommender,
			ISplit<IRatings> split,
			IList<int> test_users,
			IList<int> candidate_items,
			CandidateItems candidate_item_mode,
			RepeatedEvents repeated_events,
			uint max_iter,
			uint find_iter = 1,
			bool show_fold_results = false)
		{
			if (!(recommender is IIterativeModel))
				throw new ArgumentException("recommender must be of type IIterativeModel");

			var split_recommenders     = new RatingPredictor[split.NumberOfFolds];
			var iterative_recommenders = new IIterativeModel[split.NumberOfFolds];
			var fold_results = new ItemRecommendationEvaluationResults[split.NumberOfFolds];

			// initial training and evaluation
			Parallel.For(0, (int) split.NumberOfFolds, i =>
			{
				try
				{
					split_recommenders[i] = (RatingPredictor) recommender.Clone(); // to avoid changes in recommender
					split_recommenders[i].Ratings = split.Train[i];
					split_recommenders[i].Train();
					iterative_recommenders[i] = (IIterativeModel) split_recommenders[i];
					
					var test_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(split.Test[i]);
					var training_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(split.Train[i]);
					fold_results[i] = Items.Evaluate(split_recommenders[i], test_data_posonly, training_data_posonly, test_users, candidate_items, candidate_item_mode, repeated_events);
					if (show_fold_results)
						Console.WriteLine("fold {0} {1} iteration {2}", i, fold_results, iterative_recommenders[i].NumIter);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw;
				}
			});
			Console.WriteLine("{0} iteration {1}", new ItemRecommendationEvaluationResults(fold_results), iterative_recommenders[0].NumIter);

			// iterative training and evaluation
			for (int it = (int) iterative_recommenders[0].NumIter + 1; it <= max_iter; it++)
			{
				Parallel.For(0, (int) split.NumberOfFolds, i =>
				{
					try
					{
						iterative_recommenders[i].Iterate();

						if (it % find_iter == 0)
						{
							var test_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(split.Test[i]);
							var training_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(split.Train[i]);

							fold_results[i] = Items.Evaluate(split_recommenders[i], test_data_posonly, training_data_posonly, test_users, candidate_items, candidate_item_mode, repeated_events);
							if (show_fold_results)
								Console.WriteLine("fold {0} {1} iteration {2}", i, fold_results, it);
						}
					}
					catch (Exception e)
					{
						Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
						throw;
					}
				});
				Console.WriteLine("{0} iteration {1}", new ItemRecommendationEvaluationResults(fold_results), it);
			}
		}
	}
}

