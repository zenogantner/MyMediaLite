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
	/// <summary>Group recommender that takes the maximum user score as the group score</summary>
	public class Maximum : GroupRecommender
	{
		///
		public Maximum(IRecommender recommender) : base(recommender) { }

		///
		public override IList<int> RankItems(ICollection<int> users, ICollection<int> items)
		{
			var maximum_scores = new Dictionary<int, float>();

			foreach (int item_id in items)
			{
				maximum_scores[item_id] = float.MinValue;
				foreach (int user_id in users)
					if (recommender.CanPredict(user_id, item_id))
						maximum_scores[item_id] = Math.Max(maximum_scores[item_id], recommender.Predict(user_id, item_id));
			}

			var ranked_items = new List<int>(items);
			ranked_items.Sort(delegate(int i1, int i2) { return maximum_scores[i2].CompareTo(maximum_scores[i1]); } );

			return ranked_items;
		}
	}
}

