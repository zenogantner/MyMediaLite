// Copyright (C) 2010, 2011 Zeno Gantner
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
using System.Linq;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval.Measures;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class for filtered item recommendation</summary>
	public static class ItemsFiltered
	{
		/// <summary>For a given user and the test dataset, return a dictionary of items filtered by attributes</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="test">the test dataset</param>
		/// <param name="item_attributes"></param>
		/// <returns>a dictionary containing a mapping from attribute IDs to collections of item IDs</returns>
		static public Dictionary<int, ICollection<int>> GetFilteredItems(
			int user_id, IPosOnlyFeedback test,
			SparseBooleanMatrix item_attributes)
		{
			var filtered_items = new Dictionary<int, ICollection<int>>();

			foreach (int item_id in test.UserMatrix[user_id])
				foreach (int attribute_id in item_attributes[item_id])
					if (filtered_items.ContainsKey(attribute_id))
						filtered_items[attribute_id].Add(item_id);
					else
						filtered_items[attribute_id] = new HashSet<int>() { item_id };

			return filtered_items;
		}

		/// <summary>Evaluation for rankings of filtered items</summary>
		/// <remarks>
		/// </remarks>
		/// <param name="recommender">item recommender</param>
		/// <param name="test">test cases</param>
		/// <param name="train">training data</param>
		/// <param name="item_attributes">the item attributes to be used for filtering</param>
		/// <param name="test_users">a collection of integers with all test users</param>
		/// <param name="candidate_items">a collection of integers with all candidate items</param>
		/// <param name="repeated_events">allow repeated events in the evaluation (i.e. items accessed by a user before may be in the recommended list)</param>
		/// <returns>a dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(
			this IRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback train,
			SparseBooleanMatrix item_attributes,
			IList<int> test_users,
			IList<int> candidate_items,
			bool repeated_events = false)
		{
			SparseBooleanMatrix items_by_attribute = (SparseBooleanMatrix) item_attributes.Transpose();

			int num_users = 0;
			int num_lists = 0;
			var result = new Dictionary<string, double>();
			foreach (string method in Items.Measures)
				result[method] = 0;

			Parallel.ForEach (test_users, user_id =>
			{
				var filtered_items = GetFilteredItems(user_id, test, item_attributes);
				int last_user_id = -1;

				foreach (int attribute_id in filtered_items.Keys)
				{
					var filtered_candidate_items = new HashSet<int>(items_by_attribute[attribute_id]);
					filtered_candidate_items.IntersectWith(candidate_items);

					var correct_items = new HashSet<int>(filtered_items[attribute_id]);
					correct_items.IntersectWith(filtered_candidate_items);

					// the number of candidate items for this user
					var candidate_items_in_train = new HashSet<int>(train.UserMatrix[user_id]);
					candidate_items_in_train.IntersectWith(filtered_candidate_items);
					int num_eval_items = filtered_candidate_items.Count - candidate_items_in_train.Count;

					// skip all users that have 0 or #filtered_candidate_items test items
					if (correct_items.Count == 0)
						return;
					if (num_eval_items - correct_items.Count == 0)
						return;

					// evaluation
					IList<int> prediction_list = Extensions.PredictItems(recommender, user_id, filtered_candidate_items.ToArray());
					ICollection<int> ignore_items = repeated_events ? new int[0] : train.UserMatrix[user_id];

					double auc  = AUC.Compute(prediction_list, correct_items, ignore_items);
					double map  = PrecisionAndRecall.AP(prediction_list, correct_items, ignore_items);
					double ndcg = NDCG.Compute(prediction_list, correct_items, ignore_items);
					double rr   = ReciprocalRank.Compute(prediction_list, correct_items, ignore_items);
					var positions = new int[] { 5, 10 };
					var prec = PrecisionAndRecall.PrecisionAt(prediction_list, correct_items, ignore_items, positions);
					var recall = PrecisionAndRecall.RecallAt(prediction_list, correct_items, ignore_items, positions);

					// thread-safe incrementing
					lock(result)
					{
						// counting stats
						num_lists++;
						if (last_user_id != user_id)
						{
							last_user_id = user_id;
							num_users++;
						}

						// result bookkeeping
						result["AUC"]       += auc;
						result["MAP"]       += map;
						result["NDCG"]      += ndcg;
						result["MRR"]       += rr;
						result["prec@5"]    += prec[5];
						result["prec@10"]   += prec[10];
						result["recall@5"]  += recall[5];
						result["recall@10"] += recall[10];
					}

					if (prediction_list.Count != filtered_candidate_items.Count)
						throw new Exception("Not all items have been ranked.");

					if (num_lists % 5000 == 0)
						Console.Error.Write(".");
					if (num_lists % 300000 == 0)
						Console.Error.WriteLine();
				}
			});

			foreach (string measure in Items.Measures)
				result[measure] /= num_lists;
			result["num_users"] = num_users;
			result["num_lists"] = num_lists;
			result["num_items"] = candidate_items.Count;

			return result;
		}

		// TODO implement online eval
	}
}
