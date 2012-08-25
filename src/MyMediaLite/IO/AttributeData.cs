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
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Data;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers static methods to read (binary) attribute data into IBooleanMatrix objects</summary>
	/// <remarks>
	/// The expected (sparse) line format is:
	/// ENTITY_ID SEPARATOR ATTRIBUTE_ID
	/// for attributes that are set.
	/// SEPARATOR can be space, tab, or comma.
	/// </remarks>
	public static class AttributeData
	{
		/// <summary>Read binary attribute data from a file</summary>
		/// <remarks>
		/// The expected (sparse) line format is:
		/// ENTITY_ID tab/space/comma ATTRIBUTE_ID
		/// for the relations that hold.
		/// </remarks>
		/// <param name="filename">the name of the file to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the attribute data</returns>
		static public IBooleanMatrix Read(string filename, IMapping mapping)
		{
			return Wrap.FormatException<IBooleanMatrix>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
					return Read(reader, mapping);
			});
		}

		/// <summary>Read binary attribute data from a StreamReader</summary>
		/// <remarks>
		/// The expected (sparse) line format is:
		/// ENTITY_ID tab/space/comma ATTRIBUTE_ID
		/// for the relations that hold.
		/// </remarks>
		/// <param name="reader">a StreamReader to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the attribute data</returns>
		static public IBooleanMatrix Read(StreamReader reader, IMapping mapping)
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
					throw new FormatException("Expected exactly 2 columns: " + line);

				int entity_id = mapping.ToInternalID(tokens[0]);
				int attr_id   = int.Parse(tokens[1]);

				matrix[entity_id, attr_id] = true;
			}

			return matrix;
		}

		/// <summary>Read binary attribute data from an IDataReader, e.g. a database via DbDataReader</summary>
		/// <param name="reader">an IDataReader to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the attribute data</returns>
		static public IBooleanMatrix Read(IDataReader reader, IMapping mapping)
		{
			if (reader.FieldCount < 2)
				throw new Exception("Expected at least 2 columns.");

			var matrix = new SparseBooleanMatrix();

			while (!reader.Read())
			{
				int entity_id = mapping.ToInternalID(reader.GetString(0));
				int attr_id   = reader.GetInt32(1);

				matrix[entity_id, attr_id] = true;
			}

			return matrix;
		}
	}
}
