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

namespace MyMediaLite.Data
{
	/// <summary>Array-based storage for rating data.</summary>
	/// <remarks>
	/// Very memory-efficient.
	///
	/// This data structure does NOT support online updates.
	/// </remarks>
	public class StaticByteRatings : StaticRatings
	{
		byte[] ByteValues;

		/// <inheritdoc/>
		public override double this[int index]
		{
			get {
				return (double) ByteValues[index];
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <inheritdoc/>
		public StaticByteRatings(int size)
		{
			Users  = new int[size];
			Items  = new int[size];
			ByteValues = new byte[size];
		}

		/// <inheritdoc/>
		public override void Add(int user_id, int item_id, double rating)
		{
			Add(user_id, item_id, (byte) rating);
		}

		/// <inheritdoc/>
		public override void Add(int user_id, int item_id, byte rating)
		{
			if (pos == ByteValues.Length)
				throw new Exception(string.Format("Ratings storage is full, only space fo {0} ratings", Count));

			Users[pos]      = user_id;
			Items[pos]      = item_id;
			ByteValues[pos] = rating;

			if (user_id > MaxUserID)
				MaxUserID = user_id;

			if (item_id > MaxItemID)
				MaxItemID = item_id;

			pos++;
		}
	}
}

