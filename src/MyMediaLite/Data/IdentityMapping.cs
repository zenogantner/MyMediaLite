// Copyright (C) 2011, 2012 Zeno Gantner
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
// astring with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MyMediaLite.Data
{
	/// <summary>Identity mapping for entity IDs: Every original ID is mapped to itself</summary>
	public sealed class IdentityMapping : IMapping
	{
		private int MaxEntityID { get; set; }

		///
		public int NumberOfEntities { get { return MaxEntityID + 1; } }

		///
		public ICollection<string> OriginalIDs {
			get {
				var id_list = new string[MaxEntityID + 1];
				for (int i = 0; i <= MaxEntityID; i++)
					id_list[i] = i.ToString();

				return id_list;
			}
		}

		///
		public ICollection<int> InternalIDs {
			get {
				var id_list = new int[MaxEntityID + 1];
				for (int i = 0; i <= MaxEntityID; i++)
					id_list[i] = i;

				return id_list;
			}
		}

		///
		public string ToOriginalID(int internal_id)
		{
			MaxEntityID = Math.Max(MaxEntityID, internal_id);
			return internal_id.ToString();
		}

		///
		public int ToInternalID(string original_id)
		{
			int id = int.Parse(original_id);
			MaxEntityID = Math.Max(MaxEntityID, id);
			return id;
		}

		///
		public IList<string> ToOriginalID(IList<int> internal_id_list)
		{
			MaxEntityID = Math.Max(MaxEntityID, internal_id_list.Max());

			var original_ids = new string[internal_id_list.Count];
			for (int i = 0; i < internal_id_list.Count; i++)
				original_ids[i] = internal_id_list[i].ToString();

			return original_ids;
		}

		///
		public IList<int> ToInternalID(IList<string> original_id_list)
		{
			var internal_ids = new int[original_id_list.Count];
			for (int i = 0; i < original_id_list.Count; i++)
				internal_ids[i] = int.Parse(original_id_list[i]);

			MaxEntityID = Math.Max(MaxEntityID, internal_ids.Max());

			return internal_ids;
		}
	}
}
