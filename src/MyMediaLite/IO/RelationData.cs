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
using System.Data;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers static methods to read (binary) relation over entities into SparseBooleanMatrix objects</summary>
	public static class RelationData
	{
		/// <summary>Read binary attribute data from file</summary>
		/// <remarks>
		/// The expected (sparse) line format is:
		/// ENTITY_ID whitespace/comma ENTITY_ID
		/// for the relations that hold.
		/// </remarks>
		/// <param name="filename">the name of the file to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the relation data</returns>
		static public SparseBooleanMatrix Read(string filename, IEntityMapping mapping)
		{
			try
			{
				using ( var reader = new StreamReader(filename) )
					return Read(reader, mapping);
			}
			catch (IOException e)
			{
				throw new IOException(string.Format("Could not read file {0}: {1}", filename, e.Message));
			}			
		}

		/// <summary>Read binary relation data from file</summary>
		/// <remarks>
		/// The expected (sparse) line format is:
		/// ENTITY_ID whitespace/comma ENTITY_ID
		/// for the relations that hold.
		/// </remarks>
		/// <param name="reader">a StreamReader to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the relation data</returns>
		static public SparseBooleanMatrix Read(StreamReader reader, IEntityMapping mapping)
		{
			var matrix = new SparseBooleanMatrix();

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				// ignore empty lines
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(Constants.SPLIT_CHARS);

				if (tokens.Length != 2)
					throw new IOException("Expected exactly 2 columns: " + line);

				int entity1_id = mapping.ToInternalID(long.Parse(tokens[0]));
				int entity2_id = mapping.ToInternalID(long.Parse(tokens[1]));

				matrix[entity1_id, entity2_id] = true;
			}

			return matrix;
		}

		/// <summary>Read binary relation data from an IDataReader, e.g. a database via DbDataReader</summary>
		/// <param name="reader">an IDataReader to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the relation data</returns>
		static public SparseBooleanMatrix Read(IDataReader reader, IEntityMapping mapping)
		{
			if (reader.FieldCount < 2)
				throw new IOException("Expected at least 2 columns.");

			var matrix = new SparseBooleanMatrix();

			while (!reader.Read())
			{
				int entity1_id = mapping.ToInternalID(reader.GetInt32(0));
				int entity2_id = mapping.ToInternalID(reader.GetInt32(0));

				matrix[entity1_id, entity2_id] = true;
			}

			return matrix;
		}
	}
}
