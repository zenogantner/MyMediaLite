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

    public class Ratings
    {
		// TODO: remark: Theoretically, we could hold several ratings (of different value) for each user-item combination.
		//               However, FindRating does not support it, and it is strange that we do not allow the user to
		//               give the same rating twice.
		//               We should consider using a lighter backend than HashSet, e.g. a list datatype.
		//               As we do not have performance problems at the moment, we will do this at some point in the future.
		//               Zeno Gantner, 2009-02-17

        private HashSet<RatingEvent> ratingSet = new HashSet<RatingEvent>();
        private double sum_rating = 0;

        public int Count {
			get { return ratingSet.Count; }
		}

		public double Average {
			get { return sum_rating / Count; }
		}

		public double Sum {
			get { return sum_rating; }
		}

		public IEnumerator GetEnumerator()
		{
			return ratingSet.GetEnumerator();
		}

		public List<RatingEvent> ToList()
		{
			return ratingSet.ToList();
		}

		public void AddRating(RatingEvent rating)
        {
            sum_rating += rating.rating;
            ratingSet.Add(rating);
        }

		public void ChangeRating(RatingEvent rating, double new_rating)
        {
            sum_rating += new_rating - rating.rating;
        }

        public void RemoveRating(RatingEvent rating)
        {
            ratingSet.Remove(rating);
            sum_rating -= rating.rating;
        }

        public RatingEvent FindRating(int user_id, int item_id)
        {
            foreach (RatingEvent rating in ratingSet)
            {
                if ((rating.user_id == user_id) && (rating.item_id == item_id))
                {
                    return rating;
                }
            }
            return null;
        }

		public HashSet<int> GetUsers()
		{
			HashSet<int> users = new HashSet<int>();
			foreach (RatingEvent rating in ratingSet)
				users.Add(rating.user_id);
			return users;
		}

		public HashSet<int> GetItems()
		{
			HashSet<int> items = new HashSet<int>();
			foreach (RatingEvent rating in ratingSet)
				items.Add(rating.item_id);
			return items;
		}
    }

    /// <remarks>
    /// Data storage for rating data.
    /// The rating events are accessible in user-wise, item-wise and unsorted triple-wise order.
    /// In order to save memory, the different access modes can be deactivated by constructor parameters.
    /// </remarks>
    /// <author>Steffen Rendle, University of Hildesheim</author>
    public class RatingData
    {
        public Ratings all = null;
        public List<Ratings> byUser = null;
        public List<Ratings> byItem = null;

        public RatingData(bool hasAll, bool hasUser, bool hasItem)
        {
            if (hasAll) {
                all = new Ratings();
            }
            if (hasUser) {
                byUser = new List<Ratings>();
            }
            if (hasItem) {
                byItem = new List<Ratings>();
            }
        }

        public void AddRating(RatingEvent rating)
        {
            if (byItem != null)
            {
                AddUser(rating.user_id);
                byUser[(int)rating.user_id].AddRating(rating);
            }
            if (byItem != null)
            {
                AddItem(rating.item_id);
                byItem[(int)rating.item_id].AddRating(rating);
            }
            if (all != null)
            {
                all.AddRating(rating);
            }

        }

        public void AddUser(int entity_id)
        {
            if (byUser != null)
            {
                while (entity_id >= byUser.Count)
                {
                    Ratings ratings = new Ratings();
                    byUser.Add(ratings);
                }
            }
        }

        public void AddItem(int entity_id)
        {
            if (byItem != null)
            {
                while (entity_id >= byItem.Count)
                {
                    Ratings ratings = new Ratings();
                    byItem.Add(ratings);
                }
            }
        }

        public void RemoveRating(RatingEvent rating)
        {
            if ((byUser != null) && (rating.user_id < byUser.Count))
            {
                byUser[(int)rating.user_id].RemoveRating(rating);
            }
            if ((byItem != null) && (rating.item_id < byItem.Count))
            {
                byItem[(int)rating.item_id].RemoveRating(rating);
            }
            if ((all != null))
            {
                all.RemoveRating(rating);
            }
        }

        public void RemoveUser(int user_id)
        {
            if (byUser != null)
            {
                Ratings r = byUser[(int)user_id];
                List<RatingEvent> temp = r.ToList();
                while (temp.Count > 0)
                {
                    RemoveRating(temp[0]);
                }
            }
            else if ((byItem != null) || (all != null))
            {
                throw new Exception("data storage out of sync");
            }
        }

        public void RemoveItem(int item_id)
        {
            if (byItem != null)
            {
                Ratings r = byItem[(int)item_id];
                List<RatingEvent> temp = r.ToList();
                while (temp.Count > 0)
                    RemoveRating(temp[0]);
            }
            else if ((byUser != null) || (all != null))
            {
                throw new Exception("data storage out of sync");
            }
        }

        public void ChangeRating(RatingEvent rating, double new_rating)
        {
            if ((byUser != null) && (rating.user_id < byUser.Count))
                byUser[(int)rating.user_id].ChangeRating(rating, new_rating);
            if ((byItem != null) && (rating.item_id < byItem.Count))
                byItem[(int)rating.item_id].ChangeRating(rating, new_rating);
            if ((all != null))
                all.ChangeRating(rating, new_rating);

            rating.rating = new_rating;
        }

        public RatingEvent FindRating(int user_id, int item_id)
        {
            int cnt_user = Int32.MaxValue;
            int cnt_item = Int32.MaxValue;
            if ((byUser != null) && (byUser.Count > user_id))
                cnt_user = byUser[(int)user_id].Count;
            if ((byItem != null) && (byItem.Count > item_id))
                cnt_item = byItem[(int)item_id].Count;

            if (cnt_user < cnt_item)
                return byUser[(int)user_id].FindRating(user_id, item_id);
			else if (cnt_user > cnt_item)
                return byItem[(int)item_id].FindRating(user_id, item_id);
			else if (cnt_user < Int32.MaxValue)
                return byUser[(int)user_id].FindRating(user_id, item_id);
            else if (all != null)
                return all.FindRating(user_id, item_id);
			else
                return null;
        }
    }
}
