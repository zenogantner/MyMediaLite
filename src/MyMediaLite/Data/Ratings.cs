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
using System.Runtime.Serialization;

namespace MyMediaLite.Data
{
	/// <summary>Data structure for storing ratings</summary>
	/// <remarks>
	/// Small memory overhead for added flexibility.
	///
	/// This data structure supports incremental updates.
	/// </remarks>
	///
	[Serializable()]
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
		public RatingScale Scale
		{
			get {
				if (scale == null)
					InitScale();
				return scale;
			}
			protected set {
				scale = value;
			}
		}
		RatingScale scale;

		/// <summary>Default constructor</summary>
		public Ratings() : base()
		{
			Values = new List<float>();
		}

		///
		public Ratings(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Values = (List<float>) info.GetValue("Values", typeof(List<float>));
			Scale  = (RatingScale) info.GetValue("Scale", typeof(RatingScale));
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
		public virtual void InitScale()
		{
			Scale = new RatingScale(this.Values);
		}
		// TODO think about adding SafeAdd method

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
			int user_id = Users[index];
			int item_id = Items[index];

			Users.RemoveAt(index);
			Items.RemoveAt(index);
			Values.RemoveAt(index);

			UpdateCountsAndIndices(new HashSet<int>() { user_id }, new HashSet<int>() { item_id });
		}

		/// <summary>update user- and item-wise counts and indices</summary>
		/// <param name='users'>the modified users</param>
		/// <param name='items'>the modified itemsItems.
		/// </param>
		protected void UpdateCountsAndIndices(ISet<int> users, ISet<int> items)
		{
			// update indices
			if (by_user != null)
				foreach (int user_id in users)
					by_user[user_id] = new List<int>();
			if (by_item != null)
				foreach (int item_id in items)
					by_item[item_id] = new List<int>();

			if (by_user != null || by_item != null)
				for (int i = 0; i < Count; i++) // one pass over the data
				{
					if (by_user != null && users.Contains(Users[i]))
						by_user[Users[i]].Add(i);
					if (by_item != null && items.Contains(Items[i]))
						by_item[Items[i]].Add(i);
				}

			// update counts
			if (count_by_user != null && by_user != null)
				foreach (int user_id in users)
					count_by_user[user_id] = by_user[user_id].Count;
			if (count_by_item != null && by_item != null)
				foreach (int item_id in items)
					count_by_item[item_id] = by_item[item_id].Count;
			if ((count_by_user != null || count_by_item != null) && (by_user == null || by_item == null))
			{
				if (count_by_user != null)
					foreach (int user_id in users)
						count_by_user[user_id] = 0;
				if (count_by_item != null)
					foreach (int item_id in items)
						count_by_item[item_id] = 0;

				for (int i = 0; i < Count; i++) // one pass over the data
				{
					if (count_by_user != null && users.Contains(Users[i]))
						count_by_user[Users[i]]++;
					if (count_by_item != null && items.Contains(Items[i]))
						count_by_item[Items[i]]++;
				}
			}
		}

		///
		public override void RemoveUser(int user_id)
		{
			var items_to_update = new HashSet<int>();

			for (int index = 0; index < Count; index++)
				if (Users[index] == user_id)
				{
					items_to_update.Add(Items[index]);

					Users.RemoveAt(index);
					Items.RemoveAt(index);
					Values.RemoveAt(index);

					index--; // avoid missing an entry
				}

			UpdateCountsAndIndices(new HashSet<int>() { user_id }, items_to_update);

			if (MaxUserID == user_id)
				MaxUserID--;
		}

		///
		public override void RemoveItem(int item_id)
		{
			var users_to_update = new HashSet<int>();

			for (int index = 0; index < Count; index++)
				if (Items[index] == item_id)
				{
					users_to_update.Add(Users[index]);

					Users.RemoveAt(index);
					Items.RemoveAt(index);
					Values.RemoveAt(index);

					index--; // avoid missing an entry
				}

			UpdateCountsAndIndices(users_to_update, new HashSet<int>() { item_id });

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
		IEnumerator IEnumerable.GetEnumerator() { return Values.GetEnumerator(); }

		///
		IEnumerator<float> IEnumerable<float>.GetEnumerator() { return Values.GetEnumerator(); }

		///
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Values", this.Values);
			info.AddValue("Scale", this.Scale);
		}
	}
}