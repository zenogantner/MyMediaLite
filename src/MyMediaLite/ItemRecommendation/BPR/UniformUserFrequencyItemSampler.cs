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
using MyMediaLite.Data;

namespace MyMediaLite.ItemRecommendation.BPR
{
	public class UniformUserFrequencyItemSampler : UniformUserSampler
	{
		private IInteractionReader negative_item_sampling_reader;

		public UniformUserFrequencyItemSampler(IInteractions interactions) : base(interactions)
		{
			negative_item_sampling_reader = interactions.Random;
		}

		///
		public override bool OtherItem(ISet<int> user_items, int item_id, out int other_item_id)
		{
			bool item_is_positive = user_items.Contains(item_id);

			do
			{
				negative_item_sampling_reader.ReadInfinite();
				other_item_id = negative_item_sampling_reader.GetItem();
			}
			while (user_items.Contains(other_item_id) == item_is_positive);

			return item_is_positive;
		}
		
		/// <summary>Sample a pair of items, given a user</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the ID of the first item</param>
		/// <param name="other_item_id">the ID of the second item</param>
		protected void ItemPair(int user_id, out int item_id, out int other_item_id)
		{
			var user_items = interactions.ByUser(user_id).Items;
			item_id = user_items.ElementAt(random.Next(user_items.Count));
			OtherItem(user_items, item_id, out other_item_id);
		}

	}
}

