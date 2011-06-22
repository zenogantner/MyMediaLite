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
using MyMediaLite.Data;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Abstract class for rating predictors that keep the rating data in memory for training (and possibly prediction)</summary>
	public abstract class RatingPredictor : IRatingPredictor, ICloneable
	{
		/// <summary>Maximum user ID</summary>
		public int MaxUserID  { get; set; }

		/// <summary>Maximum item ID</summary>
		public int MaxItemID  {	get; set; }

		/// <summary>The max rating value</summary>
		public virtual double MaxRating { get { return max_rating; } set { max_rating = value; } }
		/// <summary>The max rating value</summary>
		protected double max_rating;

		/// <summary>The min rating value</summary>
		public virtual double MinRating { get { return min_rating; } set { min_rating = value; } }
		/// <summary>The min rating value</summary>
		protected double min_rating;

		/// <summary>true if users shall be updated when doing incremental updates</summary>
		/// <remarks>
		/// Default is true.
		/// Set to false if you do not want any updates to the user model parameters when doing incremental updates.
		/// </remarks>
		public bool UpdateUsers { get; set; }

		/// <summary>true if items shall be updated when doing incremental updates</summary>
		/// <remarks>
		/// Default is true.
		/// Set to false if you do not want any updates to the item model parameters when doing incremental updates.
		/// </remarks>
		public bool UpdateItems { get; set; }

		/// <summary>The rating data</summary>
		public virtual IRatings Ratings { get { return ratings; } set { ratings = value; } }

		/// <summary>rating data</summary>
		protected IRatings ratings;

		/// <summary>Default constructor</summary>
		public RatingPredictor()
		{
			UpdateUsers = true;
			UpdateItems = true;
		}

		/// <summary>create a shallow copy of the object</summary>
		public Object Clone()
		{
			return this.MemberwiseClone();
		}

		///
		public abstract double Predict(int user_id, int item_id);

		/// <summary>Inits the recommender model</summary>
		/// <remarks>
		/// This method is called by the Train() method.
		/// When overriding, please call base.InitModel() to get the functions performed in the base class.
		/// </remarks>
		protected virtual void InitModel()
		{
			MaxUserID = Ratings.MaxUserID;
			MaxItemID = Ratings.MaxItemID;
		}

		///
		public abstract void Train();

		///
		public abstract void SaveModel(string filename);

		///
		public abstract void LoadModel(string filename);

		///
		public virtual bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID && user_id >= 0 && item_id <= MaxItemID && item_id >= 0);
		}

		///
		public virtual void AddRating(int user_id, int item_id, double rating)
		{
			if (user_id > MaxUserID)
				AddUser(user_id);
			if (item_id > MaxItemID)
				AddItem(item_id);

			ratings.Add(user_id, item_id, rating);
		}

		///
		public virtual void UpdateRating(int user_id, int item_id, double rating)
		{
			throw new NotImplementedException();
		}

		///
		public virtual void RemoveRating(int user_id, int item_id)
		{
			throw new NotImplementedException();
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