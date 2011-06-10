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
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers static methods to read (binary) attribute data into SparseBooleanMatrix objects</summary>
	/// <remarks>
	/// The expected (sparse) line format is:
	/// ENTITY_ID whitespace ATTRIBUTE_ID
	/// for attributes that are set.
	/// </remarks>
	public class AttributeData
	{
		/// <summary>Read binary attribute data from a file</summary>
		/// <param name="filename">the name of the file to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the attribute data</returns>
		static public SparseBooleanMatrix Read(string filename, IEntityMapping mapping)
		{
			using ( var reader = new StreamReader(filename) )
				return Read(reader, mapping);
		}

		/// <summary>Read binary attribute data from a StreamReader</summary>
		/// <param name="reader">a StreamReader to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the attribute data</returns>
		static public SparseBooleanMatrix Read(StreamReader reader, IEntityMapping mapping)
		{
			var matrix = new SparseBooleanMatrix();

			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			var split_chars = new char[]{ '\t', ' ' };
			string line;

			while (!reader.EndOfStream)
			{
			   	line = reader.ReadLine();

				// ignore empty lines
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(split_chars);

				if (tokens.Length != 2)
					throw new IOException("Expected exactly two columns: " + line);

				int entity_id = mapping.ToInternalID(int.Parse(tokens[0]));
				int attr_id   = int.Parse(tokens[1]);

			   	matrix[entity_id, attr_id] = true;
			}

			return matrix;
		}

		/// <summary>Read binary attribute data from an IDataReader, e.g. a database via DbDataReader</summary>
		/// <param name="reader">an IDataReader to be read from</param>
		/// <param name="mapping">the mapping object for the given entity type</param>
		/// <returns>the attribute data</returns>
		static public SparseBooleanMatrix Read(IDataReader reader, IEntityMapping mapping)
		{
			if (reader.FieldCount < 2)
				throw new IOException("Expected at least two columns.");

			var matrix = new SparseBooleanMatrix();

			while (!reader.Read())
			{
				int entity_id = mapping.ToInternalID(reader.GetInt32(0));
				int attr_id   = reader.GetInt32(1);

			   	matrix[entity_id, attr_id] = true;
			}

			return matrix;
		}
	}
}
