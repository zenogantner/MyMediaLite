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

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Interface for rating predictors which support incremental training</summary>
	/// <remarks>
	/// By incremental training we mean that after each update, the recommender does not
	/// perform a complete re-training using all data, but only a brief update procedure
	/// taking into account the update and only a subset of the existing training data.
	///
	/// This interface does not prevent you from doing a complete re-training when implementing
	/// a new class. This makes sense e.g. for simple average-based models.
	///
	/// This interface assumes that every user can rate every item only once.
	/// </remarks>
	public interface IIncrementalRatingPredictor : IRatingPredictor
	{
		/// <summary>Add a new rating and perform incremental training</summary>
		/// <param name='user_id'>the ID of the user who performed the rating</param>
		/// <param name='item_id'>the ID of the rated item</param>
		/// <param name='rating'>the rating value</param>
		void AddRating(int user_id, int item_id, double rating);

		/// <summary>Update an existing rating and perform incremental training</summary>
		/// <param name='user_id'>the ID of the user who performed the rating</param>
		/// <param name='item_id'>the ID of the rated item</param>
		/// <param name='rating'>the rating value</param>
		void UpdateRating(int user_id, int item_id, double rating);

		/// <summary>Remove an existing rating and perform "incremental" training</summary>
		/// <param name='user_id'>the ID of the user who performed the rating</param>
		/// <param name='item_id'>the ID of the rated item</param>
		void RemoveRating(int user_id, int item_id);

		/// <summary>Remove a user from the recommender model, and delete all their ratings</summary>
		/// <remarks>
		/// It is up to the recommender implementor whether there should be model updates after this
		/// action, both options are valid.
		/// </remarks>
		/// <param name='user_id'>the ID of the user to be removed</param>
		void RemoveUser(int user_id);

		/// <summary>Remove an item from the recommender model, and delete all ratings of this item</summary>
		/// <remarks>
		/// It is up to the recommender implementor whether there should be model updates after this
		/// action, both options are valid.
		/// </remarks>
		/// <param name='item_id'>the ID of the user to be removed</param>
		void RemoveItem(int item_id);
	}
}
