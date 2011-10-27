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
//

using System;
using System.Collections.Generic;
using System.IO;
using MyMediaLite.Util;

namespace MyMediaLite.IO
{
	/// <summary>Routines to read lists of numbers from text files</summary>
	public static class NumberFile
	{
		/// <summary>Read a list of longs from a StreamReader</summary>
		/// <param name="reader">the <see cref="StreamReader"/> to be read from</param>
		/// <returns>a list of longs</returns>
		public static IList<long> ReadLongs(TextReader reader)
		{
			var numbers = new List<long>();

			string line = string.Empty;
			try
			{
				while ((line = reader.ReadLine()) != null)
					numbers.Add(long.Parse(line));
			}
			catch (Exception)
			{
				throw new FormatException(string.Format("Could not read line '{0}'", line));
			}

			return numbers;
		}

		/// <summary>Read a list of longs from a file</summary>
		/// <param name="filename">the name of the file to be read from</param>
		/// <returns>a list of longs</returns>
		public static IList<long> ReadLongs(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			return Wrap.FormatException<IList<long>>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
					return ReadLongs(reader);
			});
		}

		/// <summary>Read a list of integers from a StreamReader</summary>
		/// <param name="reader">the <see cref="StreamReader"/> to be read from</param>
		/// <returns>a list of integers</returns>
		public static IList<int> ReadIntegers(TextReader reader)
		{
			var numbers = new List<int>();

			string line = string.Empty;
			try
			{
				while ((line = reader.ReadLine()) != null)
					numbers.Add(int.Parse(line));
			}
			catch (Exception)
			{
				throw new FormatException(string.Format("Could not read line '{0}'", line));
			}

			return numbers;
		}

		/// <summary>Read a list of integers from a file</summary>
		/// <param name="filename">the name of the file to be read from</param>
		/// <returns>a list of integers</returns>
		public static IList<int> ReadIntegers(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			return Wrap.FormatException<IList<int>>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
					return ReadIntegers(reader);
			});
		}
	}
}

