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
using MyMediaLite.data;


namespace MyMediaLite.rating_predictor
{
	public abstract class Memory : RatingPredictor
	{
		/// <summary>Maximum user ID</summary>
		public int MaxUserID  { get; protected set;	}
		/// <summary>Maximum item ID</summary>
		public int MaxItemID  {	get; protected set; }

		/// <summary>
		/// public in order to allow more fine-grained access by some programs, e.g. for finding the right number
		/// of iterations w/ MatrixFactorization
		/// </summary>
		public RatingData ratings;

		public override bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID && user_id >= 0 && item_id <= MaxItemID && item_id >= 0);
		}

        /// <inheritdoc/>
        public override void AddRating(int user_id, int item_id, double rating)
        {
            RatingEvent r = new RatingEvent();
            r.user_id = user_id;
            r.item_id = item_id;
            r.rating = rating;
            ratings.AddRating(r);
        }

        /// <inheritdoc/>
        public override void UpdateRating(int user_id, int item_id, double rating)
        {
            RatingEvent r = ratings.FindRating(user_id, item_id);
            if (r == null)
                throw new Exception("Rating not found");
            ratings.ChangeRating(r, rating);
        }

        /// <inheritdoc/>
        public override void RemoveRating(int user_id, int item_id)
        {
            RatingEvent r = ratings.FindRating(user_id, item_id);
            if (r == null)
                throw new Exception("Rating not found");
            ratings.RemoveRating(r);
        }

        /// <inheritdoc/>
        public override void AddUser(int user_id)
        {
            ratings.AddUser(user_id);
			MaxUserID = Math.Max(MaxUserID, user_id);
        }

        /// <inheritdoc/>
        public override void AddItem(int item_id)
        {
            ratings.AddItem(item_id);
			MaxItemID = Math.Max(MaxItemID, item_id);
        }

        /// <inheritdoc/>
        public override void RemoveUser(int user_id)
        {
            ratings.RemoveUser(user_id);
        }

        /// <inheritdoc/>
        public override void RemoveItem(int item_id)
        {
            ratings.RemoveItem(item_id);
        }

		public virtual void SetCollaborativeData(RatingData ratings)
		{
			this.ratings = ratings;

			MaxUserID = ratings.byUser.Count - 1;
			MaxItemID = ratings.byItem.Count - 1;
		}
	}
}

