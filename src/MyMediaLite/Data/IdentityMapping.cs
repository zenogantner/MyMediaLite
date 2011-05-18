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
using System.Linq;

namespace MyMediaLite.Data
{
	/// <summary>Identity mapping for entity IDs: Every original ID is mapped to itself</summary>
	public sealed class IdentityMapping : IEntityMapping
	{
		private int MaxEntityID { get; set; }
		
		/// <inheritdoc/>
		public ICollection<int> OriginalIDs {
			get {
				// TODO maybe there is an elegant LINQ expression to generate this list?
				var id_list = new int[MaxEntityID + 1];
				for (int i = 0; i <= MaxEntityID; i++)
					id_list[i] = i;
					
				return id_list;
			}
		}

		/// <inheritdoc/>
		public ICollection<int> InternalIDs { get { return OriginalIDs; } }

		/// <inheritdoc/>
		public int ToOriginalID(int internal_id)
		{
			MaxEntityID = Math.Max(MaxEntityID, internal_id);
			return internal_id;
		}

		/// <inheritdoc/>
		public int ToInternalID(int original_id)
		{
			MaxEntityID = Math.Max(MaxEntityID, original_id);
			return original_id;
		}

		/// <inheritdoc/>
		public IList<int> ToOriginalID(IList<int> internal_id_list)
		{
			MaxEntityID = Math.Max(MaxEntityID, internal_id_list.Max());
			return new List<int>(internal_id_list);
		}

		/// <inheritdoc/>		
		public IList<int> ToInternalID(IList<int> original_id_list)
		{
			MaxEntityID = Math.Max(MaxEntityID, original_id_list.Max());
			return new List<int>(original_id_list);
		}
	}
}
