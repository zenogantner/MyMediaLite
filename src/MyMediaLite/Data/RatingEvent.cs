// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

namespace MyMediaLite.Data
{
	/// <summary>Representation of a rating event, consisting of a user ID, an item ID, and the value of the rating</summary>
    public class RatingEvent
    {
		/// <summary>the user ID</summary>
        public int user_id;

		/// <summary>the item ID</summary>
        public int item_id;

		/// <summary>the rating value</summary>
        public double rating;

		/// <summary>Default constructor</summary>
		public RatingEvent() { }

		/// <summary>Create a RatingEvent object from given data</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <param name="rating">the rating value</param>
		public RatingEvent(int user_id, int item_id, double rating)
		{
			this.user_id = user_id;
			this.item_id = item_id;
			this.rating  = rating;
		}
    }
}