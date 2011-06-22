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
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	// TODO unit tests

	/// <summary>Data structure for implicit, positive-only user feedback</summary>
	/// <remarks>
	/// This data structure supports incremental updates if supported by T.
	/// </remarks>
	public class PosOnlyFeedback<T> : IPosOnlyFeedback where T : IBooleanMatrix, new()
	{
		/// <summary>By-user access, users are stored in the rows, items in the culumns</summary>
		public IBooleanMatrix UserMatrix { get; private set; }

		/// <summary>By-item access, items are stored in the rows, users in the culumns</summary>
		public IBooleanMatrix ItemMatrix
		{
			get {
				if (item_matrix == null)
					item_matrix = (IBooleanMatrix) UserMatrix.Transpose();

				return item_matrix;
			}
		}
		IBooleanMatrix item_matrix;

		/// <summary>the maximum user ID</summary>
		public int MaxUserID { get; private set; }

		/// <summary>the maximum item ID</summary>
		public int MaxItemID { get; private set; }

		/// <summary>the number of feedback events</summary>
		public int Count { get { return UserMatrix.NumberOfEntries; } }

		/// <summary>all users that have given feedback</summary>
		public ICollection<int> AllUsers { get { return UserMatrix.NonEmptyRowIDs; } }

		/// <summary>all items mentioned at least once</summary>
		public ICollection<int> AllItems {
			get {
				if (item_matrix == null)
					return UserMatrix.NonEmptyColumnIDs;
				else
					return ItemMatrix.NonEmptyRowIDs;
			}
		}

		/// <summary>Default constructor</summary>
		public PosOnlyFeedback()
		{
			UserMatrix = new T();
		}

		/// <summary>Create a PosOnlyFeedback object from an existing user-item matrix</summary>
		/// <param name="user_matrix">the user-item matrix</param>
		public PosOnlyFeedback(T user_matrix)
		{
			UserMatrix = user_matrix;
			MaxUserID = user_matrix.NumberOfRows;
			MaxItemID = user_matrix.NumberOfColumns;
		}

		/// <summary>Add a user-item event to the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		public void Add(int user_id, int item_id)
		{
			UserMatrix[user_id, item_id] = true;
			if (item_matrix != null)
				item_matrix[item_id, user_id] = true;

			if (user_id > MaxUserID)
				MaxUserID = user_id;

			if (item_id > MaxItemID)
				MaxItemID = item_id;
		}

		/// <summary>Remove a user-item event from the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		public void Remove(int user_id, int item_id)
		{
			UserMatrix[user_id, item_id] = false;
			if (item_matrix != null)
				item_matrix[item_id, user_id] = false;
		}

		/// <summary>Remove all feedback by a given user</summary>
		/// <param name="user_id">the user id</param>
		public void RemoveUser(int user_id)
		{
			UserMatrix[user_id].Clear();
			if (item_matrix != null)
				for (int i = 0; i < item_matrix.NumberOfRows; i++)
					item_matrix[i].Remove(user_id);
		}

		/// <summary>Remove all feedback about a given item</summary>
		/// <param name="item_id">the item ID</param>
		public void RemoveItem(int item_id)
		{
			for (int u = 0; u < UserMatrix.NumberOfRows; u++)
				UserMatrix[u].Remove(item_id);

			if (item_matrix != null)
				item_matrix[item_id].Clear();
		}

		/// <summary>Compute the number of overlapping events in two feedback datasets</summary>
		/// <param name="s">the feedback dataset to compare to</param>
		/// <returns>the number of overlapping events, i.e. events that have the same user and item ID</returns>
		public int Overlap(IPosOnlyFeedback s)
		{
			return UserMatrix.Overlap(s.UserMatrix);
		}
	}
}