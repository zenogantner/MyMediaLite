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

using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>Interface to map external entity IDs to internal ones to ensure that there are no gaps in the numbering</summary>
	public interface IEntityMapping
	{
		/// <summary>all original (external) entity IDs</summary>
		/// <value>all original (external) entity IDs</value>
		ICollection<int> OriginalIDs { get; }

		/// <summary>all internal entity IDs</summary>
		/// <value>all internal entity IDs</value>
		ICollection<int> InternalIDs { get; }

		/// <summary>Get original (external) ID of a given entity, if the given internal ID is unknown, throw an exception.</summary>
		/// <param name="internal_id">the internal ID of the entity</param>
		/// <returns>the original (external) ID of the entitiy</returns>
		int ToOriginalID(int internal_id);

		/// <summary>Get internal ID of a given entity. If the given external ID is unknown, create a new internal ID for it and store the mapping.</summary>
		/// <param name="original_id">the original (external) ID of the entity</param>
		/// <returns>the internal ID of the entitiy</returns>
		int ToInternalID(int original_id);

		/// <summary>Get original (external) IDs of a list of given entities</summary>
		/// <param name="internal_id_list">the list of internal IDs</param>
		/// <returns>the list of original (external) IDs</returns>
		IList<int> ToOriginalID(IList<int> internal_id_list);

		/// <summary>Get internal IDs of a list of given entities</summary>
		/// <param name="original_id_list">the list of original (external) IDs</param>
		/// <returns>a list of internal IDs</returns>
		IList<int> ToInternalID(IList<int> original_id_list);
	}
}

