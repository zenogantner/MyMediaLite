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
	public class WeightedBPRSampler : UniformPairSampler
	{
		private IInteractionReader negative_item_sampling_reader;

		public WeightedBPRSampler(IInteractions interactions) : base(interactions)
		{
			negative_item_sampling_reader = interactions.Random;
		}

		/// <summary>Sample another item, given the first one and the user</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the ID of the given item</param>
		/// <param name="other_item_id">the ID of the other item</param>
		/// <returns>true if the given item was already seen by user u</returns>
		public override bool OtherItem(int user_id, int item_id, out int other_item_id)
		{
			var user_items = interactions.ByUser(user_id).Items;
			bool item_is_positive = user_items.Contains(item_id);

			do
			{
				negative_item_sampling_reader.ReadInfinite();
				other_item_id = negative_item_sampling_reader.GetItem();
			}
			while (user_items.Contains(other_item_id) == item_is_positive);

			return item_is_positive;
		}
	}
}

