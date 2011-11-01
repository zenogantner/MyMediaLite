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
	/// <summary>Data structure that allows access to selected entries of a rating data structure</summary>
	public class RatingsProxy : Ratings
	{
		/// <summary>Create a RatingsProxy object</summary>
		/// <param name="ratings">a ratings data structure</param>
		/// <param name="indices">an index list pointing to entries in the ratings</param>
		public RatingsProxy(IRatings ratings, IList<int> indices)
		{
			Users  = new ListProxy<int>(ratings.Users, indices);
			Items  = new ListProxy<int>(ratings.Items, indices);
			Values = new ListProxy<double>(ratings, indices);

			MaxUserID = ratings.MaxUserID;
			MaxItemID = ratings.MaxItemID;
			MaxRating = ratings.MaxRating;
			MinRating = ratings.MinRating;
		}
	}
}

