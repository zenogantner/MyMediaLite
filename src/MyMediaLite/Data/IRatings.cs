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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>Interface for rating datasets</summary>
	public interface IRatings
	{
		// TODO add value fields to all properties

		/// <summary>the user entries</summary>
		IList<int> Users { get; }
		/// <summary>the item entries</summary>
		IList<int> Items { get; }
		/// <summary>the rating value entries</summary>
		IList<double> Values  { get; } // TODO make generic
		/// <summary>get the rating value for a given index</summary>
		/// <param name="index">the index</param>
		double this[int index] { get; }

		/// <summary>the maximum user ID in the dataset</summary>
		int MaxUserID { get; }
		/// <summary>the maximum item ID in the dataset</summary>
		int MaxItemID { get; }

		/// <summary>indices by user</summary>
		IList<IList<int>> ByUser { get; }
		/// <summary>indices by item</summary>
		IList<IList<int>> ByItem { get; }
		/// <summary>get a randomly ordered list of all indices</summary>
		IList<int> RandomIndex { get; }
		// TODO add method to force refresh

		/// <summary>Build the user indices</summary>
		void BuildUserIndices();
		/// <summary>Build the item indices</summary>
		void BuildItemIndices();
		/// <summary>Build the random index</summary>
		void BuildRandomIndex();

		/// <summary>number of ratings in the dataset</summary>
		int Count { get; }
		/// <summary>average rating in the dataset</summary>
		double Average { get; }

		/// <summary>all user IDs in the dataset</summary>
		HashSet<int> AllUsers { get; }
		/// <summary>all item IDs in the dataset</summary>
		HashSet<int> AllItems { get; }

		/// <summary>Get all users that are referenced by a given list of indices</summary>
		/// <param name="indices">the indices to take into account</param>
		/// <returns>the set of users</returns>
		HashSet<int> GetUsers(IList<int> indices);
		/// <summary>Get all items that are referenced by a given list of indices</summary>
		/// <param name="indices">the indices to take into account</param>
		/// <returns>the set of itemss</returns>
		HashSet<int> GetItems(IList<int> indices);

		double this[int user_id, int item_id] { get; }

		double Get(int user_id, int item_id);

		bool TryGet(int user_id, int item_id, out double rating);

		bool TryGet(int user_id, int item_id, ICollection<int> indexes, out double rating);

		double Get(int user_id, int item_id, ICollection<int> indexes);

		int GetIndex(int user_id, int item_id);

		int GetIndex(int user_id, int item_id, ICollection<int> indexes);

		bool TryGetIndex(int user_id, int item_id, out int index);

		bool TryGetIndex(int user_id, int item_id, ICollection<int> indexes, out int index);

		void Add(int user_id, int item_id, double rating); // TODO think about returning the index of the newly added rating

		void RemoveAt(int index);
		
		void RemoveUser(int user_id);
		
		void RemoveItem(int item_id);
	}
}

