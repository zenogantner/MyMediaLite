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
		protected IList<float> Values;

		///
		public virtual float this[int index]
		{
			get {
				return Values[index];
			}
			set {
				throw new NotSupportedException();
			}
		}

		///
		public float MaxRating { get; protected set; }
		///
		public float MinRating { get; protected set; }

		/// <summary>Default constructor</summary>
		public Ratings() : base()
		{
			Values = new List<float>();
			MinRating = float.MaxValue;
			MaxRating = float.MinValue;
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
		public float Average
		{
			get {
				double sum = 0;
				for (int index = 0; index < Count; index++)
					sum += this[index];
				return (float) sum / Count;
			}
		}

		///
		public virtual float this[int user_id, int item_id]
		{
			get {
				for (int index = 0; index < Values.Count; index++)
					if (Users[index] == user_id && Items[index] == item_id)
						return Values[index];
				throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
			}
		}

		///
		public virtual bool TryGet(int user_id, int item_id, out float rating)
		{
			rating = float.NegativeInfinity;
			for (int index = 0; index < Values.Count; index++)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = Values[index];
					return true;
				}

			return false;
		}

		///
		public virtual float Get(int user_id, int item_id, ICollection<int> indexes)
		{
			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
					return Values[index];

			throw new Exception(string.Format("rating {0}, {1} not found.", user_id, item_id));
		}

		///
		public virtual bool TryGet(int user_id, int item_id, ICollection<int> indexes, out float rating)
		{
			rating = float.NegativeInfinity;

			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = Values[index];
					return true;
				}

			return false;
		}

		///
		public virtual void Add(int user_id, int item_id, byte rating)
		{
			Add(user_id, item_id, (float) rating);
		}

		///
		public virtual void Add(int user_id, int item_id, float rating)
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
		public void Add(float item) { throw new NotSupportedException(); }

		///
		public void Clear() { throw new NotSupportedException(); }

		///
		public bool Contains(float item) { throw new NotSupportedException(); }

		///
		public void CopyTo(float[] array, int index) { throw new NotSupportedException(); }

		///
		public int IndexOf(float item) { throw new NotSupportedException(); }

		///
		public void Insert(int index, float item) { throw new NotSupportedException(); }

		///
		public bool Remove(float item) { throw new NotSupportedException(); }

		///
		IEnumerator IEnumerable.GetEnumerator() { throw new NotSupportedException(); }

		///
		IEnumerator<float> IEnumerable<float>.GetEnumerator() { throw new NotSupportedException(); }
	}
}