// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.data_type;

namespace MyMediaLite.data
{
	// TODO think about getting rid of this -- all the functionality is already covered by SparseBooleanMatrix
	
	/// <remarks>
	/// A class for loading and representing binary entity attributes in-memory in recommender engines.
	/// </remarks>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class BinaryAttributes
	{
		SparseBooleanMatrix data;

		/// <summary>
		/// The maximum ID of the described entities
		/// </summary>
		public int MaxEntityID  {
			get;
			private set;
		}

		/// <summary>
		/// Constructor. Creates an object of type BinaryAttributes
		/// </summary>
		/// <param name="attr">
		/// a sparse boolean matrix where the rows represent entities and the columns represent attributes
		/// </param>
		public BinaryAttributes(SparseBooleanMatrix attr)
		{
			this.SetAttributes(attr);
		}

		/// <summary>
		/// Get the attribute data
		/// </summary>
		/// <returns>
		/// A <see cref="SparseBooleanMatrix"/>
		/// </returns>
		public SparseBooleanMatrix GetAttributes()
		{
			return data;
		}

		public void SetAttributes(SparseBooleanMatrix attr)
		{
			this.data = attr;
			this.MaxEntityID = attr.GetNumberOfRows();
		}

		// TODO [] access
		public HashSet<int> GetAttributes(int entity_id)
		{
			return data[entity_id];
		}
	}
}
