// Copyright (C) 2010 Steffen Rendle
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

namespace MyMediaLite.data
{
	/// <author>Steffen Rendle, University of Hildesheim</author>
    public class Ratings
    {
        private List<RatingEvent> ratingList;
        private double sum_rating = 0;

		public Ratings()
		{
			 this.ratingList = new List<RatingEvent>();
		}
		
		public Ratings(int num_ratings)
		{
			 this.ratingList = new List<RatingEvent>(num_ratings);
		}
		
        public int Count
		{
			get { return ratingList.Count; }
		}

		public double Average
		{
			get { return sum_rating / Count; }
		}

		public double Sum
		{
			get { return sum_rating; }
		}

		public IEnumerator GetEnumerator()
		{
			return ratingList.GetEnumerator();
		}

		public List<RatingEvent> ToList()
		{
			return ratingList;
		}

		public void AddRating(RatingEvent rating)
        {
            sum_rating += rating.rating;
            ratingList.Add(rating);
        }

		public void ChangeRating(RatingEvent rating, double new_rating)
        {
            sum_rating += new_rating - rating.rating;
        }

        public void RemoveRating(RatingEvent rating)
        {
            ratingList.Remove(rating);
            sum_rating -= rating.rating;
        }

        public RatingEvent FindRating(int user_id, int item_id)
        {
			// TODO think about how to exploit orderings
            foreach (RatingEvent rating in ratingList)
            {
                if ((rating.user_id == user_id) && (rating.item_id == item_id))
                    return rating;
            }
            return null;
        }

		public HashSet<int> GetUsers()
		{
			HashSet<int> users = new HashSet<int>();
			foreach (RatingEvent rating in ratingList)
				users.Add(rating.user_id);
			return users;
		}

		public HashSet<int> GetItems()
		{
			HashSet<int> items = new HashSet<int>();
			foreach (RatingEvent rating in ratingList)
				items.Add(rating.item_id);
			return items;
		}
    }
}
