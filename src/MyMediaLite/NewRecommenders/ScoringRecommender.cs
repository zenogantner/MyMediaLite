// Copyright (C) 2013 Zeno Gantner
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
using System;
using System.Collections.Generic;
using System.Linq;
using C5;

namespace MyMediaLite
{
	public abstract class ScoringRecommender : IRecommender
	{
		public virtual bool SupportsFoldIn { get { return false; } }

		abstract public float Score(int userId, int itemId);

		public System.Collections.Generic.IList<Tuple<int, float>> Recommend(int userId, IEnumerable<int> itemSet, int n)
		{
			// TODO get rid of this, or do in separate methods -- you either want to score all your items, or get a top n recommendation
			if (n == -1)
			{
				var scoredItems = new List<Tuple<int, float>>();
				foreach (int itemId in itemSet)
				{
					float score = Score(userId, itemId);
					scoredItems.Add(Tuple.Create(itemId, score));
				}
				return scoredItems.OrderByDescending(x => x.Item2).ToArray();
			}

			var comparer = new DelegateComparer<Tuple<int, float>>( (a, b) => a.Item2.CompareTo(b.Item2) );
			var heap = new IntervalHeap<Tuple<int, float>>(n, comparer);
			float minRelevantScore = float.MinValue;

			foreach (int itemId in itemSet)
			{
				float score = Score(userId, itemId);
				if (score > minRelevantScore)
				{
					heap.Add(Tuple.Create(itemId, score));
					if (heap.Count > n)
					{
						heap.DeleteMin();
						minRelevantScore = heap.FindMin().Item2;
					}
				}
			}

			var orderedItems = new Tuple<int, float>[heap.Count];
			for (int i = 0; i < orderedItems.Length; i++)
				orderedItems[i] = heap.DeleteMax();
			return orderedItems;
		}

		public virtual System.Collections.Generic.IList<Tuple<int, float>> FoldIn(IUserData userData, IEnumerable<int> itemSet, int n)
		{
			throw new NotSupportedException();
		}
	}
}

