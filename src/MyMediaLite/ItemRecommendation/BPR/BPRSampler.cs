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
using MyMediaLite.Data;

namespace MyMediaLite.ItemRecommendation.BPR
{
	public abstract class BPRSampler : IBPRSampler
	{
		protected IInteractions interactions;

		/// <summary>Reference to (per-thread) singleton random number generator</summary>
		[ThreadStatic] // we need one random number generator per thread because synchronizing is slow
		static protected System.Random random;

		protected int max_user_id;
		protected int max_item_id;

		public BPRSampler(IInteractions interactions)
		{
			this.interactions = interactions;
			this.max_user_id = interactions.MaxUserID;
			this.max_item_id = interactions.MaxItemID;
			random = MyMediaLite.Random.GetInstance();
		}

		public abstract void NextTriple(out int u, out int i, out int j);
		//public abstract void ItemPair(int user_id, out int item_id, out int other_item_id);
		public abstract int NextUser();

		/// <summary>Sample another item, given the first one and the user</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the ID of the given item</param>
		/// <param name="other_item_id">the ID of the other item</param>
		/// <returns>true if the given item was already seen by user u</returns>
		public virtual bool OtherItem(int user_id, int item_id, out int other_item_id)
		{
			var user_items = interactions.ByUser(user_id).Items;
			bool item_is_positive = user_items.Contains(item_id);

			do
				other_item_id = random.Next(max_item_id + 1);
			while (user_items.Contains(other_item_id) == item_is_positive);

			return item_is_positive;
		}
	}
}

