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
	// TODO create 2 versions of this: 1 using ListProxy, and one that copies the data
	
	public class ByUserReaders
	{
		private readonly object syncLock = new object();
		
		// TODO make readonly?
		private IList<IInteraction> InteractionList { get; set; }
		private int MaxUserID { get; set; }

		public ByUserReaders(IList<IInteraction> interaction_list, int max_user_id)
		{
			InteractionList = interaction_list;
			MaxUserID = max_user_id;
			BuildUserIndices();
		}

		///
		public IInteractionReader this[int user_id]
		{
			get {
				lock(syncLock)
				{
					if (by_user_indices == null)
						BuildUserIndices();
					if (user_id >= by_user_indices.Count)
						throw new ArgumentOutOfRangeException();
					return new InteractionReader(
						new ListProxy<IInteraction>(InteractionList, by_user_indices[user_id]),
						user_singletons[user_id],
						by_user_items[user_id]);
				}
			}
		}
		/// <summary>Indices organized by user</summary>
		private IList<IList<int>> by_user_indices;
		private IList<ISet<int>> by_user_items;
		private IList<ISet<int>> user_singletons;

		void BuildUserIndices()
		{
			by_user_indices = new List<IList<int>>();
			by_user_items = new List<ISet<int>>();
			user_singletons = new List<ISet<int>>();
			for (int u = 0; u <= MaxUserID; u++) // create arrays and have a nicer loop
			{
				by_user_indices.Add(new List<int>());
				by_user_items.Add(new HashSet<int>());
				user_singletons.Add(new HashSet<int>(new int[] { u }));
			}

			// one pass over the data
			for (int index = 0; index < InteractionList.Count; index++)
			{
				int user_id = InteractionList[index].User;
				int item_id = InteractionList[index].Item;
				by_user_indices[user_id].Add(index);
				by_user_items[user_id].Add (item_id);
			}
		}
	}
}

