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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyMediaLite.data
{
    /// <remarks>
    /// Data storage for rating data.
    /// The rating events are accessible in user-wise, item-wise and unsorted triple-wise order.
    ///
    /// In order to save memory, the object initially stores only the unsorted ratings.
    /// If at some point user or item-wise access is needed, the underlying data structures
    /// are transparently created on-the-fly.
    /// </remarks>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class RatingData
    {
        private Ratings all = new Ratings();
		/// <summary>
		/// All ratings
		/// </summary>
        public Ratings All { get { return all; } }

		/// <summary>
		/// Ratings by user
		/// </summary>
		public List<Ratings> ByUser
		{
			get
			{
				if (this.byUser == null)
					InitByUser();

				return this.byUser;
			}
		}
		private List<Ratings> byUser = null;

		/// <summary>
		/// Ratings by item
		/// </summary>
		public List<Ratings> ByItem
		{
			get
			{
				if (this.byItem == null)
					InitByItem();

				return this.byItem;
			}
		}
		private List<Ratings> byItem = null;

		/// <summary>
		/// The maximum user ID in the ratings
		/// </summary>
		public int MaxUserID { get { return max_user_id; } }
		private int max_user_id = 0;
		
		/// <summary>
		/// The maximum item ID in the ratings
		/// </summary>
		public int MaxItemID { get { return max_item_id; } }
		private int max_item_id = 0;

		/// <summary>
		/// The average rating value in the collection
		/// </summary>
		public double Average { get { return all.Average; } }
		
		/// <summary>The number of ratings in the collection</summary>
		public int Count { get { return all.Count; } }		
		
		private void InitByUser()
		{
			this.byUser = new List<Ratings>();
			foreach (RatingEvent rating in all)
            {
                AddUser(rating.user_id);
                byUser[(int)rating.user_id].AddRating(rating);
            }
		}

		private void InitByItem()
		{
			this.byItem = new List<Ratings>();
			foreach (RatingEvent rating in all)
            {
                AddItem(rating.item_id);
                byItem[(int)rating.item_id].AddRating(rating);
            }
		}

		/// <summary>
		/// Returns an enumerator for use in foreach loops
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/> containing the elements of this.All
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return all.GetEnumerator();
		}

		/// <summary>
		/// Add a rating event to the collection
		/// </summary>
		/// <param name="rating">
		/// the rating event
		/// </param>
        public void AddRating(RatingEvent rating)
        {
            if (byUser != null)
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
                all.AddRating(rating);

			if (rating.user_id > max_user_id)
				max_user_id = rating.user_id;
			if (rating.item_id > max_item_id)
				max_item_id = rating.item_id;
        }

		/// <summary>
		/// Add a user - reserve resources for a new user
		/// </summary>
		/// <param name="user_id">
		/// the user ID
		/// </param>
        public void AddUser(int user_id)
        {
            if (byUser != null)
            {
                while (user_id >= byUser.Count)
                {
                    Ratings ratings = new Ratings();
                    byUser.Add(ratings);
                }
            }
        }
		/// <summary>
		/// Add an item - reserve resources for a new item
		/// </summary>
		/// <param name="item_id">
		/// the item ID
		/// </param>
        public void AddItem(int item_id)
        {
            if (byItem != null)
            {
                while (item_id >= byItem.Count)
                {
                    Ratings ratings = new Ratings();
                    byItem.Add(ratings);
                }
            }
        }

		/// <summary>
		/// Remove a rating from the collection
		/// </summary>
		/// <param name="rating">
		/// </param>
        public void RemoveRating(RatingEvent rating)
        {
            if ((byUser != null) && (rating.user_id < byUser.Count))
                byUser[(int)rating.user_id].RemoveRating(rating);
            if ((byItem != null) && (rating.item_id < byItem.Count))
                byItem[(int)rating.item_id].RemoveRating(rating);
            if ((all != null))
                all.RemoveRating(rating);
        }

		/// <summary>
		/// Remove a user and all their ratings from the collection
		/// </summary>
		/// <param name="user_id">
		/// the numerical ID of the user
		/// </param>
        public void RemoveUser(int user_id)
        {
            if (byUser != null)
				foreach (RatingEvent r in ByUser[user_id])
                    RemoveRating(r);
            else if ((byItem != null) || (all != null))
                throw new Exception("Data storage is out of sync.");
        }

		/// <summary>
		/// Remove an item and all its ratings from the collection
		/// </summary>
		/// <param name="item_id">
		/// the numerical ID of the item
		/// </param>
        public void RemoveItem(int item_id)
        {
            if (byItem != null)
                foreach (RatingEvent r in ByItem[item_id])
                    RemoveRating(r);
            else if ((byUser != null) || (all != null))
                throw new Exception("Data storage is out of sync.");
        }

		/// <summary>
		/// Change a rating value in the collection
		/// </summary>
		/// <param name="rating">
		/// the rating event
		/// </param>
		/// <param name="new_rating">
		/// the new rating value
		/// </param>
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

		/// <summary>
		/// Find the rating value for a given user and item
		/// </summary>
		/// <param name="user_id">
		/// the numerical ID of the user
		/// </param>
		/// <param name="item_id">
		/// the numerical ID of the user
		/// </param>
		/// <returns>
		/// the rating event corresponding to the given user and item
		/// </returns>
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
