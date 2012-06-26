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

namespace MyMediaLite.Data
{
	/// <summary>Array-based storage for rating data.</summary>
	/// <remarks>
	/// Very memory-efficient.
	///
	/// This data structure does NOT support incremental updates.
	/// </remarks>
	[Serializable()]
	public class StaticByteRatings : StaticRatings
	{
		byte[] byte_values;

		///
		public override float this[int index]
		{
			get {
				return (float) byte_values[index];
			}
			set {
				throw new NotSupportedException();
			}
		}

		///
		public override float this[int user_id, int item_id]
		{
			get {
				// TODO speed up
				for (int index = 0; index < pos; index++)
					if (Users[index] == user_id && Items[index] == item_id)
						return (float) byte_values[index];

				throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
			}
		}

		///
		public StaticByteRatings(int size)
		{
			Users  = new int[size];
			Items  = new int[size];
			byte_values = new byte[size];
		}

		///
		public StaticByteRatings(SerializationInfo info, StreamingContext context)
		{
			Users = (int[]) info.GetValue("Users", typeof(int[]));
			Items = (int[]) info.GetValue("Items", typeof(int[]));
			byte_values = (byte[]) info.GetValue("Values", typeof(byte[]));
			Scale  = (RatingScale) info.GetValue("Scale", typeof(RatingScale));

			MaxUserID = Users.Max();
			MaxItemID = Items.Max();

			pos = byte_values.Length;
		}

		///
		public override void InitScale()
		{
			Scale = new RatingScale(this.byte_values);
		}

		///
		public override void Add(int user_id, int item_id, float rating)
		{
			Add(user_id, item_id, (byte) rating);
		}

		///
		public override void Add(int user_id, int item_id, byte rating)
		{
			if (pos >= byte_values.Length)
				throw new KeyNotFoundException(string.Format("Ratings storage is full, only space for {0} ratings", Count));

			Users[pos]       = user_id;
			Items[pos]       = item_id;
			byte_values[pos] = rating;

			if (user_id > MaxUserID)
				MaxUserID = user_id;
			if (item_id > MaxItemID)
				MaxItemID = item_id;

			pos++;
		}

		///
		public override bool TryGet(int user_id, int item_id, out float rating)
		{
			rating = float.NegativeInfinity;
			// TODO speed up
			for (int index = 0; index < pos; index++)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = (float) byte_values[index];
					return true;
				}

			return false;
		}

		///
		public override float Get(int user_id, int item_id, ICollection<int> indexes)
		{
			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
					return (float) byte_values[index];

			throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
		}

		///
		public override bool TryGet(int user_id, int item_id, ICollection<int> indexes, out float rating)
		{
			rating = float.NegativeInfinity;

			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = (float) byte_values[index];
					return true;
				}

			return false;
		}

		///
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Users", this.Users);
			info.AddValue("Items", this.Items);
			info.AddValue("Values", this.byte_values);
		}
	}
}

