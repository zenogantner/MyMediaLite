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
using System.Threading;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.DataType;
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
			return string.Format(CultureInfo.InvariantCulture, "AUC {0:0.#####} prec@5 {1:0.#####} prec@10 {2:0.#####} MAP {3:0.#####} recall@5 {4:0.#####} recall@10 {5:0.#####} NDCG {6:0.#####} mrr {7:0.#####} num_users {8} num_items {9} num_lists {10}",
			                     result["AUC"], result["prec@5"], result["prec@10"], result["MAP"], result["recall@5"], result["recall@10"], result["NDCG"], result["mrr"], result["num_users"], result["num_items"], result["num_lists"]);
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
			return Evaluate(recommender, test, train, relevant_users, relevant_items, true);
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
		/// <param name="ignore_overlap">if true, ignore items that appear for a user in the training set when evaluating for that user</param>
		/// <returns>a dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(
			IItemRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback train,
		    ICollection<int> relevant_users,
			ICollection<int> relevant_items,
			bool ignore_overlap)
		{
			if (train.Overlap(test) > 0)
				Console.Error.WriteLine("WARNING: Overlapping train and test data");

			// compute evaluation measures
			double auc_sum       = 0;
			double map_sum       = 0;
			double prec_5_sum    = 0;
			double prec_10_sum   = 0;
			double recall_5_sum  = 0;
			double recall_10_sum = 0;
			double ndcg_sum      = 0;
			double rr_sum        = 0;
			int num_users        = 0;

			//foreach (int user_id in relevant_users)
			Parallel.ForEach (relevant_users, user_id =>
			{
				var correct_items = new HashSet<int>(test.UserMatrix[user_id]);
				correct_items.IntersectWith(relevant_items);

				// the number of items that are really relevant for this user
				var relevant_items_in_train = new HashSet<int>(train.UserMatrix[user_id]);
				relevant_items_in_train.IntersectWith(relevant_items);
				int num_eval_items = relevant_items.Count - (ignore_overlap ? relevant_items_in_train.Count() : 0);

				// skip all users that have 0 or #relevant_items test items
				if (correct_items.Count == 0)
					return;
				if (num_eval_items - correct_items.Count == 0)
					return;

				num_users++;
				IList<int> prediction = Prediction.PredictItems(recommender, user_id, relevant_items);
				if (prediction.Count != relevant_items.Count)
					throw new Exception("Not all items have been ranked.");

				ICollection<int> ignore_items = ignore_overlap ? train.UserMatrix[user_id] : new int[0];

				auc_sum  += AUC(prediction, correct_items, ignore_items);
				map_sum  += MAP(prediction, correct_items, ignore_items);
				ndcg_sum += NDCG(prediction, correct_items, ignore_items);
				rr_sum   += ReciprocalRank(prediction, correct_items, ignore_items);

				var ns = new int[] { 5, 10, 15 };
				var prec = PrecisionAt(prediction, correct_items, ignore_items, ns);
				prec_5_sum  += prec[5];
				prec_10_sum += prec[10];
				var recall = RecallAt(prediction, correct_items, ignore_items, ns);
				recall_5_sum  += recall[5];
				recall_10_sum += recall[10];

				if (num_users % 1000 == 0)
					Console.Error.Write(".");
				if (num_users % 60000 == 0)
					Console.Error.WriteLine();
			});

			var result = new Dictionary<string, double>();
			result["AUC"]       = auc_sum / num_users;
			result["prec@5"]    = prec_5_sum / num_users;
			result["prec@10"]   = prec_10_sum / num_users;
			result["MAP"]       = map_sum / num_users;
			result["recall@5"]  = recall_5_sum / num_users;
			result["recall@10"] = recall_10_sum / num_users;
			result["NDCG"]      = ndcg_sum / num_users;
			result["mrr"]       = rr_sum / num_users;
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

		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="split">a dataset split</param>
		/// <param name="relevant_users">a collection of integers with all relevant users</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> EvaluateOnSplit(ItemRecommender recommender,
		                                                         ISplit<IPosOnlyFeedback> split,
													 		     ICollection<int> relevant_users,
																 ICollection<int> relevant_items)
		{
			return EvaluateOnSplit(recommender, split, relevant_users, relevant_items, false);
		}

		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">an item recommender</param>
		/// <param name="split">a dataset split</param>
		/// <param name="relevant_users">a collection of integers with all relevant users</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <param name="show_results">set to true to print results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> EvaluateOnSplit(ItemRecommender recommender,
		                                                         ISplit<IPosOnlyFeedback> split,
													 		     ICollection<int> relevant_users,
																 ICollection<int> relevant_items,
		                                                         bool show_results)
		{
			var avg_results = new Dictionary<string, double>();

			for (int fold = 0; fold < split.NumberOfFolds; fold++)
			{
				var split_recommender = (ItemRecommender) recommender.Clone(); // to avoid changes in recommender
				split_recommender.Feedback = split.Train[fold];
				split_recommender.Train();
				var fold_results = Evaluate(split_recommender, split.Train[fold], split.Test[fold], relevant_users, relevant_items);

				foreach (var key in fold_results.Keys)
					if (avg_results.ContainsKey(key))
						avg_results[key] += fold_results[key];
					else
						avg_results[key] = fold_results[key];
				if (show_results)
					Console.Error.Write("fold {0} {1}", fold, FormatResults(avg_results));
			}

			foreach (var key in avg_results.Keys.ToList())
				avg_results[key] /= split.NumberOfFolds;

			return avg_results;
		}

		/// <summary>Compute the area under the ROC curve (AUC) of a list of ranked items</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Area_Under_the_ROC_Curve
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>,
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the AUC for the given data</returns>
		public static double AUC(IList<int> ranked_items, ICollection<int> correct_items)
		{
			return AUC(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the area under the ROC curve (AUC) of a list of ranked items</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Area_Under_the_ROC_Curve
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the AUC for the given data</returns>
		public static double AUC(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
			int num_eval_items = ranked_items.Count - ignore_items.Intersect(ranked_items).Count();
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

		/// <summary>Compute the reciprocal rank of a list of ranked items</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Mean_reciprocal_rank
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>,
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the reciprocal rank for the given data</returns>
		public static double ReciprocalRank(IList<int> ranked_items, ICollection<int> correct_items)
		{
			return ReciprocalRank(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the reciprocal rank of a list of ranked items</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Mean_reciprocal_rank
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the mean reciprocal rank for the given data</returns>
		public static double ReciprocalRank(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
			int pos = 0;

			foreach (int item_id in ranked_items)
			{
				if (ignore_items.Contains(item_id))
					continue;

				if (correct_items.Contains(ranked_items[pos]))
					return (double) 1 / (pos + 1);

				pos++;
			}

			return 0;
		}

		/// <summary>Compute the mean average precision (MAP) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the MAP for the given data</returns>
		public static double MAP(IList<int> ranked_items, ICollection<int> correct_items)
		{
			return MAP(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the mean average precision (MAP) of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the MAP for the given data</returns>
		public static double MAP(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
			int hit_count       = 0;
			double avg_prec_sum = 0;
			int left_out        = 0;

			for (int i = 0; i < ranked_items.Count; i++)
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
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Discounted_Cumulative_Gain
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the NDCG for the given data</returns>
		public static double NDCG(IList<int> ranked_items, ICollection<int> correct_items)
		{
			return NDCG(ranked_items, correct_items, new HashSet<int>());
		}

		/// <summary>Compute the normalized discounted cumulative gain (NDCG) of a list of ranked items</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Discounted_Cumulative_Gain
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the NDCG for the given data</returns>
		public static double NDCG(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items)
		{
			double dcg   = 0;
			double idcg  = ComputeIDCG(correct_items.Count);
			int left_out = 0;

			for (int i = 0; i < ranked_items.Count; i++)
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

		/// <summary>Compute the precision@N of a list of ranked items at several N</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <param name="ns">the cutoff positions in the list</param>
		/// <returns>the precision@N for the given data at the different positions N</returns>
		public static Dictionary<int, double> PrecisionAt(IList<int> ranked_items,
		                                                  ICollection<int> correct_items,
		                                                  ICollection<int> ignore_items,
		                                                  IList<int> ns)
		{
			var precision_at_n = new Dictionary<int, double>();
			foreach (int n in ns)
				precision_at_n[n] = PrecisionAt(ranked_items, correct_items, ignore_items, n);
			return precision_at_n;
		}

		/// <summary>Compute the precision@N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the precision@N for the given data</returns>
		public static double PrecisionAt(IList<int> ranked_items, ICollection<int> correct_items, int n)
		{
			return PrecisionAt(ranked_items, correct_items, new HashSet<int>(), n);
		}

		/// <summary>Compute the precision@N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the precision@N for the given data</returns>
		public static double PrecisionAt(IList<int> ranked_items, ICollection<int> correct_items,
		                                 ICollection<int> ignore_items, int n)
		{
			return (double) HitsAt(ranked_items, correct_items, ignore_items, n) / n;
		}

		/// <summary>Compute the recall@N of a list of ranked items at several N</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <param name="ns">the cutoff positions in the list</param>
		/// <returns>the recall@N for the given data at the different positions N</returns>
		public static Dictionary<int, double> RecallAt(IList<int> ranked_items,
		                                                  ICollection<int> correct_items,
		                                                  ICollection<int> ignore_items,
		                                                  IList<int> ns)
		{
			var recall_at_n = new Dictionary<int, double>();
			foreach (int n in ns)
				recall_at_n[n] = RecallAt(ranked_items, correct_items, ignore_items, n);
			return recall_at_n;
		}

		/// <summary>Compute the recall@N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the recall@N for the given data</returns>
		public static double RecallAt(IList<int> ranked_items, ICollection<int> correct_items, int n)
		{
			return RecallAt(ranked_items, correct_items, new HashSet<int>(), n);
		}

		/// <summary>Compute the recall@N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the recall@N for the given data</returns>
		public static double RecallAt(IList<int> ranked_items, ICollection<int> correct_items,
		                                 ICollection<int> ignore_items, int n)
		{
			return (double) HitsAt(ranked_items, correct_items, ignore_items, n) / correct_items.Count;
		}

		/// <summary>Compute the number of hits until position N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the hits@N for the given data</returns>
		public static int HitsAt(IList<int> ranked_items, ICollection<int> correct_items,
		                                 ICollection<int> ignore_items, int n)
		{
			if (n < 1)
				throw new ArgumentException("n must be at least 1.");

			int hit_count = 0;
			int left_out  = 0;

			for (int i = 0; i < ranked_items.Count; i++)
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

			return hit_count;
		}

		/// <summary>Computes the ideal DCG given the number of positive items.</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Discounted_Cumulative_Gain
		/// </remarks>
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
