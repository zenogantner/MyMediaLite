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
using System.Text;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.item_recommender;
using MyMediaLite.rating_predictor;
using MyMediaLite.util;


namespace MyMediaLite.eval
{
    /// <summary>Evaluation class</summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public static class Prediction
    {
		static public void WritePredictions(
			RecommenderEngine engine,
			SparseBooleanMatrix train,
			int max_user_id,
			HashSet<int> relevant_items,
			int num_predictions, // -1 if no limit ...
			StreamWriter writer)
		{

			List<int> relevant_users = new List<int>();
			for (int u = 0; u <= max_user_id; u++)
				relevant_users.Add(u);
			WritePredictions(engine, train, relevant_users, relevant_items, num_predictions, writer);
		}

		static public void WritePredictions(
			RecommenderEngine engine,
			SparseBooleanMatrix train,
		    List<int> relevant_users,
			HashSet<int> relevant_items,
			int num_predictions, // -1 if no limit ... TODO why not 0?
			StreamWriter writer)
		{
			foreach (int user_id in relevant_users)
			{
				HashSet<int> ignore_items = train.GetRow(user_id);
				WritePredictions(engine, user_id, relevant_items, ignore_items, num_predictions, writer);
			}
		}

		// TODO think about not using WeightedItems, because there may be some overhead involved ...

		static public void WritePredictions(
			RecommenderEngine engine,
            int user_id,
		    HashSet<int> relevant_items,
		    HashSet<int> ignore_items,
			int num_predictions, // -1 if no limit ...
		    StreamWriter writer)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

            List<WeightedItem> score_list = new List<WeightedItem>();
            foreach (int item_id in relevant_items)
                score_list.Add( new WeightedItem(item_id, engine.Predict(user_id, item_id)));
				
			score_list.Sort(); // TODO actually a heap would be enough
			score_list.Reverse();

			int prediction_count = 0;

			foreach (var wi in score_list)
			{
				// TODO move up the ignore_items check
				if (!ignore_items.Contains(wi.item_id) && wi.weight > double.MinValue)
				{
					writer.WriteLine("{0}\t{1}\t{2}", user_id, wi.item_id, wi.weight.ToString(ni));
					prediction_count++;
				}

				if (prediction_count == num_predictions)
					break;
			}
		}

		// TODO get rid of this method? It is used in BPR-MF's ComputeFit method.
		static public int[] PredictItems(RecommenderEngine engine, int user_id, int max_item_id)
		{
            List<WeightedItem> result = new List<WeightedItem>();
            for (int item_id = 0; item_id < max_item_id + 1; item_id++)
                result.Add( new WeightedItem(item_id, engine.Predict(user_id, item_id)));

			result.Sort();
			result.Reverse();

            int[] return_array = new int[max_item_id + 1];
            for (int i = 0; i < return_array.Length; i++)
            	return_array[i] = result[i].item_id;

            return return_array;
		}

		// TODO this should be part of the real API ...?
		static public int[] PredictItems(RecommenderEngine engine, int user_id, HashSet<int> relevant_items)
		{
            List<WeightedItem> result = new List<WeightedItem>();

            foreach (int item_id in relevant_items)
                result.Add( new WeightedItem(item_id, engine.Predict(user_id, item_id)));

			result.Sort();
			result.Reverse();

            int[] return_array = new int[result.Count];
            for (int i = 0; i < return_array.Length; i++)
            	return_array[i] = result[i].item_id;
            return return_array;
		}
    }
}
