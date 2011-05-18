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
	/// <summary>Array-based storage for rating data</summary>
	/// <remarks>
	/// Very memory-efficient.
	///
	/// This data structure does NOT support online updates.
	/// </remarks>
	public class StaticFloatRatings : StaticRatings
	{
		float[] float_values;

		/// <inheritdoc/>
		public override double this[int index]
		{
			get {
				return (double) float_values[index];
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <inheritdoc/>
		public override double this[int user_id, int item_id]
		{
			get {
				// TODO speed up
				for (int index = 0; index < pos; index++)
					if (Users[index] == user_id && Items[index] == item_id)
						return (double) float_values[index];

				throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
			}
		}

		/// <inheritdoc/>
		public StaticFloatRatings(int size)
		{
			Users  = new int[size];
			Items  = new int[size];
			float_values = new float[size];
		}

		/// <inheritdoc/>
		public override void Add(int user_id, int item_id, double rating)
		{
			Add(user_id, item_id, (float) rating);
		}

		/// <inheritdoc/>
		public override void Add(int user_id, int item_id, byte rating)
		{
			Add(user_id, item_id, (float) rating);
		}

		/// <inheritdoc/>
		public override void Add(int user_id, int item_id, float rating)
		{
			if (pos == float_values.Length)
				throw new Exception(string.Format("Ratings storage is full, only space for {0} ratings", Count));

			Users[pos]        = user_id;
			Items[pos]        = item_id;
			float_values[pos] = rating;

			if (user_id > MaxUserID)
				MaxUserID = user_id;

			if (item_id > MaxItemID)
				MaxItemID = item_id;

			pos++;
		}

		/// <inheritdoc/>
		public override bool TryGet(int user_id, int item_id, out double rating)
		{
			rating = double.NegativeInfinity;
			// TODO speed up
			for (int index = 0; index < pos; index++)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = (double) float_values[index];
					return true;
				}

			return false;
		}

		/// <inheritdoc/>
		public override double Get(int user_id, int item_id, ICollection<int> indexes)
		{
			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
					return (double) float_values[index];

			throw new KeyNotFoundException(string.Format("rating {0}, {1} not found.", user_id, item_id));
		}

		/// <inheritdoc/>
		public override bool TryGet(int user_id, int item_id, ICollection<int> indexes, out double rating)
		{
			rating = double.NegativeInfinity;

			foreach (int index in indexes)
				if (Users[index] == user_id && Items[index] == item_id)
				{
					rating = (double) float_values[index];
					return true;
				}

			return false;
		}
	}
}