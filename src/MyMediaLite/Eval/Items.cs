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
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval.Measures;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class for item recommendation</summary>
	public static class Items
	{
		/// <summary>the evaluation measures for item prediction offered by the class</summary>
		static public ICollection<string> Measures
		{
			get	{
				string[] measures = { "AUC", "prec@5", "prec@10", "MAP", "recall@5", "recall@10", "NDCG", "mrr" };
				return new HashSet<string>(measures);
			}
		}

		/// <summary>Format item prediction results</summary>
		/// <param name="result">the result dictionary</param>
		/// <returns>a string containing the results</returns>
		static public string FormatResults(Dictionary<string, double> result)
		{
			return string.Format(
				CultureInfo.InvariantCulture, "AUC {0:0.#####} prec@5 {1:0.#####} prec@10 {2:0.#####} MAP {3:0.#####} recall@5 {4:0.#####} recall@10 {5:0.#####} NDCG {6:0.#####} mrr {7:0.#####} num_users {8} num_items {9} num_lists {10}",
				result["AUC"], result["prec@5"], result["prec@10"], result["MAP"], result["recall@5"], result["recall@10"], result["NDCG"], result["mrr"], result["num_users"], result["num_items"], result["num_lists"]
			);
		}

		/// <summary>Evaluation for rankings of items</summary>
		/// <remarks>
		/// User-item combinations that appear in both sets are ignored for the test set, and thus in the evaluation,
		/// except the boolean argument repeated_events is set.
		///
		/// The evaluation measures are listed in the Measures property.
		/// Additionally, 'num_users' and 'num_items' report the number of users that were used to compute the results
		/// and the number of items that were taken into account.
		///
		/// Literature:
		/// <list type="bullet">
		///   <item><description>
		///   C. Manning, P. Raghavan, H. Schütze: Introduction to Information Retrieval, Cambridge University Press, 2008
		///   </description></item>
		/// </list>
		///
		/// On multi-core/multi-processor systems, the routine tries to use as many cores as possible,
		/// which should to an almost linear speed-up.
		/// </remarks>
		/// <param name="recommender">item recommender</param>
		/// <param name="test">test cases</param>
		/// <param name="training">training data</param>
		/// <param name="relevant_users">a list of integers with all relevant users</param>
		/// <param name="relevant_items">a list of integers with all relevant items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="repeated_events">allow repeated events in the evaluation (i.e. items accessed by a user before may be in the recommended list)</param>
		/// <returns>a dictionary containing the evaluation results (default is false)</returns>
		static public Dictionary<string, double> Evaluate(
			IRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback training,
			IList<int> relevant_users,
			IList<int> relevant_items,
			CandidateItems candidate_item_mode,
			bool repeated_events = false)
		{
			if (!repeated_events && training.OverlapCount(test) > 0)
				Console.Error.WriteLine("WARNING: Overlapping train and test data");

			switch (candidate_item_mode)
			{
				case CandidateItems.TRAINING: relevant_items = training.AllItems; break;
				case CandidateItems.TEST:     relevant_items = test.AllItems; break;
				case CandidateItems.OVERLAP:  relevant_items = new List<int>(test.AllItems.Intersect(training.AllItems)); break;
				case CandidateItems.UNION:    relevant_items = new List<int>(test.AllItems.Union(training.AllItems)); break;
			}

			int num_users = 0;
			var result = new Dictionary<string, double>();
			foreach (string method in Measures)
				result[method] = 0;

			Parallel.ForEach(relevant_users, user_id =>
			{
				try
				{
					var correct_items = new HashSet<int>(test.UserMatrix[user_id]);
					correct_items.IntersectWith(relevant_items);

					// the number of items that are really relevant for this user
					var relevant_items_in_train = new HashSet<int>(training.UserMatrix[user_id]);
					relevant_items_in_train.IntersectWith(relevant_items);
					int num_eval_items = relevant_items.Count - (repeated_events ? 0 : relevant_items_in_train.Count());

					// skip all users that have 0 or #relevant_items test items
					if (correct_items.Count == 0)
						return;
					if (num_eval_items - correct_items.Count == 0)
						return;

					IList<int> prediction_list = Prediction.PredictItems(recommender, user_id, relevant_items);
					if (prediction_list.Count != relevant_items.Count)
						throw new Exception("Not all items have been ranked.");

					ICollection<int> ignore_items = repeated_events ? new int[0] : training.UserMatrix[user_id];

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
						num_users++;
						result["AUC"]       += auc;
						result["MAP"]       += map;
						result["NDCG"]      += ndcg;
						result["mrr"]       += rr;
						result["prec@5"]    += prec[5];
						result["prec@10"]   += prec[10];
						result["recall@5"]  += recall[5];
						result["recall@10"] += recall[10];
					}

					if (num_users % 1000 == 0)
						Console.Error.Write(".");
					if (num_users % 60000 == 0)
						Console.Error.WriteLine();
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace); throw e;
				}
			});

			foreach (string measure in Measures)
				result[measure] /= num_users;
			result["num_users"] = num_users;
			result["num_lists"] = num_users;
			result["num_items"] = relevant_items.Count;

			return result;
		}
	}
}
