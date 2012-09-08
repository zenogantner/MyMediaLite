// Copyright (C) 2012 Zeno Gantner
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
//
using System;
using System.IO;
using MyMediaLite.Data;

namespace MyMediaLite.IO
{
	/// <summary>I/O routines for classes implementing the IEntityMapping interface</summary>
	public static class EntityMappingExtensions
	{
		/// <summary>Save the mappings to a file</summary>
		/// <param name='mapping'>the mapping object to store</param>
		/// <param name='filename'>the name of the file</param>
		public static void SaveMapping(this IMapping mapping, string filename)
		{
			using ( var writer = new StreamWriter(filename) )
				foreach (int internal_id in mapping.InternalIDs)
					writer.WriteLine("{0}\t{1}", internal_id, mapping.ToOriginalID(internal_id));
		}

		/// <summary>Load entity mappings from a file</summary>
		/// <param name='filename'>the name of the file</param>
		/// <returns>an object of type EntityMapping</returns>
		public static IMapping LoadMapping(this string filename)
		{
			var mapping = new Mapping();

			using ( var reader = new StreamReader(filename) )
			{
				string line;
				while ( (line = reader.ReadLine()) != null )
				{
					if (line.Length == 0)
						continue;

					string[] tokens = line.Split('\t');

					if (tokens.Length != 2)
						throw new FormatException("Expected exactly 2 columns: " + line);

					int internal_id    = int.Parse(tokens[0]);
					string external_id = tokens[1];

					if (internal_id != mapping.NumberOfEntities)
						throw new FormatException(string.Format("Expected ID {0}, not {1}, in line '{2}'", mapping.NumberOfEntities, internal_id, line));

					mapping.internal_to_original.Add(external_id);
					mapping.original_to_internal[external_id] = internal_id;
				}
			}

			return mapping;
		}
	}
}

