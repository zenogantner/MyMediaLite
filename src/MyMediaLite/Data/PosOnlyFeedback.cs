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

using System;
using System.Collections.Generic;
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Data structure for implicit, positive-only user feedback</summary>
	/// <remarks>
	/// This data structure does currently NOT support online updates.
	/// Adding this functionality is trivial, though.
	/// </remarks>
	public class PosOnlyFeedback
	{
		/// <summary>By-user access, users are stored in the rows, items in the culumns</summary>
		public SparseBooleanMatrix UserMatrix { get; private set; }

		/// <summary>By-item access, items are stored in the rows, users in the culumns</summary>		
		public SparseBooleanMatrix ItemMatrix
		{
			get {
				if (item_matrix == null)
					BuildItemMatrix();

				return item_matrix;
			}
		}
		SparseBooleanMatrix item_matrix;

		/// <summary>Create a PosOnlyFeedback object</summary>
		public PosOnlyFeedback()
		{
			UserMatrix = new SparseBooleanMatrix();
		}

		void BuildItemMatrix()
		{
			item_matrix = new SparseBooleanMatrix();
			for (int i = 0; i < UserMatrix.NumberOfRows; i++)
				foreach (int j in UserMatrix[i])
					item_matrix[j, i] = true;
		}

		/// <summary>Add a user-item event to the data structure</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		public void Add(int user_id, int item_id)
		{
			UserMatrix[user_id, item_id] = true;
		}
	}
}