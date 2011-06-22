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
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Rating data structure for ratings with time stamps</summary>
	/// <remarks>This data structure does NOT support incremental updates.</remarks>
	public class StaticRatingsWithDateTime : StaticRatings
	{
		/// <summary>List of DateTime values for each rating event</summary>
		public IList<DateTime> DateTimes { get; private set; }

		/// <summary>Create a new StaticRatingsWithDateTime object</summary>
		/// <param name="size">the number of ratings</param>
		public StaticRatingsWithDateTime(int size) : base(size)
		{
			DateTimes = new DateTime[size];
		}

		///
		public void Add(int user_id, int item_id, double rating, DateTime datetime)
		{
			DateTimes[pos] = datetime; // must be before base.Add because pos changes in there ...
			base.Add(user_id, item_id, rating);
		}
	}
}