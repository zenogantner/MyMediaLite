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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Data structure that allows access to selected entries of a rating data structure</summary>
	public class RatingsProxy : Ratings
	{
		IList<int> indices;

		/// <summary>Create a RatingsProxy object</summary>
		/// <param name="ratings">a ratings data structure</param>
		/// <param name="indices">an index list pointing to entries in the ratings</param>
		public RatingsProxy(IRatings ratings, IList<int> indices)
		{
			this.indices = indices;

			Users  = new ListProxy<int>(ratings.Users, indices);
			Items  = new ListProxy<int>(ratings.Items, indices);
			Values = new ListProxy<float>(ratings, indices);

			MaxUserID = Users.Max();
			MaxItemID = Items.Max();
			Scale = ratings.Scale;
		}

		///
		public override void RemoveAt(int index)
		{
			int user_id = Users[index];
			int item_id = Items[index];

			indices.RemoveAt(index);
			UpdateCountsAndIndices(new HashSet<int>() { user_id }, new HashSet<int>() { item_id });
		}

		///
		public override void RemoveUser(int user_id)
		{
			var items_to_update = new HashSet<int>();

			for (int index = 0; index < Count; index++)
				if (Users[index] == user_id)
				{
					items_to_update.Add(Items[index]);
					indices.RemoveAt(index);
					index--; // avoid missing an entry
				}

			UpdateCountsAndIndices(new HashSet<int>() { user_id }, items_to_update);

			if (MaxUserID == user_id)
				MaxUserID--;
		}

		///
		public override void RemoveItem(int item_id)
		{
			var users_to_update = new HashSet<int>();

			for (int index = 0; index < Count; index++)
				if (Items[index] == item_id)
				{
					users_to_update.Add(Users[index]);
					indices.RemoveAt(index);
					index--; // avoid missing an entry
				}

			UpdateCountsAndIndices(users_to_update, new HashSet<int>() { item_id });

			if (MaxItemID == item_id)
				MaxItemID--;
		}
	}
}

