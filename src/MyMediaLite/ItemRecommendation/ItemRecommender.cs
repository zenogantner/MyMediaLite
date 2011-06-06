// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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

using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Data;
//using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Abstract item recommender class that loads the training data into memory</summary>
	/// <remarks>
	/// The data is stored in two sparse matrices:
	/// one column-wise and one row-wise
	/// </remarks>
	public abstract class ItemRecommender : IItemRecommender, ICloneable
	{
		/// <summary>Maximum user ID</summary>
		public int MaxUserID { get; set; }

		/// <summary>Maximum item ID</summary>
		public int MaxItemID { get; set; }

		/// <summary>the feedback data to be used for training</summary>
		public virtual IPosOnlyFeedback Feedback
		{
			get { return this.feedback; }
			set {
				this.feedback = value;
				MaxUserID = feedback.MaxUserID;
				MaxItemID = feedback.MaxItemID;
			}
		}
		IPosOnlyFeedback feedback;

		/// <summary>create a shallow copy of the object</summary>
		public Object Clone()
		{
			return this.MemberwiseClone();
		}
		
		///
		public abstract double Predict(int user_id, int item_id);

		///
		public virtual bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID && user_id >= 0 && item_id <= MaxItemID && item_id >= 0);
		}

		///
		public abstract void Train();

		///
		public abstract void LoadModel(string filename);

		///
		public abstract void SaveModel(string filename);

		///
		public virtual void AddFeedback(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				AddUser(user_id);
			if (item_id > MaxItemID)
				AddItem(item_id);

			Feedback.Add(user_id, item_id);
		}

		///
		public virtual void RemoveFeedback(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				throw new ArgumentException("Unknown user " + user_id);
			if (item_id > MaxItemID)
				throw new ArgumentException("Unknown item " + item_id);

			Feedback.Remove(user_id, item_id);
		}

		///
		protected virtual void AddUser(int user_id)
		{
			if (user_id > MaxUserID)
				MaxUserID = user_id;
		}

		///
		protected virtual void AddItem(int item_id)
		{
			if (item_id > MaxItemID)
				MaxItemID = item_id;
		}

		///
		public virtual void RemoveUser(int user_id)
		{
			Feedback.RemoveUser(user_id);

			if (user_id == MaxUserID)
				MaxUserID--;
		}

		///
		public virtual void RemoveItem(int item_id)
		{
			Feedback.RemoveItem(item_id);

			if (item_id == MaxItemID)
				MaxItemID--;
		}
	}
}