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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>Interface for different kinds of collaborative filtering data sets</summary>
	/// <remarks>
	/// Implementing classes/inheriting interfaces are e.g. for rating data and for positive-only implicit feedback.
	///
	/// The main feature of a dataset is that it has some kind of order (not explicitly stated)
	/// - random, chronological, user-wise, or item-wise - and that it contains tuples of users and
	/// items (not necessarily unique tuples).
	///
	/// Implementing classes and inheriting interfaces can add additional data to each user-item tuple,
	/// e.g. the date/time of an event, location, context, etc., as well as additional index structures
	/// to access the dataset in a certain fashion.
	/// </remarks>
	public interface IDataSet
	{
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

		/// <summary>Build the user indices</summary>
		void BuildUserIndices();
		/// <summary>Build the item indices</summary>
		void BuildItemIndices();
		/// <summary>Build the random index</summary>
		void BuildRandomIndex();

		/// <summary>Remove all events related to a given user</summary>
		/// <param name="user_id">the user ID</param>
		void RemoveUser(int user_id);

		/// <summary>Remove all events related to a given item</summary>
		/// <param name="item_id">the item ID</param>
		void RemoveItem(int item_id);
	}
}

