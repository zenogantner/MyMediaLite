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

		///
		public ICollection<long> OriginalIDs {
			get {
				var id_list = new long[MaxEntityID + 1];
				for (int i = 0; i <= MaxEntityID; i++)
					id_list[i] = i;

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
		public long ToOriginalID(int internal_id)
		{
			MaxEntityID = Math.Max(MaxEntityID, internal_id);
			return internal_id;
		}

		///
		public int ToInternalID(long original_id)
		{
			if (original_id > int.MaxValue)
				throw new ArgumentOutOfRangeException("original_id", "cannot be greater than int.MaxValue");

			MaxEntityID = Math.Max(MaxEntityID, (int) original_id);
			return (int) original_id;
		}

		///
		public IList<long> ToOriginalID(IList<int> internal_id_list)
		{
			MaxEntityID = Math.Max(MaxEntityID, internal_id_list.Max());

			var original_ids = new long[internal_id_list.Count];
			for (int i = 0; i < internal_id_list.Count; i++)
				original_ids[i] = internal_id_list[i];

			return original_ids;
		}

		///
		public IList<int> ToInternalID(IList<long> original_id_list)
		{
			var internal_ids = new int[original_id_list.Count];
			for (int i = 0; i < original_id_list.Count; i++)
			{
				if (original_id_list[i] > int.MaxValue)
					throw new ArgumentOutOfRangeException(string.Format("original_ids[{0}]", i), "cannot be greater than int.MaxValue");

				internal_ids[i] = (int) original_id_list[i];
			}

			MaxEntityID = (int) Math.Max(MaxEntityID, original_id_list.Max());

			return internal_ids;
		}
	}
}
