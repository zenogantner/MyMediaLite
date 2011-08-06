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
		public override IList<int> RankItems(IList<int> users, IList<int> items)
		{
			var maximum_scores = new Dictionary<int, double>();

			foreach (int i in items)
			{
				maximum_scores[i] = double.MinValue;
				foreach (int u in users) // TODO consider taking CanPredict into account
					maximum_scores[i] = Math.Max(maximum_scores[i], recommender.Predict(u, i));
			}

			var ranked_items = new List<int>(items);
			ranked_items.Sort(delegate(int i1, int i2) { return maximum_scores[i2].CompareTo(maximum_scores[i1]); } );

			return ranked_items;
		}
	}
}

