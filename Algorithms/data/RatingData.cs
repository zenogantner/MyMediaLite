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
        public Ratings All { get { return all; } }
		
		public List<Ratings> ByUser
		{
			get
			{
				if (byUser == null)
					InitByUser();	
					
				return byUser;
			}
		}
		protected List<Ratings> byUser = null;

		public List<Ratings> ByItem
		{
			get
			{
				if (byUser == null)
					InitByItem();	
					
				return byItem;
			}
		}		
		protected List<Ratings> byItem = null;

		public int max_user_id = 0;
		public int max_item_id = 0;

		private void InitByUser()
		{
			byUser = new List<Ratings>();
			foreach (RatingEvent rating in all)
            {
                AddUser(rating.user_id);
                byUser[(int)rating.user_id].AddRating(rating);
            }
		}

		private void InitByItem()
		{
			byItem = new List<Ratings>();
			foreach (RatingEvent rating in all)
            {
                AddItem(rating.item_id);
                byItem[(int)rating.item_id].AddRating(rating);
            }
		}		

		public IEnumerator GetEnumerator()
		{
			return all.GetEnumerator();
		}		
		
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
                byUser[(int)rating.user_id].RemoveRating(rating);
            if ((byItem != null) && (rating.item_id < byItem.Count))
                byItem[(int)rating.item_id].RemoveRating(rating);
            if ((all != null))
                all.RemoveRating(rating);
        }

        public void RemoveUser(int user_id)
        {
            if (byUser != null)
            {
                Ratings r = byUser[(int)user_id];
                List<RatingEvent> temp = r.ToList();
                while (temp.Count > 0)
                    RemoveRating(temp[0]);
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
		
		
		// TODO think about making those properties
		public double Average()
		{
			return all.Average;
		}
		
		public int Count()
		{
			return all.Count;
		}
    }
}
