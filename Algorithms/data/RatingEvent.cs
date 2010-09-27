// Copyright (C) 2010 Zeno Gantner
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Helper data structures used by some of the MyMedia engines
/// </summary>
/// <author>Steffen Rendle, University of Hildesheim</author>
namespace MyMediaLite.data
{
	/// <summary>
	/// Representation of a rating event, consisting of a user ID, an item ID,
	/// and the value of the rating.
	/// </summary>
	/// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class RatingEvent
    {
        public int user_id;
        public int item_id;
        public double rating;

		public RatingEvent() { }
		public RatingEvent(int user_id, int item_id, double rating)
		{
			this.user_id = user_id;
			this.item_id = item_id;
			this.rating  = rating;
		}
    }
}
