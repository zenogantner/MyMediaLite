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
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Combine two IRatings objects</summary>
	public class CombinedRatings : Ratings
	{
		/// <summary>Create a CombinedRatings object from to existing IRatings objects</summary>
		/// <param name="ratings1">the first data set</param>
		/// <param name="ratings2">the second data set</param>
		public CombinedRatings(IRatings ratings1, IRatings ratings2)
		{
			Users = new CombinedList<int>(ratings1.Users, ratings2.Users);
			Items = new CombinedList<int>(ratings1.Items, ratings2.Items);
			Values = new CombinedList<double>(ratings1, ratings2);
			
			MaxUserID = Math.Max(ratings1.MaxUserID, ratings2.MaxUserID);
			MaxItemID = Math.Max(ratings1.MaxItemID, ratings2.MaxItemID);
		}
	}
}