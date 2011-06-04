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
	public interface IItemRecommender : IRecommender
	{
		// <summary>Check if the recommender can produce meaningful recommendations for a given user</summary>
		// <param name="user_id">the user ID</param>
		// <returns>true if the recommender can produce recommendations for the given user, false otherwise</returns>
		// bool CanPredict(int user_id);

		///
		void AddFeedback(int user_id, int item_id);
		///
		void RemoveFeedback(int user_id, int item_id);
		///
		void AddUser(int user_id);
		///
		void AddItem(int item_id);
		///
		void RemoveUser(int user_id);
		///
		void RemoveItem(int item_id);
	}
}