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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Data structure for implicit, positive-only user feedback</summary>
	/// <remarks>
	/// This data structure supports incremental updates if supported by the type parameter T.
	/// </remarks>
	///
	[Serializable()]
	public class PosOnlyFeedback<T> : DataSet, IPosOnlyFeedback, ISerializable where T : IBooleanMatrix, new()
	{
		/// <summary>By-user access, users are stored in the rows, items in the columns</summary>
		public IBooleanMatrix UserMatrix
		{
			get {
				if (user_matrix == null)
					user_matrix = GetUserMatrixCopy();

				return user_matrix;
			}
		}
		IBooleanMatrix user_matrix;

		/// <summary>By-item access, items are stored in the rows, users in the columns</summary>
		public IBooleanMatrix ItemMatrix
		{
			get {
				if (item_matrix == null)
					item_matrix = GetItemMatrixCopy();

				return item_matrix;
			}
		}
		IBooleanMatrix item_matrix;

		/// <summary>Default constructor</summary>
		public PosOnlyFeedback() : base() { }

		///
		public PosOnlyFeedback(IDataSet dataset) : base(dataset) { }

		///
		public PosOnlyFeedback(SerializationInfo info, StreamingContext context) : base(info, context) { }

		///
		public IBooleanMatrix GetUserMatrixCopy()
		{
			var matrix = new T();
			for (int index = 0; index < Count; index++)
				matrix[Users[index], Items[index]] = true;
			return matrix;
		}

		///
		public IBooleanMatrix GetItemMatrixCopy()
		{
			var matrix = new T();
			for (int index = 0; index < Count; index++)
				matrix[Items[index], Users[index]] = true;
			return matrix;
		}

		/// <summary>Add a user-item event to the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		public void Add(int user_id, int item_id)
		{
			Users.Add(user_id);
			Items.Add(item_id);

			if (user_matrix != null)
				user_matrix[user_id, item_id] = true;
			if (item_matrix != null)
				item_matrix[item_id, user_id] = true;

			if (user_id > MaxUserID)
				MaxUserID = user_id;

			if (item_id > MaxItemID)
				MaxItemID = item_id;
		}

		///
		public void Remove(int user_id, int item_id)
		{
			int index = -1;

			while (TryGetIndex(user_id, item_id, out index))
			{
				Users.RemoveAt(index);
				Items.RemoveAt(index);
			}

			if (user_matrix != null)
				user_matrix[user_id, item_id] = false;
			if (item_matrix != null)
				item_matrix[item_id, user_id] = false;
		}

		/// <summary>Remove the event with a given index</summary>
		/// <param name="index">the index of the event to be removed</param>
		public void Remove(int index)
		{
			int user_id = Users[index];
			int item_id = Items[index];
			Users.RemoveAt(index);
			Items.RemoveAt(index);

			if (!TryGetIndex(user_id, item_id, out index))
			{
				if (user_matrix != null)
					user_matrix[user_id, item_id] = false;
				if (item_matrix != null)
					item_matrix[item_id, user_id] = false;
			}
		}

		/// <summary>Remove all feedback by a given user</summary>
		/// <param name="user_id">the user id</param>
		public override void RemoveUser(int user_id)
		{
			IList<int> indices = new List<int>();
			if (by_user != null)
				indices = ByUser[user_id];
			else if (user_matrix != null)
				indices = new List<int>(user_matrix[user_id]);
			else
				for (int index = 0; index < Count; index++)
					if (Users[index] == user_id)
						indices.Add(index);

			// assumption: indices is sorted
			for (int i = indices.Count - 1; i >= 0; i--)
			{
				Users.RemoveAt(indices[i]);
				Items.RemoveAt(indices[i]);
			}

			if (user_matrix != null)
				user_matrix[user_id].Clear();
			if (item_matrix != null)
				for (int i = 0; i < item_matrix.NumberOfRows; i++)
					item_matrix[i].Remove(user_id);
		}

		/// <summary>Remove all feedback about a given item</summary>
		/// <param name="item_id">the item ID</param>
		public override void RemoveItem(int item_id)
		{
			IList<int> indices = new List<int>();
			if (by_item != null)
				indices = ByItem[item_id];
			else if (item_matrix != null)
				indices = new List<int>(item_matrix[item_id]);
			else
				for (int index = 0; index < Count; index++)
					if (Items[index] == item_id)
						indices.Add(index);

			// assumption: indices is sorted
			for (int i = indices.Count - 1; i >= 0; i--)
			{
				Users.RemoveAt(indices[i]);
				Items.RemoveAt(indices[i]);
			}

			if (user_matrix != null)
				for (int u = 0; u < user_matrix.NumberOfRows; u++)
					user_matrix[u].Remove(item_id);

			if (item_matrix != null)
				item_matrix[item_id].Clear();
		}

		///
		public IPosOnlyFeedback Transpose()
		{
			var transpose = new PosOnlyFeedback<T>();
			transpose.Users = new List<int>(this.Items);
			transpose.Items = new List<int>(this.Users);

			return transpose;
		}
	}
}
