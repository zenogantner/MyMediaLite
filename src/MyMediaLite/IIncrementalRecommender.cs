// Copyright (C) 2012 Zeno Gantner
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
// 

namespace MyMediaLite
{
	/// <summary>
	/// Interface for recommenders that support incremental model updates.
	/// </summary>
	public interface IIncrementalRecommender
	{
		/// <summary>Remove all feedback by one user</summary>
		/// <param name='user_id'>the user ID</param>
		void RemoveUser(int user_id);

		/// <summary>Remove all feedback by one item</summary>
		/// <param name='item_id'>the item ID</param>
		void RemoveItem(int item_id);

		/// <summary>true if users shall be updated when doing incremental updates</summary>
		/// <remarks>
		/// Default should be true.
		/// Set to false if you do not want any updates to the user model parameters when doing incremental updates.
		/// </remarks>
		bool UpdateUsers { get; set; }

		/// <summary>true if items shall be updated when doing incremental updates</summary>
		/// <remarks>
		/// Set to false if you do not want any updates to the item model parameters when doing incremental updates.
		/// </remarks>
		bool UpdateItems { get; set; }
	}
}

