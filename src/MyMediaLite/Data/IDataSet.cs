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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>Interface for different kinds of collaborative filtering data sets</summary>
	/// <remarks>
	/// <para>
	/// Implementing classes/inheriting interfaces are e.g. for rating data and for positive-only implicit feedback.
	/// </para>
	///
	/// <para>
	/// The main feature of a dataset is that it has some kind of order (not explicitly stated)
	/// - random, chronological, user-wise, or item-wise - and that it contains tuples of users and
	/// items (not necessarily unique tuples).
	/// </para>
	///
	/// <para>
	/// Implementing classes and inheriting interfaces can add additional data to each user-item tuple,
	/// e.g. the date/time of an event, location, context, etc., as well as additional index structures
	/// to access the dataset in a certain fashion.
	/// </para>
	/// </remarks>
	public interface IDataSet
	{
		/// <summary>the number of interaction events in the dataset</summary>
		int Count { get; }

		/// <summary>the user entries</summary>
		IList<int> Users { get; }
		/// <summary>the item entries</summary>
		IList<int> Items { get; }

		/// <summary>the maximum user ID in the dataset</summary>
		int MaxUserID { get; }
		/// <summary>the maximum item ID in the dataset</summary>
		int MaxItemID { get; }

		/// <summary>all user IDs in the dataset</summary>
		IList<int> AllUsers { get; }
		/// <summary>all item IDs in the dataset</summary>
		IList<int> AllItems { get; }

		/// <summary>indices by user</summary>
		/// <remarks>Should be implemented as a lazy data structure</remarks>
		IList<IList<int>> ByUser { get; }
		/// <summary>indices by item</summary>
		/// <remarks>Should be implemented as a lazy data structure</remarks>
		IList<IList<int>> ByItem { get; }
		/// <summary>get a randomly ordered list of all indices</summary>
		/// <remarks>Should be implemented as a lazy data structure</remarks>
		IList<int> RandomIndex { get; }

		/// <summary>count by user</summary>
		/// <remarks>Should be implemented as a lazy data structure</remarks>
		IList<int> CountByUser { get; }
		/// <summary>count by item</summary>
		/// <remarks>Should be implemented as a lazy data structure</remarks>
		IList<int> CountByItem { get; }

		/// <summary>Remove all events related to a given user</summary>
		/// <param name="user_id">the user ID</param>
		void RemoveUser(int user_id);

		/// <summary>Remove all events related to a given item</summary>
		/// <param name="item_id">the item ID</param>
		void RemoveItem(int item_id);

		/// <summary>Get all users that are referenced by a given list of indices</summary>
		/// <param name="indices">the indices to take into account</param>
		/// <returns>all users referenced by the list of indices</returns>
		ISet<int> GetUsers(IList<int> indices);
		/// <summary>Get all items that are referenced by a given list of indices</summary>
		/// <param name="indices">the indices to take into account</param>
		/// <returns>all items referenced by the list of indices</returns>
		ISet<int> GetItems(IList<int> indices);

		/// <summary>Get index for a given user and item</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the index of the first event encountered that matches the user ID and item ID</returns>
		int GetIndex(int user_id, int item_id);
		/// <summary>Get index for given user and item</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <param name="indexes">the indexes to look at</param>
		/// <returns>the index of the first event encountered that matches the user ID and item ID</returns>
		int GetIndex(int user_id, int item_id, ICollection<int> indexes);

		/// <summary>Try to get the index for given user and item</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <param name="index">will contain the index of the first event encountered that matches the user ID and item ID</param>
		/// <returns>true if an index was found for the user and item</returns>
		bool TryGetIndex(int user_id, int item_id, out int index);
		/// <summary>Try to get the index for given user and item</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <param name="indexes">the indexes to look at</param>
		/// <param name="index">will contain the index of the first event encountered that matches the user ID and item ID</param>
		/// <returns>true if an index was found for the user and item</returns>
		bool TryGetIndex(int user_id, int item_id, ICollection<int> indexes, out int index);
	}
}

