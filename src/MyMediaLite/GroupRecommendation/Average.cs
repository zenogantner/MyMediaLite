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
	/// <summary>Group recommender that averages user scores</summary>
	public class Average : GroupRecommender
	{
		///
		public Average(IRecommender recommender) : base(recommender) { }

		///
		public override IList<int> RankItems(ICollection<int> users, ICollection<int> items)
		{
			var average_scores = new Dictionary<int, float>();

			foreach (int i in items)
			{
				int count = 0;
				average_scores[i] = 0;
				foreach (int u in users)
					if (recommender.CanPredict(u, i))
					{
						average_scores[i] += recommender.Predict(u, i);
						count++;
					}
				if (count > 0)
					average_scores[i] /= users.Count;
				else
					average_scores[i] = float.MinValue;
			}

			var ranked_items = new List<int>(items);
			ranked_items.Sort(delegate(int i1, int i2) { return average_scores[i2].CompareTo(average_scores[i1]); } );

			return ranked_items;
		}
	}
}

