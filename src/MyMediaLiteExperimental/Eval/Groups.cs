// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval.Measures;
using MyMediaLite.GroupRecommendation;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class for group recommendation</summary>
	public static class Groups
	{
		/// <summary>the evaluation measures for item prediction offered by the class</summary>
		static public ICollection<string> Measures
		{
			get	{
				return Items.Measures;
			}
		}

		// TODO add recall eval
		
		/// <summary>Format group recommendation results</summary>
		/// <param name="result">the result dictionary</param>
		/// <returns>the formatted results</returns>
		static public string FormatResults(Dictionary<string, double> result)
		{
			return string.Format(CultureInfo.InvariantCulture, "AUC {0:0.#####} prec@5 {1:0.#####} prec@10 {2:0.#####} MAP {3:0.#####} NDCG {4:0.#####} num_users {5} num_items {6} num_lists {7}",
 	                             result["AUC"], result["prec@5"], result["prec@10"], result["MAP"], result["NDCG"], result["num_groups"], result["num_items"], result["num_lists"]);
		}

		/// <summary>Evaluation for rankings of items</summary>
		/// <remarks>
		/// User-item combinations that appear in both sets are ignored for the test set, and thus in the evaluation.
		/// The evaluation measures are listed in the ItemPredictionMeasures property.
		/// Additionally, 'num_groups' and 'num_items' report the number of user groups that were used to compute the results
		/// and the number of items that were taken into account.
		///
		/// Literature:
		///   C. Manning, P. Raghavan, H. Schütze: Introduction to Information Retrieval, Cambridge University Press, 2008
		/// </remarks>
		/// <param name="recommender">group recommender</param>
		/// <param name="test">test cases</param>
		/// <param name="train">training data</param>
		/// <param name="group_to_user">group to user relation</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <returns>a dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(
			GroupRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback train,
		    SparseBooleanMatrix group_to_user,
			ICollection<int> relevant_items)
		{
			return Evaluate(recommender, test, train, group_to_user, relevant_items, true);
		}

		/// <summary>Evaluation for rankings of items</summary>
		/// <remarks>
		/// User-item combinations that appear in both sets are ignored for the test set, and thus in the evaluation.
		/// The evaluation measures are listed in the ItemPredictionMeasures property.
		/// Additionally, 'num_users' and 'num_items' report the number of users that were used to compute the results
		/// and the number of items that were taken into account.
		///
		/// Literature:
		///   C. Manning, P. Raghavan, H. Schütze: Introduction to Information Retrieval, Cambridge University Press, 2008
		/// </remarks>
		/// <param name="recommender">group recommender</param>
		/// <param name="test">test cases</param>
		/// <param name="train">training data</param>
		/// <param name="group_to_user">group to user relation</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <param name="ignore_overlap">if true, ignore items that appear for a group in the training set when evaluating for that user</param>
		/// <returns>a dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(
			GroupRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback train,
		    SparseBooleanMatrix group_to_user,
			ICollection<int> relevant_items,
			bool ignore_overlap)
		{
			if (train.Overlap(test) > 0)
				Console.Error.WriteLine("WARNING: Overlapping train and test data");

			// compute evaluation measures
			double auc_sum     = 0;
			double map_sum     = 0;
			double prec_5_sum  = 0;
			double prec_10_sum = 0;
			double prec_15_sum = 0;
			double ndcg_sum    = 0;
			int num_groups     = 0;

			foreach (int group_id in group_to_user.NonEmptyRowIDs)
			{
				var users = group_to_user.GetEntriesByRow(group_id);

				var correct_items = new HashSet<int>();
				foreach (int user_id in users)
					correct_items.UnionWith(test.UserMatrix[user_id]);
				correct_items.IntersectWith(relevant_items);

				var relevant_items_in_train = new HashSet<int>();
				foreach (int user_id in users)
					relevant_items_in_train.UnionWith(train.UserMatrix[user_id]);
				relevant_items_in_train.IntersectWith(relevant_items);
				int num_eval_items = relevant_items.Count - (ignore_overlap ? relevant_items_in_train.Count() : 0);

				// skip all groups that have 0 or #relevant_items test items
				if (correct_items.Count == 0)
					continue;
				if (num_eval_items - correct_items.Count == 0)
					continue;

				num_groups++;

				IList<int> prediction_list = recommender.RankItems(users, relevant_items);
				if (prediction_list.Count != relevant_items.Count)
					throw new Exception("Not all items have been ranked.");

				var ignore_items = ignore_overlap ? relevant_items_in_train : new HashSet<int>();
				auc_sum     += AUC.Compute(prediction_list, correct_items, ignore_items);
				map_sum     += PrecisionAndRecall.AP(prediction_list, correct_items, ignore_items);
				ndcg_sum    += NDCG.Compute(prediction_list, correct_items, ignore_items);
				prec_5_sum  += PrecisionAndRecall.PrecisionAt(prediction_list, correct_items, ignore_items,  5);
				prec_10_sum += PrecisionAndRecall.PrecisionAt(prediction_list, correct_items, ignore_items, 10);
				prec_15_sum += PrecisionAndRecall.PrecisionAt(prediction_list, correct_items, ignore_items, 15);

				if (num_groups % 1000 == 0)
					Console.Error.Write(".");
				if (num_groups % 60000 == 0)
					Console.Error.WriteLine();
			}

			var result = new Dictionary<string, double>();
			result["AUC"]        = auc_sum / num_groups;
			result["MAP"]        = map_sum / num_groups;
			result["NDCG"]       = ndcg_sum / num_groups;
			result["prec@5"]     = prec_5_sum / num_groups;
			result["prec@10"]    = prec_10_sum / num_groups;
			result["prec@15"]    = prec_15_sum / num_groups;
			result["num_groups"] = num_groups;
			result["num_lists"]  = num_groups;
			result["num_items"]  = relevant_items.Count;

			return result;

		}
	}
}
