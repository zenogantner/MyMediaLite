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
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.Eval
{
	/// <summary>Online evaluation for rankings of items</summary>
	public static class ItemsOnline
	{
		/// <summary>Online evaluation for rankings of items</summary>
		/// <remarks>
		/// The evaluation protocol works as follows:
		/// For every test user, evaluate on the test items, and then add the those test items to the training set and perform an incremental update.
		/// The sequence of users is random.
		/// </remarks>
		/// <param name="recommender">the item recommender to be evaluated</param>
		/// <param name="test">test cases</param>
		/// <param name="training">training data (must be connected to the recommender's training data)</param>
		/// <param name="test_users">a list of all test user IDs</param>
		/// <param name="candidate_items">a list of all candidate item IDs</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <returns>a dictionary containing the evaluation results (averaged by user)</returns>
		static public ItemRecommendationEvaluationResults EvaluateOnline(
			this IRecommender recommender,
			IPosOnlyFeedback test, IPosOnlyFeedback training,
			IList<int> test_users, IList<int> candidate_items,
			CandidateItems candidate_item_mode)
		{
			var incremental_recommender = recommender as IIncrementalItemRecommender;
			if (incremental_recommender == null)
				throw new ArgumentException("recommender must be of type IIncrementalItemRecommender");

			// prepare candidate items once to avoid recreating them
			switch (candidate_item_mode)
			{
				case CandidateItems.TRAINING: candidate_items = training.AllItems; break;
				case CandidateItems.TEST:     candidate_items = test.AllItems; break;
				case CandidateItems.OVERLAP:  candidate_items = new List<int>(test.AllItems.Intersect(training.AllItems)); break;
				case CandidateItems.UNION:    candidate_items = new List<int>(test.AllItems.Union(training.AllItems)); break;
			}
			
			test_users.Shuffle();
			var results_by_user = new Dictionary<int, ItemRecommendationEvaluationResults>();
			foreach (int user_id in test_users)
			{
				if (candidate_items.Intersect(test.ByUser[user_id]).Count() == 0)
					continue;
				
				// prepare data
				var current_test_data = new PosOnlyFeedback<SparseBooleanMatrix>();
				foreach (int index in test.ByUser[user_id])
					current_test_data.Add(user_id, test.Items[index]);
				// evaluate user
				var current_result = Items.Evaluate(recommender, current_test_data, training, current_test_data.AllUsers, candidate_items, CandidateItems.EXPLICIT);
				results_by_user[user_id] = current_result;

				// update recommender
				var tuples = new List<Tuple<int, int>>();
				foreach (int index in test.ByUser[user_id])
					tuples.Add(Tuple.Create(user_id, test.Items[index]));
				incremental_recommender.AddFeedback(tuples);
			}

			var results = new ItemRecommendationEvaluationResults();

			foreach (int u in results_by_user.Keys)
				foreach (string measure in Items.Measures)
					results[measure] += results_by_user[u][measure];

			foreach (string measure in Items.Measures)
				results[measure] /= results_by_user.Count;

			results["num_users"] = results_by_user.Count;
			results["num_items"] = candidate_items.Count;
			results["num_lists"] = results_by_user.Count;

			return results;
		}
	}
}

