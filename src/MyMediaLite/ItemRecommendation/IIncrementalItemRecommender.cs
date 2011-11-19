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

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Interface for item recommenders</summary>
	/// <remarks>
	/// Item prediction or item recommendation is the task of predicting items (movies, books, products, videos, jokes)
	/// that a user may like, based on past user behavior (and possibly other information).
	///
	/// See also http://recsyswiki/wiki/Item_prediction
	/// </remarks>
	public interface IIncrementalItemRecommender : IRecommender
	{
		/// <summary>Add a positive feedback event</summary>
		/// <param name='user_id'>the user ID</param>
		/// <param name='item_id'>the item ID</param>
		///
		void AddFeedback(int user_id, int item_id);

		/// <summary>Remove all feedback events by the given user-item combination</summary>
		/// <param name='user_id'>the user ID</param>
		/// <param name='item_id'>the item ID</param>
		void RemoveFeedback(int user_id, int item_id);

		/// <summary>Remove all feedback by one user</summary>
		/// <param name='user_id'>the user ID</param>
		void RemoveUser(int user_id);

		/// <summary>Remove all feedback by one item</summary>
		/// <param name='item_id'>the item ID</param>
		void RemoveItem(int item_id);
	}
}