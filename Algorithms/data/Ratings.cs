// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.util;

namespace MyMediaLite.data
{
	/// <summary>Class representing a collection of ratings in a particular order</summary>
    public class Ratings
    {
        private List<RatingEvent> rating_list = new List<RatingEvent>();

		/// <summary>Number of ratings in the collection</summary>
        public int Count { get { return rating_list.Count; }	}

		/// <summary>Average rating value in the collection</summary>
		public double Average
		{
			get
			{
				double sum = 0;
				foreach (RatingEvent r in rating_list)
					sum += r.rating;
				return sum / rating_list.Count;
			}
		}

		/// <summary>
		/// Access an event in the collection directly via an index
		/// </summary>
		/// <param name="index">
		/// the index
		/// </param>
		public RatingEvent this [int index]
		{
			get
			{
				return rating_list[index];
			}
		}

		/// <summary>
		/// Shuffle the order of the rating events
		/// </summary>
		/// <remarks>
		/// Fisher-Yates shuffle
		/// </remarks>
		public void Shuffle()
		{
			Utils.Shuffle<RatingEvent>(rating_list);
		}

		/// <inheritdoc />
		public IEnumerator GetEnumerator()
		{
			return rating_list.GetEnumerator();
		}

		/// <summary>
		/// Add a rating event to the collection.
		/// </summary>
		/// <param name="rating">the <see cref="RatingEvent"/> to add</param>
		public void AddRating(RatingEvent rating)
        {
            rating_list.Add(rating);
        }

		/// <summary>
		/// Remove a rating from the collection.
		/// </summary>
		/// <param name="rating">the rating event to remove</param>
        public void RemoveRating(RatingEvent rating)
        {
            rating_list.Remove(rating);
        }

		/// <summary>
		/// Find a rating for a given user and item
		/// </summary>
		/// <param name="user_id">the numerical ID of the user</param>
		/// <param name="item_id">the numerical ID of the item</param>
		/// <returns>the rating event corresponding to the given user and item, null otherwise</returns>
        public RatingEvent FindRating(int user_id, int item_id)
        {
            foreach (RatingEvent rating in rating_list)
                if ((rating.user_id == user_id) && (rating.item_id == item_id))
                    return rating;
            return null;
        }

		/// <summary>
		/// Get the users in the rating collection
		/// </summary>
		/// <returns>
		/// a collection of numerical user IDs
		/// </returns>
		public HashSet<int> GetUsers()
		{
			HashSet<int> users = new HashSet<int>();
			foreach (RatingEvent rating in rating_list)
				users.Add(rating.user_id);
			return users;
		}
		// TODO use ISet as soon as we support Mono 2.8

		/// <summary>
		/// Get the items in the rating collection
		/// </summary>
		/// <returns>
		/// a collection of numerical item IDs
		/// </returns>
		public HashSet<int> GetItems()
		{
			HashSet<int> items = new HashSet<int>();
			foreach (RatingEvent rating in rating_list)
				items.Add(rating.item_id);
			return items;
		}
    }
}
