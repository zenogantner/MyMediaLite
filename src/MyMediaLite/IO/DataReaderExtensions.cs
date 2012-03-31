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
using System.Data;

namespace MyMediaLite.IO
{
	/// <summary>Extension methods for IDataReader objects</summary>
	public static class DataReaderExtensions
	{
		/// <summary>Get a getter function for (usually user or item) IDs from an IDataReader</summary>
		/// <returns>a function returning a string</returns>
		/// <param name='reader'>the reader object</param>
		/// <param name='i'>index of the field to access</param>
		/// <exception cref='FormatException'>
		///   thrown if field i is not of type String, Int32, or Int64
		/// </exception>
		public static Func<string> GetStringGetter(this IDataReader reader, int i)
		{
			Type t = reader.GetFieldType(i);
			if (t.Equals(String.Empty.GetType()))
				return () => reader.GetString(i);
			else if (t.Equals(Int32.MinValue.GetType()))
				return () => reader.GetInt32(i).ToString();
			else if (t.Equals(Int64.MinValue.GetType()))
				return () => reader.GetInt64(i).ToString();
			else
				throw new FormatException(string.Format("Type '{0}' not supported for field {1}.", t.Name, i));
		}

		/// <summary>Get a getter function for ratings from an IDataReader</summary>
		/// <returns>a function returning a float</returns>
		/// <param name='reader'>the reader object</param>
		/// <param name='i'>index of the field to access</param>
		/// <exception cref='FormatException'>
		///   thrown if field i is not of type Float, Double, Int32, or Int64
		/// </exception>
		public static Func<float> GetFloatGetter(this IDataReader reader, int i)
		{
			Type t = reader.GetFieldType(i);
			if (t.Equals(float.MinValue.GetType()))
				return () => reader.GetFloat(i);
			else if (t.Equals(double.MinValue.GetType()))
				return () => (float) reader.GetDouble(i);
			else if (t.Equals(Int32.MinValue.GetType()))
				return () => (float) reader.GetInt32(i);
			else if (t.Equals(Int64.MinValue.GetType()))
				return () => (float) reader.GetInt64(i);
			else
				throw new FormatException(string.Format("Type '{0}' not supported for field {1}.", t.Name, i));
		}

	}
}