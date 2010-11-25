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
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.data_type;


namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// Abstract item recommender class, that loads the training data into two sparse matrices: one column-wise and one row-wise
    /// </summary>
    public abstract class Memory : IItemRecommender
    {
		/// <summary>Maximum user ID</summary>
		public int MaxUserID  { get; set;	}
		/// <summary>Maximum item ID</summary>
		public int MaxItemID  {	get; set; }

        /// <summary>Implicit feedback, user-wise</summary>
        protected SparseBooleanMatrix data_user;

		/// <summary>Implicit feedback, item-wise</summary>
        protected SparseBooleanMatrix data_item;

		/// <inheritdoc/>
		public abstract double Predict(int user_id, int item_id);

		/// <inheritdoc/>
		public abstract void Train();

		/// <inheritdoc/>
		public abstract void LoadModel(string filename);

		/// <inheritdoc/>
		public abstract void SaveModel(string filename);

		/// <inheritdoc/>
		public virtual void AddFeedback(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				throw new ArgumentException("Unknown user " + user_id + ". Add it before inserting event data.");
			if (item_id > MaxItemID)
				throw new ArgumentException("Unknown item " + item_id + ". Add it before inserting event data.");

			// update data structures
			HashSet<int> user_items = data_user[user_id];
			user_items.Add(item_id);
			HashSet<int> item_users = data_item[item_id];
			item_users.Add(user_id);
		}

		/// <inheritdoc/>
		public virtual void RemoveFeedback(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				throw new ArgumentException("Unknown user " + user_id);
			if (item_id > MaxItemID)
				throw new ArgumentException("Unknown item " + item_id);

			// update data structures
			HashSet<int> user_items = data_user[user_id];
			user_items.Remove(item_id);
			HashSet<int> item_users = data_item[item_id];
			item_users.Remove(user_id);
		}

		/// <inheritdoc/>
		public virtual void AddUser(int user_id)
		{
			if (user_id > MaxUserID)
				MaxUserID = user_id;
		}

		/// <inheritdoc/>
		public virtual void AddItem(int item_id)
		{
			if (item_id > MaxItemID)
				MaxItemID = item_id;
		}

		/// <inheritdoc/>
		public virtual void RemoveUser(int user_id)
		{
			// remove all mentions of this user from data structures
			HashSet<int> user_items = data_user[user_id];
			foreach (int item_id in user_items)
				data_item[item_id, user_id] = false;
			user_items.Clear();

			if (user_id == MaxUserID)
				MaxUserID--;
		}

		/// <inheritdoc/>
		public virtual void RemoveItem(int item_id)
		{
			// remove all mentions of this item from data structures
			HashSet<int> item_users = data_item[item_id];
			foreach (int user_id in item_users)
				data_user[user_id, item_id] = false;
			item_users.Clear();

			if (item_id == MaxItemID)
				MaxItemID--;
		}

		
		// TODO document
		public void SetCollaborativeData(SparseBooleanMatrix user_items)
		{
            this.data_user = user_items;
            this.data_item = user_items.Transpose();
			
			this.MaxUserID = Math.Max(MaxUserID, data_user.NonEmptyRowIDs.Max());			
			this.MaxItemID = Math.Max(MaxItemID, data_item.NonEmptyRowIDs.Max());
		}
    }
}