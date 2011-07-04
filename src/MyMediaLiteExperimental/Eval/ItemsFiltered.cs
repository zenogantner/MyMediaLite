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
	/// <summary>Evaluation class for filtered item recommendation</summary>
	public static class ItemsFiltered
	{
		// TODO generalize more to save code ...
		// TODO generalize that normal protocol is just an instance of this? Only if w/o performance penalty ...
		
		static public Dictionary<int, ICollection<int>> GetFilteredItems(int user_id, IPosOnlyFeedback test,
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
		/// <param name="relevant_users">a collection of integers with all relevant users</param>
		/// <param name="relevant_items">a collection of integers with all relevant items</param>
		/// <returns>a dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(
			IItemRecommender recommender,
			IPosOnlyFeedback test,
			IPosOnlyFeedback train,
		    SparseBooleanMatrix item_attributes,
		    ICollection<int> relevant_users,
			ICollection<int> relevant_items)
		{
			if (train.Overlap(test) > 0)
				Console.Error.WriteLine("WARNING: Overlapping train and test data");

			SparseBooleanMatrix items_by_attribute = (SparseBooleanMatrix) item_attributes.Transpose();
			
			// compute evaluation measures
			double auc_sum     = 0;
			double map_sum     = 0;
			double prec_5_sum  = 0;
			double prec_10_sum = 0;
			double prec_15_sum = 0;
			double ndcg_sum    = 0;
			int num_lists      = 0;

			foreach (int user_id in relevant_users)
			{
				var filtered_items = GetFilteredItems(user_id, test, item_attributes);
				
				foreach (int attribute_id in filtered_items.Keys)
				{
					// TODO optimize this a bit, currently it is quite naive
					var relevant_filtered_items = new HashSet<int>(items_by_attribute[attribute_id]);
					relevant_filtered_items.IntersectWith(relevant_items);
					
					var correct_items = new HashSet<int>(filtered_items[attribute_id]);
					correct_items.IntersectWith(relevant_filtered_items);
	
					// the number of items that are really relevant for this user
					var relevant_items_in_train = new HashSet<int>(train.UserMatrix[user_id]);
					relevant_items_in_train.IntersectWith(relevant_filtered_items);
					int num_eval_items = relevant_filtered_items.Count - relevant_items_in_train.Count();
	
					// skip all users that have 0 or #relevant_filtered_items test items
					if (correct_items.Count == 0)
						continue;
					if (num_eval_items - correct_items.Count == 0)
						continue;
	
					num_lists++;
					int[] prediction = Prediction.PredictItems(recommender, user_id, relevant_filtered_items);
	
					auc_sum     += Items.AUC(prediction, correct_items, train.UserMatrix[user_id]);
					map_sum     += Items.MAP(prediction, correct_items, train.UserMatrix[user_id]);
					ndcg_sum    += Items.NDCG(prediction, correct_items, train.UserMatrix[user_id]);
					prec_5_sum  += Items.PrecisionAt(prediction, correct_items, train.UserMatrix[user_id],  5);
					prec_10_sum += Items.PrecisionAt(prediction, correct_items, train.UserMatrix[user_id], 10);
					prec_15_sum += Items.PrecisionAt(prediction, correct_items, train.UserMatrix[user_id], 15);
	
					if (prediction.Length != relevant_items.Count)
						throw new Exception("Not all items have been ranked.");
	
					if (num_lists % 1000 == 0)
						Console.Error.Write(".");
					if (num_lists % 20000 == 0)
						Console.Error.WriteLine();
				}
			}

			var result = new Dictionary<string, double>();
			result.Add("AUC",     auc_sum / num_lists);
			result.Add("MAP",     map_sum / num_lists);
			result.Add("NDCG",    ndcg_sum / num_lists);
			result.Add("prec@5",  prec_5_sum / num_lists);
			result.Add("prec@10", prec_10_sum / num_lists);
			result.Add("prec@15", prec_15_sum / num_lists);
			result.Add("num_users", num_lists);
			result.Add("num_items", relevant_items.Count);

			return result;
		}

		// TODO implement online eval
	}
}
