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
        private List<RatingEvent> ratingList = new List<RatingEvent>();
        private double sum_rating = 0;

		/// <summary>
		/// Access an event in the collection directly via an index
		/// </summary>
		/// <param name="index">
		/// the index
		/// </param>
		public RatingEvent this [int index] {
			get {
				return ratingList[index];
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
			Utils.Shuffle<RatingEvent>(ratingList);
		}
		
		/// <summary>
		/// Number of ratings in the collection
		/// </summary>
        public int Count
		{
			get { return ratingList.Count; }
		}

		/// <summary>
		/// The average (mean) rating in the collection
		/// </summary>
		public double Average
		{
			get { return sum_rating / Count; }
		}

		/// <inheritdoc />
		public IEnumerator GetEnumerator()
		{
			return ratingList.GetEnumerator();
		}

		/// <summary>
		/// Add a rating event to the collection.
		/// 
		/// Also updates the statistics.
		/// </summary>
		/// <param name="rating">the <see cref="RatingEvent"/> to add</param>
		public void AddRating(RatingEvent rating)
        {
            sum_rating += rating.rating;
            ratingList.Add(rating);
        }

		/// <summary>
		/// Change a rating event in the collection.
		///
		/// Only updates the statistics, it is assumed that the value is modified through other means afterwards.
		/// </summary>
		/// <param name="rating">a rating event</param>
		/// <param name="new_rating">the new rating value</param>
		public void ChangeRating(RatingEvent rating, double new_rating)
        {
            sum_rating += new_rating - rating.rating;
        }

		/// <summary>
		/// Remove a rating from the collection.
		/// 
		/// Also updates the statistics.
		/// </summary>
		/// <param name="rating">the rating event to remove</param>
        public void RemoveRating(RatingEvent rating)
        {
            ratingList.Remove(rating);
            sum_rating -= rating.rating;
        }

		/// <summary>
		/// Find a rating for a given user and item
		/// </summary>
		/// <param name="user_id">the numerical ID of the user</param>
		/// <param name="item_id">the numerical ID of the item</param>
		/// <returns>the rating event corresponding to the given user and item, null otherwise</returns>
        public RatingEvent FindRating(int user_id, int item_id)
        {
            foreach (RatingEvent rating in ratingList)
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
			foreach (RatingEvent rating in ratingList)
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
			foreach (RatingEvent rating in ratingList)
				items.Add(rating.item_id);
			return items;
		}
    }
}
