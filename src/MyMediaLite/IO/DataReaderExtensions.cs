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
	public delegate string return_string();
	
	public static class DataReaderExtensions
	{
		public static return_string GetGetter(this IDataReader reader, int i)
		{
			Type t = reader.GetFieldType(i);
			if (t.Equals(string.Empty.GetType()))
				return () => reader.GetString(i);
			else if (t.Equals(int.MinValue.GetType()))
				return () => reader.GetInt32(i).ToString();
			else if (t.Equals(long.MinValue.GetType()))
				return () => reader.GetInt64(i).ToString();
			else
				throw new FormatException(string.Format("Type '{0}' not supported for field {1}.", t.Name, i));
		}

	}
}

