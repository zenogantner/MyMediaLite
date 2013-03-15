// Copyright (C) 2011, 2012, 2013 Zeno Gantner
// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
using System.Linq;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval.Measures;
using MyMediaLite.ItemRecommendation;

/*! \namespace MyMediaLite.Eval
 *  \brief This namespace contains evaluation routines.
 */
namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class for item recommendation</summary>
	public static class Items
	{
		/// <summary>the evaluation measures for item prediction offered by the class</summary>
		/// <remarks>
		/// The evaluation measures currently are:
		/// <list type="bullet">
		///   <item><term>AUC</term><description>area under the ROC curve</description></item>
		///   <item><term>prec@5</term><description>precision at 5</description></item>
		///   <item><term>prec@10</term><description>precision at 10</description></item>
		///   <item><term>MAP</term><description>mean average precision</description></item>
		///   <item><term>recall@5</term><description>recall at 5</description></item>
		///   <item><term>recall@10</term><description>recall at 10</description></item>
		///   <item><term>NDCG</term><description>normalizad discounted cumulative gain</description></item>
		///   <item><term>MRR</term><description>mean reciprocal rank</description></item>
		/// </list>
		/// An item recommender is better than another according to one of those measures its score is higher.
		/// </remarks>
		static public ICollection<string> Measures
		{
			get {
				string[] measures = { "AUC", "prec@5", "prec@10", "MAP", "recall@5", "recall@10", "NDCG", "MRR" };
				return new HashSet<string>(measures);
			}
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
		/// <param name="test_users">a list of integers with all test users; if null, use all users in the test cases</param>
		/// <param name="candidate_items">a list of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		/// <param name="repeated_events">allow repeated events in the evaluation (i.e. items accessed by a user before may be in the recommended list)</param>
		/// <param name="n">length of the item list to evaluate -- if set to -1 (default), use the complete list, otherwise compute evaluation measures on the top n items</param>
		/// <returns>a dictionary containing the evaluation results (default is false)</returns>
		static public ItemRecommendationEvaluationResults Evaluate(
			this IRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback training,
			IList<int> test_users = null,
			IList<int> candidate_items = null,
			CandidateItems candidate_item_mode = CandidateItems.OVERLAP,
			RepeatedEvents repeated_events = RepeatedEvents.No,
			int n = -1)
		{
			switch (candidate_item_mode)
			{
				case CandidateItems.TRAINING: candidate_items = training.AllItems; break;
				case CandidateItems.TEST:     candidate_items = test.AllItems; break;
				case CandidateItems.OVERLAP:  candidate_items = new List<int>(test.AllItems.Intersect(training.AllItems)); break;
				case CandidateItems.UNION:    candidate_items = new List<int>(test.AllItems.Union(training.AllItems)); break;
			}
			if (candidate_items == null)
				throw new ArgumentNullException("candidate_items");
			if (test_users == null)
				test_users = test.AllUsers;

			int num_users = 0;
			var result = new ItemRecommendationEvaluationResults();

			// make sure that the user matrix is completely initialized before entering parallel code
			var training_user_matrix = training.UserMatrix;
			var test_user_matrix     = test.UserMatrix;

			Parallel.ForEach(test_users, user_id => {
				try
				{
					var correct_items = new HashSet<int>(test_user_matrix[user_id]);
					correct_items.IntersectWith(candidate_items);
					if (correct_items.Count == 0)
						return;

					var ignore_items_for_this_user = new HashSet<int>(
						repeated_events == RepeatedEvents.Yes ? new int[0] : training_user_matrix[user_id]
					);
					ignore_items_for_this_user.IntersectWith(candidate_items);
					int num_candidates_for_this_user = candidate_items.Count - ignore_items_for_this_user.Count;
					if (correct_items.Count == num_candidates_for_this_user)
						return;

					var prediction = recommender.Recommend(user_id, candidate_items:candidate_items, n:n, ignore_items:ignore_items_for_this_user);
					var prediction_list = (from t in prediction select t.Item1).ToArray();

					int num_dropped_items = num_candidates_for_this_user - prediction.Count;
					double auc  = AUC.Compute(prediction_list, correct_items, num_dropped_items);
					double map  = PrecisionAndRecall.AP(prediction_list, correct_items);
					double ndcg = NDCG.Compute(prediction_list, correct_items);
					double rr   = ReciprocalRank.Compute(prediction_list, correct_items);
					var positions = new int[] { 5, 10 };
					var prec   = PrecisionAndRecall.PrecisionAt(prediction_list, correct_items, positions);
					var recall = PrecisionAndRecall.RecallAt(prediction_list, correct_items, positions);

					// thread-safe incrementing
					lock (result)
					{
						num_users++;
						result["AUC"]       += (float) auc;
						result["MAP"]       += (float) map;
						result["NDCG"]      += (float) ndcg;
						result["MRR"]       += (float) rr;
						result["prec@5"]    += (float) prec[5];
						result["prec@10"]   += (float) prec[10];
						result["recall@5"]  += (float) recall[5];
						result["recall@10"] += (float) recall[10];
					}

					if (num_users % 1000 == 0)
						Console.Error.Write(".");
					if (num_users % 60000 == 0)
						Console.Error.WriteLine();
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw;
				}
			});

			foreach (string measure in Measures)
				result[measure] /= num_users;
			result["num_users"] = num_users;
			result["num_lists"] = num_users;
			result["num_items"] = candidate_items.Count;

			return result;
		}

		/// <summary>Computes the AUC fit of a recommender on the training data</summary>
		/// <returns>the AUC on the training data</returns>
		/// <param name='recommender'>the item recommender to evaluate</param>
		/// <param name="test_users">a list of integers with all test users; if null, use all users in the test cases</param>
		/// <param name="candidate_items">a list of integers with all candidate items</param>
		/// <param name="candidate_item_mode">the mode used to determine the candidate items</param>
		public static double ComputeFit(
			this ItemRecommender recommender,
			IList<int> test_users = null,
			IList<int> candidate_items = null,
			CandidateItems candidate_item_mode = CandidateItems.OVERLAP)
		{
			return recommender.Evaluate(
				recommender.Feedback, recommender.Feedback,
				test_users, candidate_items,
				candidate_item_mode, RepeatedEvents.Yes)["RMSE"];
		}
	}
}
