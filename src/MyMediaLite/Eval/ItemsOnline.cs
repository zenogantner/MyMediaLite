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
		// TODO consider micro- (by item) and macro-averaging (by user, the current thing); repeated events
		/// <summary>Online evaluation for rankings of items</summary>
		/// <remarks>
		/// </remarks>
		/// <param name="recommender">the item recommender to be evaluated</param>
		/// <param name="test">test cases</param>
		/// <param name="training">training data (must be connected to the recommender's training data)</param>
		/// <param name="relevant_users">a list of all relevant user IDs</param>
		/// <param name="relevant_items">a list of all relevant item IDs</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <returns>a dictionary containing the evaluation results (averaged by user)</returns>
		static public Dictionary<string, double> Evaluate(
			IIncrementalItemRecommender recommender,
			IPosOnlyFeedback test, IPosOnlyFeedback training,
			IList<int> relevant_users, IList<int> relevant_items,
			CandidateItems candidate_item_mode)
		{
			// prepare candidate items once to avoid recreating them
			switch (candidate_item_mode)
			{
				case CandidateItems.TRAINING: relevant_items = training.AllItems; break;
				case CandidateItems.TEST:     relevant_items = test.AllItems; break;
				case CandidateItems.OVERLAP:  relevant_items = new List<int>(test.AllItems.Intersect(training.AllItems)); break;
				case CandidateItems.UNION:    relevant_items = new List<int>(test.AllItems.Union(training.AllItems)); break;
			}
			candidate_item_mode = CandidateItems.EXPLICIT;

			// for better handling, move test data points into arrays
			var users = new int[test.Count];
			var items = new int[test.Count];
			int pos = 0;
			foreach (int user_id in test.UserMatrix.NonEmptyRowIDs)
				foreach (int item_id in test.UserMatrix[user_id])
				{
					users[pos] = user_id;
					items[pos] = item_id;
					pos++;
				}

			// random order of the test data points  // TODO chronological order
			var random_index = new int[test.Count];
			for (int index = 0; index < random_index.Length; index++)
				random_index[index] = index;
			Util.Utils.Shuffle<int>(random_index);

			var results_by_user = new Dictionary<int, Dictionary<string, double>>();

			int num_lists = 0;

			foreach (int index in random_index)
			{
				if (relevant_users.Contains(users[index]) && relevant_items.Contains(items[index]))
				{
					// evaluate user
					var current_test = new PosOnlyFeedback<SparseBooleanMatrix>();
					current_test.Add(users[index], items[index]);
					var current_result = Items.Evaluate(recommender, current_test, training, current_test.AllUsers, relevant_items, candidate_item_mode);

					if (current_result["num_users"] == 1)
						if (results_by_user.ContainsKey(users[index]))
						{
							foreach (string measure in Items.Measures)
								results_by_user[users[index]][measure] += current_result[measure];
							results_by_user[users[index]]["num_items"]++;
							num_lists++;
						}
						else
						{
							results_by_user[users[index]] = current_result;
							results_by_user[users[index]]["num_items"] = 1;
							results_by_user[users[index]].Remove("num_users");
						}
				}

				// update recommender
				recommender.AddFeedback(users[index], items[index]);
			}

			var results = new Dictionary<string, double>();
			foreach (string measure in Items.Measures)
				results[measure] = 0;

			foreach (int u in results_by_user.Keys)
				foreach (string measure in Items.Measures)
					results[measure] += results_by_user[u][measure] / results_by_user[u]["num_items"];

			foreach (string measure in Items.Measures)
				results[measure] /= results_by_user.Count;

			results["num_users"] = results_by_user.Count;
			results["num_items"] = relevant_items.Count;
			results["num_lists"] = num_lists;

			return results;
		}

	}
}

