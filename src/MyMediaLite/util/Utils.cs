// Copyright (C) 2010 Zeno Gantner
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
using System.IO;


namespace MyMediaLite.util
{
	/// <summary>
	/// Class containing utility functions
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Install a handler for unhandled exceptions
		/// </summary>
		/// <param name="sender">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="unhandled_event">
		/// A <see cref="UnhandledExceptionEventArgs"/>
		/// </param>
		public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs unhandled_event)
		{
			try
			{
				Exception e = (Exception)unhandled_event.ExceptionObject;
				Console.Error.WriteLine(e.Message + e.StackTrace);
			}
			finally
			{
				Console.Error.WriteLine ("Terminate on unhandled exception.");
				Environment.Exit(-1);
			}
		}

		// TODO add memory constraints and a replacement strategy
		/// <summary>
		/// Memoize a function
		/// </summary>
		/// <param name="f">The function to memoize
		/// </param>
		/// <returns>a version of the function that remembers past function results</returns>
		public static Func<A, R> Memoize<A, R>(this Func<A, R> f)
		{
  			var map = new Dictionary<A, R>();
  			return a =>
    		{
      			R value;
      			if (map.TryGetValue(a, out value))
        		return value;
      			value = f(a);
      			map.Add(a, value);
      			return value;
    		};
		}

		/// <summary>
		/// Delegate definition necessary to define MeasureTime
		/// </summary>
		public delegate void task();
		
		/// <summary>
		/// Measure how long an action takes
		/// </summary>
		/// <param name="t">
		/// A <see cref="task"/> defining the action to be measured
		/// </param>
		/// <returns>
		/// The <see cref="TimeSpan"/> it takes to perform the action
		/// </returns>
		public static TimeSpan MeasureTime(task t)
		{
			DateTime startTime = DateTime.Now;
			t();
			return DateTime.Now - startTime;
		}

		// TODO only works for strings, not for regexes, do proper implementations
		/// <summary>
		/// Split a string
		/// </summary>
		/// <param name="str">
		/// the string to be split
		/// </param>
		/// <param name="regex">
		/// the separator (warning: currently not a regex)
		/// </param>
		/// <param name="max_fields">
		/// the maximum number of fields
		/// </param>
		/// <returns>
		/// the components the string was split into
		/// </returns>
		public static string[] Split(string str, string regex, int max_fields)
		{
			string[] fields = System.Text.RegularExpressions.Regex.Split(str, regex);

			if (fields.Length > max_fields)
			{
				int rest_length = fields.Length - max_fields + 1;

				string[] return_fields = new string[max_fields];
				Array.Copy(fields, return_fields, max_fields - 1);
				return_fields[max_fields - 1] = String.Join(regex, fields, max_fields - 1, rest_length);
				return return_fields;
		    }
			return fields;
		}

		/// <summary>
		/// Read a list of integers from a StreamReader
		/// </summary>
		/// <param name="reader">
		/// the <see cref="StreamReader"/> to be read from
		/// </param>
		/// <returns>
		/// a list of integers
		/// </returns>
		public static IList<int> ReadIntegers(StreamReader reader)
		{
			List<int> numbers = new List<int>();

			while (!reader.EndOfStream)
			{
				string line = reader.ReadLine();
				numbers.Add(int.Parse(line));
			}

			return numbers;
		}

		/// <summary>
		/// Read a list of integers from a file
		/// </summary>
		/// <param name="filename">
		/// the name of the file to be read from
		/// </param>
		/// <returns>
		/// a list of integers
		/// </returns>
		public static IList<int> ReadIntegers(string filename)
		{
			using ( StreamReader reader = new StreamReader(filename) )
			{
				return ReadIntegers(reader);
			}
		}
		
		/// <summary>
		/// Shuffle a list in-place
		/// </summary>
		/// <remarks>
		/// Fisher-Yates shuffle, see
		/// http://en.wikipedia.org/wiki/Fisher–Yates_shuffle
		/// </remarks>
		public static void Shuffle<T>(IList<T> list)
		{
			Random random = MyMediaLite.util.Random.GetInstance();			
			for (int i = list.Count - 1; i >= 0; i--)
			{
				int r = random.Next(0, i + 1);

				T tmp = list[i];
				list[i] = list[r];
				list[r] = tmp;
			}			
		}
		
	}
}
