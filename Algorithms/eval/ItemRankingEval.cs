// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System.IO;
using System.Linq;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.item_recommender;
using MyMediaLite.rating_predictor;
using MyMediaLite.util;


namespace MyMediaLite.eval
{
    /// <summary>Evaluation class</summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public static class ItemRankingEval
    {
        /// <summary>
        /// Evaluation for rankings of item recommenders. Computes the AUC and precision at different levels.
        /// User-item combinations that appear in both sets are ignored for the test set, and thus in the evaluation.
        /// 
        /// MAP: C. Manning, P. Raghavan, H. Schütze: Introduction to Information Retrieval, Cambridge University Press, 2008
        /// </summary>
        /// <param name="engine">Item recommender engine</param>
        /// <param name="test">test cases</param>
        /// <param name="train">training data</param>
        /// <param name="relevant_items">a HashSet<int> with all relevant items</param>
        /// <returns>a Dictionary<string, double> containing the evaluation results</returns>
		static public Dictionary<string, double> EvaluateItemRecommender(
			ItemRecommender engine,
			SparseBooleanMatrix test,
			SparseBooleanMatrix train,
			HashSet<int> relevant_items)
        {
			if (train.Overlap(test) > 0)
				Console.Error.WriteLine("WARNING: Overlapping train and test data");

			// compute evaluation measures
            double auc_sum        = 0; // for AUC
			double map_sum        = 0; // for MAP
            int num_users         = 0; // for all
			int hit_count_5       = 0; // for precision@N
			int hit_count_10      = 0; // for precision@N
			int hit_count_15      = 0; // for precision@N
			int num_correct_items = 0; // for recall@N
			double ndcg_sum       = 0; // for nDCG

            foreach (KeyValuePair<int, HashSet<int>> user in test.GetNonEmptyRows())
            {
                int user_id = user.Key;
                HashSet<int> test_items = user.Value;

                int num_eval_items = relevant_items.Count - relevant_items.Intersect(train[user_id]).Count();
	            int num_eval_pairs = (num_eval_items - test_items.Count) * test_items.Count;

				// skip all users that have 0 or #relevant_items test items
				if (num_eval_pairs == 0)
					continue;

				int[] prediction = ItemPrediction.PredictItems(engine, user_id, relevant_items);

                if (prediction.Length != relevant_items.Count)
                    throw new Exception("Not all items have been ranked.");				
				
				num_correct_items += test_items.Count;                 // for recall@N

				int num_correct_pairs = 0;                             // for AUC
	            int hit_count         = 0;                             // for AUC and MAP
				double dcg            = 0;				               // for NDCG
				double idcg           = ComputeIDCG(test_items.Count); // for NDCG
				double avg_prec_sum   = 0;                             // for MAP

				int left_out = 0;
				// start with the highest weighting item
	            for (int i = 0; i < prediction.Length; i++)
				{
					int item_id = prediction[i];
					if (train[user_id, item_id])
					{
						left_out++;
						continue;
					}

					// compute AUC/general hit count part
                    if (!test_items.Contains(item_id))
					{
                        num_correct_pairs += hit_count;
						continue;
					}

		            hit_count++;

					// compute MAP part
					avg_prec_sum += hit_count / (i + 1 - left_out);

					// compute NDCG part
					int rank = i + 1 - left_out;
					dcg += 1 / Math.Log(rank + 1, 2);

					if (i < 5 + left_out)
						hit_count_5++;
					if (i < 10 + left_out)
						hit_count_10++;
					if (i < 15 + left_out)
						hit_count_15++;
                }

                double user_auc  = ((double) num_correct_pairs) / num_eval_pairs;
                double user_ndcg = dcg / idcg;

				auc_sum  += user_auc;
				ndcg_sum += user_ndcg;
				if (hit_count != 0)
					map_sum  += avg_prec_sum / hit_count;
                num_users++;
            }

            double auc  = auc_sum  / num_users;
			double map  = map_sum  / num_users;
			double ndcg = ndcg_sum / num_users;

	        double precision_5 = (double) hit_count_5 / (num_users * 5);

			double precision_10 = (double) hit_count_10 / (num_users * 10);
			double precision_15 = (double) hit_count_15 / (num_users * 15);

			double recall_5  = (double) hit_count_5  / num_correct_items;
			double recall_10 = (double) hit_count_10 / num_correct_items;
			double recall_15 = (double) hit_count_15 / num_correct_items;

			double precision_combined = (precision_5 + precision_10 + precision_15) / 3;
			double recall_combined    = (recall_5 + recall_10 + recall_15) / 3;

			Dictionary<string, double> result = new Dictionary<string, double>();
			result.Add("AUC",     auc);
			result.Add("NDCG",    ndcg);
			result.Add("MAP",     map);
			result.Add("prec@5",  precision_5);
			result.Add("prec@10", precision_10);
			result.Add("prec@15", precision_15);
			result.Add("prec_combined", precision_combined);
			result.Add("recall@5",  recall_5);
			result.Add("recall@10", recall_10);
			result.Add("recall@15", recall_15);
			result.Add("recall_combined", recall_combined);

			return result;
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

		static public ICollection<string> GetRatingPredictionMeasures() {
			string[] measures = { "MAE", "RMSE" };
			return new HashSet<string>(measures);
		}

		static public ICollection<string> GetItemPredictionMeasures() {
			string[] measures = { "AUC", "prec@5", "prec@10", "prec@15", "prec_combined",
				                  "recall@5", "recall@10", "recall@15",	"recall_combined" };
			return new HashSet<string>(measures);
		}
    }
}
