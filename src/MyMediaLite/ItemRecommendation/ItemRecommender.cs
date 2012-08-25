// Copyright (C) 2011, 2012 Zeno Gantner
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
	public abstract class ItemRecommender : Recommender
	{
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
	}
}