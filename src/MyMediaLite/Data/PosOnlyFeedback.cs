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
	/// <summary>Data structure for implicit, positive-only user feedback</summary>
	/// <remarks>
	/// This data structure supports online updates.
	/// </remarks>
	public class PosOnlyFeedback
	{
		/// <summary>By-user access, users are stored in the rows, items in the culumns</summary>
		public SparseBooleanMatrix UserMatrix { get; private set; }

		/// <summary>By-item access, items are stored in the rows, users in the culumns</summary>
		public SparseBooleanMatrix ItemMatrix
		{
			get {
				if (item_matrix == null)
					item_matrix = UserMatrix.Transpose();

				return item_matrix;
			}
		}
		SparseBooleanMatrix item_matrix;

		/// <summary>the maximum user ID</summary>
		public int MaxUserID { get; private set; }

		/// <summary>the maximum item ID</summary>
		public int MaxItemID { get; private set; }

		public int Count { get { return UserMatrix.NumberOfEntries; } }

		
		public ICollection<int> AllUsers { get { return UserMatrix.NonEmptyRowIDs; } }
		
		public ICollection<int> AllItems {
			get {
				if (item_matrix == null)
					return UserMatrix.NonEmptyColumnIDs;
				else
					return ItemMatrix.NonEmptyRowIDs;
			}
		}
		
		/// <summary>Create a PosOnlyFeedback object</summary>
		public PosOnlyFeedback()
		{
			UserMatrix = new SparseBooleanMatrix();
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
		/// <param name="item_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		public void Remove(int user_id, int item_id)
		{
			UserMatrix[user_id, item_id] = false;
			if (item_matrix != null)
				item_matrix[item_id, user_id] = false;
		}

		public void RemoveUser(int user_id)
		{
			UserMatrix[user_id].Clear();
			if (item_matrix != null)
				for (int i = 0; i < item_matrix.NumberOfRows; i++)
					item_matrix[i].Remove(user_id);
		}

		public void RemoveItem(int item_id)
		{
			for (int u = 0; u < UserMatrix.NumberOfRows; u++)
				UserMatrix[u].Remove(item_id);

			if (item_matrix != null)
				item_matrix[item_id].Clear();
		}

		public int Overlap(PosOnlyFeedback s)
		{
			return UserMatrix.Overlap(s.UserMatrix);
		}
	}
}