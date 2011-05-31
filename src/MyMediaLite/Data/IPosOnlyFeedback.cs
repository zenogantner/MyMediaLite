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

using System.Collections.Generic;
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Interface for implicit, positive-only user feedback</summary>
	public interface IPosOnlyFeedback
	{
		/// <summary>By-user access, users are stored in the rows, items in the culumns</summary>
		IBooleanMatrix UserMatrix { get; }

		/// <summary>By-item access, items are stored in the rows, users in the culumns</summary>
		IBooleanMatrix ItemMatrix { get; }

		/// <summary>the maximum user ID</summary>
		int MaxUserID { get; }

		/// <summary>the maximum item ID</summary>
		int MaxItemID { get; }

		/// <summary>the number of feedback events</summary>
		int Count { get; }

		/// <summary>all users that have given feedback</summary>
		ICollection<int> AllUsers { get; }
		
		/// <summary>all items mentioned at least once</summary>
		ICollection<int> AllItems { get; }
		
		/// <summary>Add a user-item event to the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		void Add(int user_id, int item_id);

		/// <summary>Remove a user-item event from the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		void Remove(int user_id, int item_id);
		
		/// <summary>Remove all feedback by a given user</summary>
		/// <param name="user_id">the user id</param>
		void RemoveUser(int user_id);

		/// <summary>Remove all feedback about a given item</summary>
		/// <param name="item_id">the item ID</param>
		void RemoveItem(int item_id);

		/// <summary>Compute the number of overlapping events in two feedback datasets</summary>
		/// <param name="s">the feedback dataset to compare to</param>
		/// <returns>the number of overlapping events, i.e. events that have the same user and item ID</returns>
		int Overlap(IPosOnlyFeedback s);
	}
}

