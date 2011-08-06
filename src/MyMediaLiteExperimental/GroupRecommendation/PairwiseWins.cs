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
using MyMediaLite.Data;

namespace MyMediaLite.GroupRecommendation
{
	/// <summary>A simple Condorcet-style voting mechanism</summary>
	public class PairwiseWins : GroupRecommender
	{
		///
		public PairwiseWins(IRecommender recommender) : base(recommender) { }

		///
		public override IList<int> RankItems(IList<int> users, IList<int> items)
		{
			var scores_by_user = new double[users.Count, items.Count];

			foreach (int i in items)
				foreach (int u in users)
					scores_by_user[u, i] = recommender.Predict(u, i);

			var wins_by_item = new Dictionary<int, int>();
			foreach (int i in items)
				wins_by_item[i] = 0;

			foreach (int u in users)
				foreach (int i in items)
					foreach (int j in items)
						if (scores_by_user[u, i] > scores_by_user[u, j])
							wins_by_item[i]++;

			var ranked_items = new List<int>(items);
			ranked_items.Sort(delegate(int i1, int i2) { return wins_by_item[i2].CompareTo(wins_by_item[i1]); } );

			return ranked_items;
		}
	}
}
