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
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.Eval
{
	/// <summary>Cross-validation for item recommendation</summary>
	public static class ItemsCrossValidation
	{
		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="split">a dataset split</param>
		/// <param name="test_users">a collection of integers with all test users</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="show_results">set to true to print results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> DoCrossValidation(
			this IRecommender recommender,
			ISplit<IPosOnlyFeedback> split,
			IList<int> test_users,
			IList<int> candidate_items,
			CandidateItems candidate_item_mode = CandidateItems.OVERLAP,
			bool show_results = false)
		{
			var avg_results = new Dictionary<string, double>();
			
			if (!(recommender is ItemRecommender))
				throw new ArgumentException("recommender must be of type ItemRecommender");
			
			Parallel.For(0, (int) split.NumberOfFolds, fold =>
			{
				try
				{
					var split_recommender = (ItemRecommender) recommender.Clone(); // avoid changes in recommender
					split_recommender.Feedback = split.Train[fold];
					split_recommender.Train();
					var fold_results = Items.Evaluate(split_recommender, split.Train[fold], split.Test[fold], test_users, candidate_items, candidate_item_mode);
	
					foreach (var key in fold_results.Keys)
						if (avg_results.ContainsKey(key))
							avg_results[key] += fold_results[key];
						else
							avg_results[key] = fold_results[key];
					if (show_results)
						Console.Error.WriteLine("fold {0} {1}", fold, Items.FormatResults(fold_results));
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw e;
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
		/// <param name="split">a positive-only feedback dataset split</param>
		/// <param name="test_users">a collection of integers with all test users</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="repeated_events">allow repeated events in the evaluation (i.e. items accessed by a user before may be in the recommended list)</param>
		/// <param name="max_iter">the maximum number of iterations</param>
		/// <param name="find_iter">the report interval</param>
		static public void DoIterativeCrossValidation(
			this IRecommender recommender,
			ISplit<IPosOnlyFeedback> split,
			IList<int> test_users,
			IList<int> candidate_items,
			CandidateItems candidate_item_mode,
			bool repeated_events,
			int max_iter,
			int find_iter = 1)
		{
			if (!(recommender is IIterativeModel))
				throw new ArgumentException("recommender must be of type IIterativeModel");
			if (!(recommender is ItemRecommender))
				throw new ArgumentException("recommender must be of type ItemRecommender");

			var split_recommenders     = new ItemRecommender[split.NumberOfFolds];
			var iterative_recommenders = new IIterativeModel[split.NumberOfFolds];

			// initial training and evaluation
			Parallel.For(0, (int) split.NumberOfFolds, i =>
			{
				try
				{
					split_recommenders[i] = (ItemRecommender) recommender.Clone(); // to avoid changes in recommender
					split_recommenders[i].Feedback = split.Train[i];
					split_recommenders[i].Train();
					iterative_recommenders[i] = (IIterativeModel) split_recommenders[i];
					var fold_results = Items.Evaluate(split_recommenders[i], split.Test[i], split.Train[i], test_users, candidate_items, candidate_item_mode, repeated_events);
					Console.WriteLine("fold {0} {1} iteration {2}", i, Items.FormatResults(fold_results), iterative_recommenders[i].NumIter);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw e;
				}
			});

			// iterative training and evaluation
			for (int it = (int) iterative_recommenders[0].NumIter + 1; it <= max_iter; it++)
				Parallel.For(0, (int) split.NumberOfFolds, i =>
				{
					try
					{
						iterative_recommenders[i].Iterate();
	
						if (it % find_iter == 0)
						{
							var fold_results = Items.Evaluate(split_recommenders[i], split.Test[i], split.Train[i], test_users, candidate_items, candidate_item_mode, repeated_events);
							Console.WriteLine("fold {0} {1} iteration {2}", i, Items.FormatResults(fold_results), it);
						}
					}
					catch (Exception e)
					{
						Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
						throw e;
					}
				});
		}
	}
}

