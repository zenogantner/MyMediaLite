// Copyright (C) 2012 Zeno Gantner
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
	/// <summary>
	/// Abstract recommender class implementing default behaviors
	/// </summary>
	public abstract class Recommender : IRecommender
	{
		/// <summary>Maximum user ID</summary>
		public int MaxUserID { get; set; }

		/// <summary>Maximum item ID</summary>
		public int MaxItemID { get; set; }

		/// <summary>create a shallow copy of the object</summary>
		public Object Clone()
		{
			return this.MemberwiseClone();
		}

		///
		public abstract float Predict(int user_id, int item_id);

		///
		public virtual bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID && user_id >= 0 && item_id <= MaxItemID && item_id >= 0);
		}

		///
		public virtual System.Collections.Generic.IList<Tuple<int, float>> Recommend(
			int user_id, int n = -1,
			System.Collections.Generic.ICollection<int> ignore_items = null,
			System.Collections.Generic.ICollection<int> candidate_items = null)
		{
			if (candidate_items == null)
				candidate_items = Enumerable.Range(0, MaxItemID - 1).ToList();
			if (ignore_items == null)
				ignore_items = new int[0];

			System.Collections.Generic.IList<Tuple<int, float>> ordered_items;

			if (n == -1)
			{
				var scored_items = new List<Tuple<int, float>>();
				foreach (int item_id in candidate_items)
					if (!ignore_items.Contains(item_id))
					{
						float score = Predict(user_id, item_id);
						if (score > float.MinValue)
							scored_items.Add(Tuple.Create(item_id, score));
					}
				ordered_items = scored_items.OrderByDescending(x => x.Item2).ToArray();
			}
			else
			{
				var comparer = new DelegateComparer<Tuple<int, float>>( (a, b) => a.Item2.CompareTo(b.Item2) );
				var heap = new IntervalHeap<Tuple<int, float>>(n, comparer);
				float min_relevant_score = float.MinValue;

				foreach (int item_id in candidate_items)
					if (!ignore_items.Contains(item_id))
					{
						float score = Predict(user_id, item_id);
						if (score > min_relevant_score)
						{
							heap.Add(Tuple.Create(item_id, score));
							if (heap.Count > n)
							{
								heap.DeleteMin();
								min_relevant_score = heap.FindMin().Item2;
							}
						}
					}

				ordered_items = new Tuple<int, float>[heap.Count];
				for (int i = 0; i < ordered_items.Count; i++)
					ordered_items[i] = heap.DeleteMax();
			}

			return ordered_items;
		}

		///
		public abstract void Train();

		///
		public virtual void LoadModel(string file) { throw new NotImplementedException(); }

		///
		public virtual void SaveModel(string file) { throw new NotImplementedException(); }

		///
		public override string ToString()
		{
			return this.GetType().Name;
		}
	}
}

