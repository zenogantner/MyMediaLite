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
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class</summary>
	public static class Items
	{
		/// <summary>the evaluation measures for item prediction offered by the class</summary>
		static public ICollection<string> Measures
		{
			get	{
				string[] measures = { "AUC", "prec@5", "prec@10", "prec@15", "NDCG", "MAP" };
				return new HashSet<string>(measures);
			}
		}

		/// <summary>Display item prediction results</summary>
		/// <param name="result">the result dictionary</param>
		static public void DisplayResults(Dictionary<string, double> result)
		{
			Console.Write(string.Format(CultureInfo.InvariantCulture, "AUC {0,0:0.#####} prec@5 {1,0:0.#####} prec@10 {2,0:0.#####} MAP {3,0:0.#####} NDCG {4,0:0.#####} num_users {5} num_items {6} num_lists {7}",
			                            result["AUC"], result["prec@5"], result["prec@10"], result["MAP"], result["NDCG"], result["num_users"], result["num_items"], result["num_lists"]));
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
		/// <param name="recommender">item recommender</param>
		/// <param name="test">test cases</param>
		/// <param name="train">training data</param>
		/// <param name="relevant_users">a collection of integers with all relevant users</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <returns>a dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(
			IItemRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback train,
		    ICollection<int> relevant_users,
			ICollection<int> relevant_items)
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
			int num_users      = 0;

			foreach (int user_id in relevant_users)
			{
				var correct_items = new HashSet<int>(test.UserMatrix[user_id]);
				correct_items.IntersectWith(relevant_items);

				// the number of items that are really relevant for this user
				var relevant_items_in_train = new HashSet<int>(train.UserMatrix[user_id]);
				relevant_items_in_train.IntersectWith(relevant_items);
				int num_eval_items = relevant_items.Count - relevant_items_in_train.Count();

				// skip all users that have 0 or #relevant_items test items
				if (correct_items.Count == 0)
					continue;
				if (num_eval_items - correct_items.Count == 0)
					continue;

				num_users++;
				int[] prediction = Prediction.PredictItems(recommender, user_id, relevant_items);

				auc_sum     += AUC(prediction, correct_items, train.UserMatrix[user_id]);
				map_sum     += MAP(prediction, correct_items, train.UserMatrix[user_id]);
				ndcg_sum    += NDCG(prediction, correct_items, train.UserMatrix[user_id]);
				prec_5_sum  += PrecisionAt(prediction, correct_items, train.UserMatrix[user_id],  5);
				prec_10_sum += PrecisionAt(prediction, correct_items, train.UserMatrix[user_id], 10);
				prec_15_sum += PrecisionAt(prediction, correct_items, train.UserMatrix[user_id], 15);

				if (prediction.Length != relevant_items.Count)
					throw new Exception("Not all items have been ranked.");

				if (num_users % 1000 == 0)
					Console.Error.Write(".");
				if (num_users % 20000 == 0)
					Console.Error.WriteLine();
			}

			var result = new Dictionary<string, double>();
			result["AUC"]       = auc_sum / num_users;
			result["MAP"]       = map_sum / num_users;
			result["NDCG"]      = ndcg_sum / num_users;
			result["prec@5"]    = prec_5_sum / num_users;
			result["prec@10"]   = prec_10_sum / num_users;
			result["prec@15"]   = prec_15_sum / num_users;
			result["num_users"] = num_users;
			result["num_lists"] = num_users;
			result["num_items"] = relevant_items.Count;

			return result;
		}

		// TODO consider micro- (by item) and macro-averaging (by user, the current thing)
		/// <summary>Online evaluation for rankings of items</summary>
		/// <remarks>
		/// </remarks>
		/// <param name="recommender">item recommender</param>
		/// <param name="test">test cases</param>
		/// <param name="train">training data (must be connected to the recommender's training data)</param>
		/// <param name="relevant_users">a collection of integers with all relevant users</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <returns>a dictionary containing the evaluation results (averaged by user)</returns>
		static public Dictionary<string, double> EvaluateOnline(
			IItemRecommender recommender,
			IPosOnlyFeedback test, IPosOnlyFeedback train,
		    ICollection<int> relevant_users, ICollection<int> relevant_items)
		{
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

			foreach (int index in random_index)
			{
				if (relevant_users.Contains(users[index]) && relevant_items.Contains(items[index]))
				{
					// evaluate user
					var current_test = new PosOnlyFeedback<SparseBooleanMatrix>();
					current_test.Add(users[index], items[index]);
					var current_result = Evaluate(recommender, current_test, train, current_test.AllUsers, relevant_items);

					if (current_result["num_users"] == 1)
						if (results_by_user.ContainsKey(users[index]))
						{
							foreach (string measure in Measures)
								results_by_user[users[index]][measure] += current_result[measure];
							results_by_user[users[index]]["num_items"]++;
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
			foreach (string measure in Measures)
				results[measure] = 0;

			foreach (int u in results_by_user.Keys)
				foreach (string measure in Measures)
					results[measure] += results_by_user[u][measure] / results_by_user[u]["num_items"];

			foreach (string measure in Measures)
				results[measure] /= results_by_user.Count;

			results["num_users"] = results_by_user.Count;
			results["num_items"] = relevant_items.Count;
			results["num_lists"] = test.Count; // FIXME this is not exact

			return results;
		}

		/// <summary>Compute the area under the ROC curve (AUC) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>,
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the AUC for the given data</returns>
		public static double AUC(int[] ranked_items, ICollection<int> correct_items)
		{
			return AUC(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the area under the ROC curve (AUC) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the AUC for the given data</returns>
		public static double AUC(int[] ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
				int num_eval_items = ranked_items.Length - ignore_items.Intersect(ranked_items).Count();
				int num_eval_pairs = (num_eval_items - correct_items.Count) * correct_items.Count;

				int num_correct_pairs = 0;
				int hit_count         = 0;

				foreach (int item_id in ranked_items)
				{
					if (ignore_items.Contains(item_id))
						continue;

					if (!correct_items.Contains(item_id))
						num_correct_pairs += hit_count;
					else
						hit_count++;
				}

				return ((double) num_correct_pairs) / num_eval_pairs;
		}

		/// <summary>Compute the mean average precision (MAP) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the MAP for the given data</returns>
		public static double MAP(int[] ranked_items, ICollection<int> correct_items)
		{
			return MAP(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the mean average precision (MAP) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the MAP for the given data</returns>
		public static double MAP(int[] ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
			int hit_count       = 0;
			double avg_prec_sum = 0;
			int left_out        = 0;

			for (int i = 0; i < ranked_items.Length; i++)
			{
				int item_id = ranked_items[i];
				if (ignore_items.Contains(item_id))
				{
					left_out++;
					continue;
				}

				if (!correct_items.Contains(item_id))
					continue;

				hit_count++;

				avg_prec_sum += (double) hit_count / (i + 1 - left_out);
			}

			if (hit_count != 0)
				return avg_prec_sum / hit_count;
			else
				return 0;
		}

		/// <summary>Compute the normalized discounted cumulative gain (NDCG) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the NDCG for the given data</returns>
		public static double NDCG(int[] ranked_items, ICollection<int> correct_items)
		{
			return NDCG(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the normalized discounted cumulative gain (NDCG) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the NDCG for the given data</returns>
		public static double NDCG(int[] ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
			double dcg   = 0;
			double idcg  = ComputeIDCG(correct_items.Count);
			int left_out = 0;

			for (int i = 0; i < ranked_items.Length; i++)
			{
				int item_id = ranked_items[i];
				if (ignore_items.Contains(item_id))
				{
					left_out++;
					continue;
				}

				if (!correct_items.Contains(item_id))
					continue;

				// compute NDCG part
				int rank = i + 1 - left_out;
				dcg += 1 / Math.Log(rank + 1, 2);
			}

			return dcg / idcg;
		}

		/// <summary>Compute the precision@N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the precision@N for the given data</returns>
		public static double PrecisionAt(int[] ranked_items, ICollection<int> correct_items, int n)
		{
			return PrecisionAt(ranked_items, correct_items, new HashSet<int>(), n);
		}

		/// <summary>Compute the precision@N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the precision@N for the given data</returns>
		public static double PrecisionAt(int[] ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items, int n)
		{
			if (n < 1)
				throw new ArgumentException("N must be at least 1.");

			int hit_count = 0;
			int left_out  = 0;

			for (int i = 0; i < ranked_items.Length; i++)
			{
				int item_id = ranked_items[i];
				if (ignore_items.Contains(item_id))
				{
					left_out++;
					continue;
				}

				if (!correct_items.Contains(item_id))
					continue;

				if (i < n + left_out)
					hit_count++;
				else
					break;
			}

			return (double) hit_count / n;
		}

		/// <summary>Computes the ideal DCG given the number of positive items.</summary>
		/// <returns>the ideal DCG</returns>
		/// <param name='n'>the number of positive items</param>
		static double ComputeIDCG(int n)
		{
			double idcg = 0;
			for (int i = 0; i < n; i++)
				idcg += 1 / Math.Log(i + 2, 2);
			return idcg;
		}
	}
}
