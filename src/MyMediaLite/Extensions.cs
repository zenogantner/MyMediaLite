// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using MyMediaLite.DataType;

namespace MyMediaLite
{
	/// <summary>Extension methods for IRecommender</summary>
	public static class Extensions
	{
		/// <summary>Predict items for a given user</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="user_id">the numerical ID of the user</param>
		/// <param name="candidate_items">a collection of numerical IDs of candidate items</param>
		/// <returns>an ordered list of items, the most likely item first</returns>
		static public IList<int> PredictItems(this IRecommender recommender, int user_id, IList<int> candidate_items)
		{
			var scored_items = ScoreItems(recommender, user_id, candidate_items);
			scored_items = scored_items.OrderByDescending(x => x.Second).ToArray();

			var result = new int[scored_items.Count];
			for (int i = 0; i < result.Length; i++)
				result[i] = scored_items[i].First;

			return result;
		}
		
		/// <summary>Score items for a given user</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="user_id">the numerical ID of the user</param>
		/// <param name="candidate_items">a collection of numerical IDs of candidate items</param>
		/// <returns>a list of pairs, each pair consisting of the item ID and the predicted score</returns>
		static public IList<Pair<int, float>> ScoreItems(this IRecommender recommender, int user_id, IList<int> candidate_items)
		{
			var result = new Pair<int, float>[candidate_items.Count];
			for (int i = 0; i < candidate_items.Count; i++)
			{
				int item_id = candidate_items[i];
				result[i] = new Pair<int, float>(item_id, recommender.Predict(user_id, item_id));
			}
			return result;
		}
	}
}

