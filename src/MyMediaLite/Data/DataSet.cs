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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

/*! \namespace MyMediaLite.Data
 *  \brief This namespace contains MyMediaLite's principal data structures,
 *  which are used e.g. to store the interaction data that is used to train
 *  personalized recommenders.
 */
namespace MyMediaLite.Data
{
	/// <summary>Abstract dataset class that implements some common functions</summary>
	[Serializable()]
	public abstract class DataSet : IDataSet, ISerializable
	{
		///
		public IList<int> Users { get; protected set; }
		///
		public IList<int> Items { get; protected set; }

		///
		public virtual int Count { get { return Users.Count; } }

		///
		public int MaxUserID { get; protected set; }
		///
		public int MaxItemID { get; protected set; }

		///
		public IList<IList<int>> ByUser
		{
			get {
				if (by_user == null)
					BuildUserIndices();
				return by_user;
			}
		}
		/// <summary>Indices organized by user</summary>
		protected IList<IList<int>> by_user;

		/// <summary>Default constructor</summary>
		public DataSet()
		{
			Users = new List<int>();
			Items = new List<int>();
		}

		/// <summary>
		/// Create new dataset view from an existing one.
		/// Share the underlying data structures, do not copy them.
		/// </summary>
		/// <param name='dataset'>the dataset to build from</param>
		public DataSet(IDataSet dataset)
		{
			Users = dataset.Users;
			Items = dataset.Items;
		}

		///
		public DataSet(SerializationInfo info, StreamingContext context)
		{
			Users = (List<int>) info.GetValue("Users", typeof(List<int>));
			Items = (List<int>) info.GetValue("Items", typeof(List<int>));

			MaxUserID = Users.Max();
			MaxItemID = Items.Max();
		}

		///
		public IList<IList<int>> ByItem
		{
			get {
				if (by_item == null)
					BuildItemIndices();
				return by_item;
			}
		}
		/// <summary>Indices organized by item</summary>
		protected IList<IList<int>> by_item;

		///
		public IList<int> RandomIndex
		{
			get {
				if (random_index == null || random_index.Length != Count)
					BuildRandomIndex();

				return random_index;
			}
		}
		private int[] random_index;

		///
		public IList<int> AllUsers
		{
			get {
				var result_set = new HashSet<int>();
				for (int index = 0; index < Users.Count; index++)
					result_set.Add(Users[index]);
				return result_set.ToArray();
			}
		}

		///
		public IList<int> AllItems
		{
			get {
				var result_set = new HashSet<int>();
				for (int index = 0; index < Items.Count; index++)
					result_set.Add(Items[index]);
				return result_set.ToArray();
			}
		}

		///
		public IList<int> CountByUser
		{
			get {
				if (count_by_user == null)
					BuildByUserCounts();
				return count_by_user;
			}
		}
		/// <summary>field for storing the count per user</summary>
		protected IList<int> count_by_user;

		void BuildByUserCounts()
		{
			count_by_user = new int[MaxUserID + 1];
			for (int index = 0; index < Count; index++)
				count_by_user[Users[index]]++;
		}

		///
		public IList<int> CountByItem
		{
			get {
				if (count_by_item == null)
					BuildByItemCounts();
				return count_by_item;
			}
		}
		/// <summary>field for storing the count per item</summary>
		protected IList<int> count_by_item;

		void BuildByItemCounts()
		{
			count_by_item = new int[MaxItemID + 1];
			for (int index = 0; index < Count; index++)
				count_by_item[Items[index]]++;
		}

		void BuildUserIndices()
		{
			by_user = new List<IList<int>>();
			for (int u = 0; u <= MaxUserID; u++)
				by_user.Add(new List<int>());

			// one pass over the data
			for (int index = 0; index < Count; index++)
				by_user[Users[index]].Add(index);
		}

		void BuildItemIndices()
		{
			by_item = new List<IList<int>>();
			for (int i = 0; i <= MaxItemID; i++)
				by_item.Add(new List<int>());

			// one pass over the data
			for (int index = 0; index < Count; index++)
				by_item[Items[index]].Add(index);
		}

		void BuildRandomIndex()
		{
			if (random_index == null || random_index.Length != Count)
			{
				random_index = new int[Count];
				for (int index = 0; index < Count; index++)
					random_index[index] = index;
			}
			random_index.Shuffle();
		}

		///
		public abstract void RemoveUser(int user_id);

		///
		public abstract void RemoveItem(int item_id);

		///
		public ISet<int> GetUsers(IList<int> indices)
		{
			var result_set = new HashSet<int>();
			foreach (int index in indices)
				result_set.Add(Users[index]);
			return result_set;
		}

		///
		public ISet<int> GetItems(IList<int> indices)
		{
			var result_set = new HashSet<int>();
			foreach (int index in indices)
				result_set.Add(Items[index]);
			return result_set;
		}

		///
		public bool TryGetIndex(int user_id, int item_id, out int index)
		{
			index = -1;

			for (int i = 0; i < Count; i++)
				if (Users[i] == user_id && Items[i] == item_id)
				{
					index = i;
					return true;
				}

			return false;
		}

		///
		public bool TryGetIndex(int user_id, int item_id, ICollection<int> indexes, out int index)
		{
			index = -1;

			foreach (int i in indexes)
				if (Users[i] == user_id && Items[i] == item_id)
				{
					index = i;
					return true;
				}

			return false;
		}

		///
		public int GetIndex(int user_id, int item_id)
		{
			for (int i = 0; i < Count; i++)
				if (Users[i] == user_id && Items[i] == item_id)
					return i;

			throw new KeyNotFoundException(string.Format("index {0}, {1} not found.", user_id, item_id));
		}

		///
		public int GetIndex(int user_id, int item_id, ICollection<int> indexes)
		{
			foreach (int i in indexes)
				if (Users[i] == user_id && Items[i] == item_id)
					return i;

			throw new KeyNotFoundException(string.Format("index {0}, {1} not found.", user_id, item_id));
		}

		///
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Users", this.Users);
			info.AddValue("Items", this.Items);
		}
	}
}
