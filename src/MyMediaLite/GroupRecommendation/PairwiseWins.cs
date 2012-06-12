// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.Data;

namespace MyMediaLite.GroupRecommendation
{
	/// <summary>A simple Condorcet-style voting mechanism</summary>
	/// <remarks>runtime complexity O(|U| |I|^2)</remarks>
	public class PairwiseWins : GroupRecommender
	{
		///
		public PairwiseWins(IRecommender recommender) : base(recommender) { }

		///
		public override IList<int> RankItems(ICollection<int> users, ICollection<int> items)
		{
			var scores_by_user = new float[users.Count, items.Count];

			var users_array = users.ToArray();
			var items_array = items.ToArray();
			
			for (int u = 0; u < users.Count; u++)
				for (int i = 0; i < items.Count; i++)
				{
					int user_id = users_array[u];
					int item_id = items_array[i];
					scores_by_user[u, i] = recommender.Predict(user_id, item_id);
				}

			var wins_by_item = new Dictionary<int, int>();
			foreach (int item_id in items)
				wins_by_item[item_id] = 0;

			for (int u = 0; u < users.Count; u++)
				for (int i = 0; i < items.Count; i++)
					for (int j = 0; j < items.Count; j++)
						if (scores_by_user[u, i] > scores_by_user[u, j])
							wins_by_item[items_array[i]]++;

			var ranked_items = new List<int>(items);
			ranked_items.Sort(delegate(int i1, int i2) { return wins_by_item[i2].CompareTo(wins_by_item[i1]); } );

			return ranked_items;
		}
	}
}
