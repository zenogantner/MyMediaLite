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
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	public class ByItemReaders
	{
		private readonly object syncLock = new object();

		private IList<IInteraction> InteractionList { get; set; }
		private int MaxItemID { get; set; }

		public ByItemReaders(IList<IInteraction> interaction_list, int max_item_id)
		{
			InteractionList = interaction_list;
			MaxItemID = max_item_id;
			BuildItemIndices();
		}

		///
		public IInteractionReader this[int item_id]
		{
			get {
				lock(syncLock)
				{
					if (by_item == null)
						BuildItemIndices();
					if (item_id >= by_item.Count)
						throw new ArgumentOutOfRangeException("item_id", string.Format("{0} >= {1}", item_id, by_item.Count));
					return new InteractionReader(
						by_item[item_id],
						by_item_users[item_id],
						item_singletons[item_id]);
				}
			}
		}
		/// <summary>Indices organized by item</summary>
		private IList<IList<IInteraction>> by_item;
		private IList<ISet<int>> by_item_users;
		private IList<ISet<int>> item_singletons;

		void BuildItemIndices()
		{
			by_item = new List<IList<IInteraction>>();
			by_item_users = new List<ISet<int>>();
			item_singletons = new List<ISet<int>>();
			for (int item_id = 0; item_id <= MaxItemID; item_id++) // create arrays and have a nicer loop
			{
				by_item.Add(new List<IInteraction>());
				by_item_users.Add(new HashSet<int>());
				item_singletons.Add(new HashSet<int>(new int[] { item_id }));
			}

			// one pass over the data
			foreach (var interaction in InteractionList)
			{
				int user_id = interaction.User;
				int item_id = interaction.Item;
				by_item[item_id].Add(interaction);
				by_item_users[item_id].Add(user_id);
			}
		}
	}
}

