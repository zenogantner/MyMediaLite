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
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Data structure that allows access to selected entries of a timed rating data structure</summary>
	public class TimedRatingsProxy : TimedRatings
	{
		/// <summary>Create a TimedRatingsProxy object</summary>
		/// <param name="ratings">a ratings data structure</param>
		/// <param name="indices">an index list pointing to entries in the ratings</param>
		public TimedRatingsProxy(ITimedRatings ratings, IList<int> indices)
		{
			Users  = new ListProxy<int>(ratings.Users, indices);
			Items  = new ListProxy<int>(ratings.Items, indices);
			Values = new ListProxy<float>(ratings, indices);
			Times  = new ListProxy<DateTime>(ratings.Times, indices);

			MaxUserID = ratings.MaxUserID;
			MaxItemID = ratings.MaxItemID;
			Scale = ratings.Scale;

			EarliestTime = Count > 0 ? Times.Min() : DateTime.MaxValue;
			LatestTime   = Count > 0 ? Times.Max() : DateTime.MinValue;
		}
	}
}

