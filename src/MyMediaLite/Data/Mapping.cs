// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
//  astring with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MyMediaLite.Data
{
	/// <summary>Class to map external entity IDs to internal ones to ensure that there are no gaps in the numbering</summary>
	[Serializable()]
	public sealed class Mapping : IMapping, ISerializable
	{
		/// <summary>Contains the mapping from the original (external) IDs to the internal IDs</summary>
		/// <remarks>
		/// Never, to repeat NEVER, directly delete entries from this dictionary!
		/// </remarks>
		internal Dictionary<string, int> original_to_internal = new Dictionary<string, int>();

		/// <summary>Contains the mapping from the internal IDs to the original (external) IDs</summary>
		/// <remarks>
		/// Never, to repeat NEVER, directly delete entries from this list!
		/// </remarks>
		internal List<string> internal_to_original = new List<string>();

		/// <summary>all original (external) entity IDs</summary>
		/// <value>all original (external) entity IDs</value>
		public ICollection<string> OriginalIDs { get { return original_to_internal.Keys; } }

		///
		public ICollection<int> InternalIDs { get { return Enumerable.Range(0, internal_to_original.Count).ToArray(); } }

		///
		public int NumberOfEntities { get { return internal_to_original.Count; } }

		/// <summary>default constructor</summary>
		public Mapping() { }

		///
		public Mapping(SerializationInfo info, StreamingContext context)
		{
			original_to_internal = (Dictionary<string, int>) info.GetValue("original_to_internal", typeof(Dictionary<string, int>));
			internal_to_original = (List<string>) info.GetValue("internal_to_original", typeof(List<string>));
		}

		/// <summary>Get original (external) ID of a given entity, if the given internal ID is unknown, throw an exception.</summary>
		/// <param name="internal_id">the internal ID of the entity</param>
		/// <returns>the original (external) ID of the entitiy</returns>
		public string ToOriginalID(int internal_id)
		{
			if (internal_id < internal_to_original.Count)
				return internal_to_original[internal_id];
			else
				throw new ArgumentException("Unknown internal ID: " + internal_id);
		}

		/// <summary>Get internal ID of a given entity. If the given external ID is unknown, create a new internal ID for it and store the mapping.</summary>
		/// <param name="original_id">the original (external) ID of the entity</param>
		/// <returns>the internal ID of the entitiy</returns>
		public int ToInternalID(string original_id)
		{
			int internal_id;
			if (original_to_internal.TryGetValue(original_id, out internal_id))
				return internal_id;

			internal_id = original_to_internal.Count;
			original_to_internal.Add(original_id, internal_id);
			internal_to_original.Add(original_id);
			return internal_id;
		}

		/// <summary>Get original (external) IDs of a list of given entities</summary>
		/// <param name="internal_id_list">the list of internal IDs</param>
		/// <returns>the list of original (external) IDs</returns>
		public IList<string> ToOriginalID(IList<int> internal_id_list)
		{
			var result = new List<string>(internal_id_list.Count);
			foreach (int id in internal_id_list)
				result.Add(ToOriginalID(id));
			return result;
		}

		/// <summary>Get internal IDs of a list of given entities</summary>
		/// <param name="original_id_list">the list of original (external) IDs</param>
		/// <returns>a list of internal IDs</returns>
		public IList<int> ToInternalID(IList<string> original_id_list)
		{
			var result = new List<int>(original_id_list.Count);
			foreach (string id in original_id_list)
				result.Add(ToInternalID(id));
			return result;
		}

		///
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("original_to_internal", this.original_to_internal);
			info.AddValue("internal_to_original", this.internal_to_original);
		}
	}
}

