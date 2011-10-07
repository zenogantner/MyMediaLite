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
using System.Linq;

namespace MyMediaLite.Data
{
	/// <summary>Data structure for storing ratings</summary>
	/// <remarks>
	/// Small memory overhead for added flexibility.
	///
	/// This data structure supports incremental updates.
	/// </remarks>
	public class Ratings : DataSet, IRatings
	{
		///
		protected IList<double> Values;

		///
		public virtual double this[int index]
		{
			get {
				return Values[index];
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <summary>Default constructor</summary>
		public Ratings() : base()
		{
			Values = new List<double>();
			MinRating = double.MaxValue;
			MaxRating = double.MinValue;
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
		IList<int> count_by_user;

		///
		public void BuildByUserCounts()
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
		IList<int> count_by_item;

		///
		public void BuildByItemCounts()
		{
			count_by_item = new int[MaxItemID + 1];
			for (int index = 0; index < Count; index++)
				count_by_item[Items[index]]++;
		}

		///
		public double Average
		{
			get {
				double sum = 0;
				for (int index = 0; index < Count; index++)
					sum += this[index];
				return (double) sum / Count;
			}
		}

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
		public virtual double this[int user_id, int item_id]
		{
			get {
				for (int index = 0; index < Values.Count; index++)
					if (Users[index] == user_id && Items[index] == item_id)
						return Values[index];
				throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
			}
		}

		///
		public double Get(int user_id, int item_id)
		{
			return this[user_id, item_id];
		}

		///
		public virtual bool TryGet(int user_id, int item_id, out double rating)
		{
			rating = double.NegativeInfinity;
			for (int index = 0; index < Values.Count; index++)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = Values[index];
					return true;
				}

			return false;
		}

		///
		public virtual double Get(int user_id, int item_id, ICollection<int> indexes)
		{
			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
					return Values[index];

			throw new Exception(string.Format("rating {0}, {1} not found.", user_id, item_id));
		}

		///
		public virtual bool TryGet(int user_id, int item_id, ICollection<int> indexes, out double rating)
		{
			rating = double.NegativeInfinity;

			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = Values[index];
					return true;
				}

			return false;
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
		public virtual void Add(int user_id, int item_id, float rating)
		{
			Add(user_id, item_id, (double) rating);
		}

		///
		public virtual void Add(int user_id, int item_id, byte rating)
		{
			Add(user_id, item_id, (double) rating);
		}

		///
		public virtual void Add(int user_id, int item_id, double rating)
		{
			Users.Add(user_id);
			Items.Add(item_id);
			Values.Add(rating);

			int pos = Users.Count - 1;

			if (user_id > MaxUserID)
				MaxUserID = user_id;
			if (item_id > MaxItemID)
				MaxItemID = item_id;
			if (rating < MinRating)
				MinRating = rating;
			if (rating > MaxRating)
				MaxRating = rating;

			// update index data structures if necessary
			if (by_user != null)
			{
				for (int u = by_user.Count; u <= user_id; u++)
					by_user.Add(new List<int>());
				by_user[user_id].Add(pos);
			}
			if (by_item != null)
			{
				for (int i = by_item.Count; i <= item_id; i++)
					by_item.Add(new List<int>());
				by_item[item_id].Add(pos);
			}
		}

		///
		public virtual void RemoveAt(int index)
		{
			Users.RemoveAt(index);
			Items.RemoveAt(index);
			Values.RemoveAt(index);
		}

		///
		public override void RemoveUser(int user_id)
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

		///
		public override void RemoveItem(int item_id)
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

		///
		public bool IsReadOnly { get { return true; } }

		///
		public void Add(double item) { throw new NotSupportedException(); }

		///
		public void Clear() { throw new NotSupportedException(); }

		///
		public bool Contains(double item) { throw new NotSupportedException(); }

		///
		public void CopyTo(double[] array, int index) { throw new NotSupportedException(); }

		///
		public int IndexOf(double item) { throw new NotSupportedException(); }

		///
		public void Insert(int index, double item) { throw new NotSupportedException(); }

		///
		public bool Remove(double item) { throw new NotSupportedException(); }

		///
		IEnumerator IEnumerable.GetEnumerator() { throw new NotSupportedException(); }

		///
		IEnumerator<double> IEnumerable<double>.GetEnumerator() { throw new NotSupportedException(); }
	}
}