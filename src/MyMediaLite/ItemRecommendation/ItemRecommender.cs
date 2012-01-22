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

/*! \namespace MyMediaLite.ItemRecommendation
 *  \brief This namespace contains item recommenders and some helper classes for item recommendation.
 */
namespace MyMediaLite.ItemRecommendation
{
	/// <summary>
	/// Abstract item recommender class that loads the (positive-only implicit feedback) training data into memory
	/// and provides flexible access to it.
	/// </summary>
	/// <remarks>
	/// The data is stored in two sparse matrices:
	/// one user-wise (in the rows)  and one item-wise.
	///
	/// Positive-only means we only which items a user has accessed/liked, but not which items a user does not like.
	/// If there is not data for a specific item, we do not know whether the user has just not yet accessed the item,
	/// or whether they really dislike it.
	///
	/// See http://recsyswiki/wiki/Item_recommendation and http://recsyswiki/wiki/Implicit_feedback
	/// </remarks>
	public abstract class ItemRecommender : IRecommender
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
				MaxUserID = Math.Max(feedback.MaxUserID, MaxUserID);
				MaxItemID = Math.Max(feedback.MaxItemID, MaxItemID);
			}
		}
		IPosOnlyFeedback feedback;

		/// <summary>create a shallow copy of the object</summary>
		public Object Clone()
		{
			return this.MemberwiseClone();
		}

		///
		public abstract float Predict(int user_id, int item_id);

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
		public override string ToString()
		{
			return this.GetType().Name;
		}
	}
}