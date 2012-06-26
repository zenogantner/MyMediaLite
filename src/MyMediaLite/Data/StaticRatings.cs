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
	//
	[Serializable()]
	public class StaticRatings : Ratings
	{
		// TODO for better performance, build array-based indices

		/// <summary>The position where the next rating will be stored</summary>
		protected int pos = 0;

		///
		public override int Count { get { return pos; } }

		/// <summary>default constructor</summary>
		public StaticRatings() { }

		///
		public StaticRatings(int size)
		{
			Users  = new int[size];
			Items  = new int[size];
			Values = new float[size];
		}

		///
		public StaticRatings(SerializationInfo info, StreamingContext context)
		{
			Users = (int[]) info.GetValue("Users", typeof(int[]));
			Items = (int[]) info.GetValue("Items", typeof(int[]));
			Values = (float[]) info.GetValue("Values", typeof(float[]));
			Scale  = (RatingScale) info.GetValue("Scale", typeof(RatingScale));

			MaxUserID = Users.Max();
			MaxItemID = Items.Max();

			pos = Values.Count;
		}

		///
		public override void Add(int user_id, int item_id, float rating)
		{
			if (pos >= Values.Count)
				throw new Exception(string.Format("Ratings storage is full, only space for {0} ratings", Count));

			Users[pos]  = user_id;
			Items[pos]  = item_id;
			Values[pos] = rating;

			if (user_id > MaxUserID)
				MaxUserID = user_id;
			if (item_id > MaxItemID)
				MaxItemID = item_id;

			pos++;
		}

		///
		public override void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		///
		public override void RemoveUser(int user_id)
		{
			throw new NotSupportedException();
		}

		///
		public override void RemoveItem(int item_id)
		{
			throw new NotSupportedException();
		}

		///
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Users", this.Users);
			info.AddValue("Items", this.Items);
			info.AddValue("Values", this.Values);
			info.AddValue("Scale", this.Scale);
		}
	}
}

