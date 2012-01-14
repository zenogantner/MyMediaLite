// Copyright (C) 2011, 2012 Zeno Gantner
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

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Base class for rating predictors that support incremental training</summary>
	public abstract class IncrementalRatingPredictor : RatingPredictor, IIncrementalRatingPredictor
	{
		///
		public bool UpdateUsers { get; set; }

		///
		public bool UpdateItems { get; set; }

		/// <summary>Default constructor</summary>
		public IncrementalRatingPredictor()
		{
			UpdateUsers = true;
			UpdateItems = true;
		}

		///
		public virtual void AddRating(int user_id, int item_id, float rating)
		{
			if (user_id > MaxUserID)
				AddUser(user_id);
			if (item_id > MaxItemID)
				AddItem(item_id);

			ratings.Add(user_id, item_id, rating);
		}

		///
		public virtual void UpdateRating(int user_id, int item_id, float rating)
		{
			int index;
			if (ratings.TryGetIndex(user_id, item_id, out index))
				ratings[index] = rating;
			else
				throw new Exception(string.Format("Cannot update rating for user {0} and item {1}: No such rating exists.", user_id, item_id));
		}

		///
		public virtual void RemoveRating(int user_id, int item_id)
		{
			int index;
			if (ratings.TryGetIndex(user_id, item_id, out index))
				ratings.RemoveAt(index);
		}

		///
		protected virtual void AddUser(int user_id)
		{
			MaxUserID = Math.Max(MaxUserID, user_id);
		}

		///
		protected virtual void AddItem(int item_id)
		{
			MaxItemID = Math.Max(MaxItemID, item_id);
		}

		///
		public virtual void RemoveUser(int user_id)
		{
			if (user_id == MaxUserID)
				MaxUserID--;
			ratings.RemoveUser(user_id);
		}

		///
		public virtual void RemoveItem(int item_id)
		{
			if (item_id == MaxItemID)
				MaxItemID--;
			ratings.RemoveItem(item_id);
		}
	}
}