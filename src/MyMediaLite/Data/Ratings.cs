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
using System.Collections;
using System.Collections.Generic;

namespace MyMediaLite.Data
{
	// TODO use this for optimizing away the ByUser or ByItem indices
	//public enum RatingDataOrg { UNKNOWN, RANDOM, BY_USER, BY_ITEM }

	// TODO optimize some index accesses via slicing

	/// <summary>Data structure for storing ratings</summary>
	/// <remarks>
	/// Small memory overhead for added flexibility.
	/// 
	/// This data structure supports online updates.
	/// </remarks>
	public class Ratings : IRatings
	{
		/// <inheritdoc/>
		public IList<int> Users { get; protected set; }
		/// <inheritdoc/>
		public IList<int> Items { get; protected set; }
		
		/// <inheritdoc/>
		protected IList<double> Values;

		/// <inheritdoc/>
		public virtual double this[int index]
		{
			get {
				return Values[index];
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		/// <inheritdoc/>
		public virtual int Count { get { return Values.Count; } }

		//public RatingDataOrg organization = RatingDataOrg.UNKNOWN;

		/// <summary>Create a new Ratings object</summary>
		public Ratings()
		{
			Users  = new List<int>();
			Items  = new List<int>();
			Values = new List<double>();
		}

		/// <inheritdoc/>
		public int MaxUserID { get; protected set; }
		/// <inheritdoc/>
		public int MaxItemID { get; protected set; }

		/// <inheritdoc/>
		public IList<IList<int>> ByUser
		{
			get {
				if (by_user == null)
					BuildUserIndices();
				return by_user;
			}
		}
		IList<IList<int>> by_user;

		/// <inheritdoc/>
		public void BuildUserIndices()
		{
			by_user = new IList<int>[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				by_user[u] = new List<int>();

			for (int index = 0; index < Count; index++)
				by_user[Users[index]].Add(index);
		}
	
		/// <inheritdoc/>
		public IList<IList<int>> ByItem
		{
			get {
				if (by_item == null)
					BuildItemIndices();
				return by_item;
			}
		}
		IList<IList<int>> by_item;

		/// <inheritdoc/>
		public void BuildItemIndices()
		{
			by_item = new IList<int>[MaxItemID + 1];
			for (int i = 0; i <= MaxItemID; i++)
				by_item[i] = new List<int>();

			for (int index = 0; index < Count; index++)
				by_item[Items[index]].Add(index);
		}

		/// <inheritdoc/>
		public IList<int> RandomIndex
		{
			get {
				if (random_index == null || random_index.Length != Count)
					BuildRandomIndex();

				return random_index;
			}
		}
		private int[] random_index;

		/// <inheritdoc/>
		public void BuildRandomIndex()
		{
			random_index = new int[Count];
			for (int index = 0; index < Count; index++)
				random_index[index] = index;
			Util.Utils.Shuffle<int>(random_index);
		}

		/// <inheritdoc/>
		public IList<int> CountByUser
		{
			get {
				if (count_by_user == null)
					BuildByUserCounts();
				return count_by_user;
			}
		}
		IList<int> count_by_user;

		/// <inheritdoc/>
		public void BuildByUserCounts()
		{
			count_by_user = new int[MaxUserID + 1];
			for (int index = 0; index < Count; index++)
				count_by_user[Users[index]]++;
		}		
		
		/// <inheritdoc/>
		public IList<int> CountByItem
		{
			get {
				if (count_by_item == null)
					BuildByItemCounts();
				return count_by_item;
			}
		}
		IList<int> count_by_item;

		/// <inheritdoc/>
		public void BuildByItemCounts()
		{
			count_by_item = new int[MaxItemID + 1];
			for (int index = 0; index < Count; index++)
				count_by_item[Items[index]]++;
		}		
		
		// TODO speed up
		/// <inheritdoc/>
		public double Average
		{
			get {
				double sum = 0;
				for (int index = 0; index < Count; index++)
					sum += this[index];
				return (double) sum / Count;
			}
		}

		// TODO think whether we want to have a set or a list here
		/// <inheritdoc/>
		public HashSet<int> AllUsers
		{
			get {
				var result_set = new HashSet<int>();
				for (int index = 0; index < Users.Count; index++)
					result_set.Add(Users[index]);
				return result_set;
			}
		}

		/// <inheritdoc/>
		public HashSet<int> AllItems
		{
			get {
				var result_set = new HashSet<int>();
				for (int index = 0; index < Items.Count; index++)
					result_set.Add(Items[index]);
				return result_set;
			}
		}

		/// <inheritdoc/>
		public HashSet<int> GetUsers(IList<int> indices)
		{
			var result_set = new HashSet<int>();
			foreach (int index in indices)
				result_set.Add(Users[index]);
			return result_set;
		}

		/// <inheritdoc/>
		public HashSet<int> GetItems(IList<int> indices)
		{
			var result_set = new HashSet<int>();
			foreach (int index in indices)
				result_set.Add(Items[index]);
			return result_set;
		}

		/// <inheritdoc/>
		public virtual double this[int user_id, int item_id]
		{
			get {
				// TODO speed up
				for (int index = 0; index < Values.Count; index++)
					if (Users[index] == user_id && Items[index] == item_id)
						return Values[index];
				throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
			}
		}

		/// <inheritdoc/>
		public double Get(int user_id, int item_id)
		{
			return this[user_id, item_id];
		}

		/// <inheritdoc/>
		public virtual bool TryGet(int user_id, int item_id, out double rating)
		{
			rating = double.NegativeInfinity;
			// TODO speed up
			for (int index = 0; index < Values.Count; index++)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = Values[index];
					return true;
				}

			return false;
		}

		/// <inheritdoc/>
		public virtual double Get(int user_id, int item_id, ICollection<int> indexes)
		{
			// TODO speed up
			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
					return Values[index];

			throw new Exception(string.Format("rating {0}, {1} not found.", user_id, item_id));
		}

		/// <inheritdoc/>
		public virtual bool TryGet(int user_id, int item_id, ICollection<int> indexes, out double rating)
		{
			rating = double.NegativeInfinity;

			// TODO speed up
			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = Values[index];
					return true;
				}

			return false;
		}

		/// <inheritdoc/>
		public bool TryGetIndex(int user_id, int item_id, out int index)
		{
			index = -1;

			// TODO speed up
			for (int i = 0; i < Count; i++)
				if (Users[i] == user_id && Items[i] == item_id)
				{
					index = i;
					return true;
				}

			return false;
		}

		/// <inheritdoc/>
		public bool TryGetIndex(int user_id, int item_id, ICollection<int> indexes, out int index)
		{
			index = -1;

			// TODO speed up
			foreach (int i in indexes)
				if (Users[i] == user_id && Items[i] == item_id)
				{
					index = i;
					return true;
				}

			return false;
		}

		/// <inheritdoc/>
		public int GetIndex(int user_id, int item_id)
		{
			// TODO speed up
			for (int i = 0; i < Count; i++)
				if (Users[i] == user_id && Items[i] == item_id)
					return i;

			throw new KeyNotFoundException(string.Format("index {0}, {1} not found.", user_id, item_id));
		}

		/// <inheritdoc/>
		public int GetIndex(int user_id, int item_id, ICollection<int> indexes)
		{
			// TODO speed up
			foreach (int i in indexes)
				if (Users[i] == user_id && Items[i] == item_id)
					return i;

			throw new KeyNotFoundException(string.Format("index {0}, {1} not found.", user_id, item_id));
		}

		/// <inheritdoc/>
		public virtual void Add(int user_id, int item_id, float rating)
		{
			Add(user_id, item_id, (double) rating);
		}		
		
		/// <inheritdoc/>
		public virtual void Add(int user_id, int item_id, byte rating)
		{
			Add(user_id, item_id, (double) rating);
		}
		
		/// <inheritdoc/>
		public virtual void Add(int user_id, int item_id, double rating)
		{
			Users.Add(user_id);
			Items.Add(item_id);
			Values.Add(rating);

			if (user_id > MaxUserID)
				MaxUserID = user_id;

			if (item_id > MaxItemID)
				MaxItemID = item_id;
		}

		/// <inheritdoc/>
		public virtual void RemoveAt(int index)
		{
			Users.RemoveAt(index);
			Items.RemoveAt(index);
			Values.RemoveAt(index);
		}
		
		/// <inheritdoc/>
		public virtual void RemoveUser(int user_id)
		{
			for (int index = 0; index < Count; index++)
				if (Users[index] == user_id)
				{
					Users.RemoveAt(index);
					Items.RemoveAt(index);
					Values.RemoveAt(index);
				}
			
			if (MaxUserID == user_id)
				MaxUserID--;
		}

		/// <inheritdoc/>
		public virtual void RemoveItem(int item_id)
		{
			for (int index = 0; index < Count; index++)
				if (Items[index] == item_id)
				{
					Users.RemoveAt(index);
					Items.RemoveAt(index);
					Values.RemoveAt(index);
				}
			
			if (MaxItemID == item_id)
				MaxItemID--;
		}		
		
		/// <inheritdoc/>
		public bool IsReadOnly { get { return true; } }
		
		/// <inheritdoc/>
		public void Add(double item) { throw new NotSupportedException(); }
		
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }
		
		/// <inheritdoc/>
		public bool Contains(double item) { throw new NotSupportedException(); }

		/// <inheritdoc/>
		public void CopyTo(double[] array, int index) { throw new NotSupportedException(); }		
		
		/// <inheritdoc/>
		public int IndexOf(double item) { throw new NotSupportedException(); }
		
		/// <inheritdoc/>
		public void Insert(int index, double item) { throw new NotSupportedException(); }

		/// <inheritdoc/>
		public bool Remove(double item) { throw new NotSupportedException(); }
			
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() { throw new NotSupportedException(); }

		/// <inheritdoc/>
		IEnumerator<double> IEnumerable<double>.GetEnumerator() { throw new NotSupportedException(); }
		
	}
}