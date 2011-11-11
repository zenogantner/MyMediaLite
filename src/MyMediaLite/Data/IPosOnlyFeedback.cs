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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Interface for implicit, positive-only user feedback</summary>
	public interface IPosOnlyFeedback : IDataSet
	{
		/// <summary>By-user access, users are stored in the rows, items in the culumns</summary>
		/// <remarks>should be implemented as lazy data structure</remarks>
		IBooleanMatrix UserMatrix { get; }

		/// <summary>By-item access, items are stored in the rows, users in the culumns</summary>
		/// <remarks>should be implemented as lazy data structure</remarks>
		IBooleanMatrix ItemMatrix { get; }

		/// <summary>Add a user-item event to the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		void Add(int user_id, int item_id);

		/// <summary>Get a copy of the item matrix</summary>
		/// <returns>a copy of the item matrix</returns>
		IBooleanMatrix GetItemMatrixCopy();

		/// <summary>Get a copy of the user matrix</summary>
		/// <returns>a copy of the user matrix</returns>
		IBooleanMatrix GetUserMatrixCopy();

		/// <summary>Remove a user-item event from the data structure</summary>
		/// <remarks>
		/// If no event for the given user-item combination exists, nothing happens.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		void Remove(int user_id, int item_id);

		/// <summary>Get the transpose of the dataset (users and items exchanged)</summary>
		/// <returns>the transpose of the dataset</returns>
		IPosOnlyFeedback Transpose();
	}
}

