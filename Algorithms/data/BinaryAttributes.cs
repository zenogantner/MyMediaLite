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
	// TODO obsolete documentation

	/// <remarks>
	/// A class for loading and representing binary entity attributes in-memory in recommender engines.
	///
	/// You need to add 5 lines to your engine class definition in order to use attributes for one entity
	/// type. An example follows.
	///
	/// In the class definition:
	/// <code>
	///     protected EntityAttributesBinary item_attributes;
	///     public int num_item_attributes = 0;
	/// </code>
	///
	/// In ReadData(), usually after <c>base.ReadData();</c>:
	/// <code>
	///     item_attributes = new EntityAttributesBinary(max_item_id);
	///		item_attributes.ReadData(backend.GetEntity(EntityType.CatalogItem), num_item_attributes);
	///		max_item_id = Math.Max(max_item_id, item_attributes.max_entity_id);
	/// </code>
	/// </remarks>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class BinaryAttributes
	{
		SparseBooleanMatrix data;

		public int MaxEntityID  {
			get;
			private set;
		}

		public BinaryAttributes(SparseBooleanMatrix attr)
		{
			this.SetAttributes(attr);
		}

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
			return data.GetRow(entity_id);
		}
	}
}

