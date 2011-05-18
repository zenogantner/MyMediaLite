// Copyright (C) 2010, 2011 Zeno Gantner
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

namespace MyMediaLite.Data
{
	/// <summary>Class to map external entity IDs to internal ones to ensure that there are no gaps in the numbering</summary>
	public sealed class EntityMapping : IEntityMapping
	{
		/// <summary>Contains the mapping from the original (external) IDs to the internal IDs</summary>
		/// <remarks>
		/// Never, to repeat NEVER, delete entries from this dictionary!
		/// </remarks>
		Dictionary<int, int> original_to_internal = new Dictionary<int, int>();

		/// <summary>Contains the mapping from the internal IDs to the original (external) IDs</summary>
		/// <remarks>
		/// Never, to repeat NEVER, delete entries from this dictionary!
		/// </remarks>
		Dictionary<int, int> internal_to_original = new Dictionary<int, int>();

		/// <summary>all original (external) entity IDs</summary>
		/// <value>all original (external) entity IDs</value>
		public ICollection<int> OriginalIDs	{ get { return original_to_internal.Keys; } }

		/// <summary>all internal entity IDs</summary>
		/// <value>all internal entity IDs</value>
		public ICollection<int> InternalIDs { get { return internal_to_original.Keys; }	}

		/// <summary>Get original (external) ID of a given entity, if the given internal ID is unknown, throw an exception.</summary>
		/// <param name="internal_id">the internal ID of the entity</param>
		/// <returns>the original (external) ID of the entitiy</returns>
		public int ToOriginalID(int internal_id)
		{
			int original_id;
			if (internal_to_original.TryGetValue(internal_id, out original_id))
				return original_id;
			else
				throw new ArgumentException("Unknown internal ID: " + internal_id);
		}

		/// <summary>Get internal ID of a given entity. If the given external ID is unknown, create a new internal ID for it and store the mapping.</summary>
		/// <param name="original_id">the original (external) ID of the entity</param>
		/// <returns>the internal ID of the entitiy</returns>
		public int ToInternalID(int original_id)
		{
			int internal_id;
			if (original_to_internal.TryGetValue(original_id, out internal_id))
				return internal_id;

			internal_id = original_to_internal.Count;
			original_to_internal.Add(original_id, internal_id);
			internal_to_original.Add(internal_id, original_id);
			return internal_id;
		}

		/// <summary>Get original (external) IDs of a list of given entities</summary>
		/// <param name="internal_id_list">the list of internal IDs</param>
		/// <returns>the list of original (external) IDs</returns>
		public IList<int> ToOriginalID(IList<int> internal_id_list)
		{
			var result = new List<int>(internal_id_list.Count);
			foreach (int id in internal_id_list)
				result.Add(ToOriginalID(id));
			return result;
		}

		/// <summary>Get internal IDs of a list of given entities</summary>
		/// <param name="original_id_list">the list of original (external) IDs</param>
		/// <returns>a list of internal IDs</returns>
		public IList<int> ToInternalID(IList<int> original_id_list)
		{
			var result = new List<int>(original_id_list.Count);
			foreach (int id in original_id_list)
				result.Add(ToInternalID(id));
			return result;
		}
	}
}

