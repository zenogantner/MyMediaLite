// Copyright (C) 2011, 2012 Zeno Gantner
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

namespace MyMediaLite
{
	/// <summary>Static methods to wrap around other code.</summary>
	public static class Wrap
	{
		/// <summary>Measure how long an action takes</summary>
		/// <param name="t">An <see cref="Action"/> defining the action to be measured</param>
		/// <returns>The <see cref="TimeSpan"/> it takes to perform the action</returns>
		public static TimeSpan MeasureTime(Action t)
		{
			DateTime startTime = DateTime.Now;
			t(); // perform task
			return DateTime.Now - startTime;
		}
		
		/// <summary>Catch FormatException and re-throw it including filename</summary>
		/// <param name='filename'>the name of the file processed inside t</param>
		/// <param name='t'>the task to be performed</param>
		/// <exception cref='FormatException'>
		/// Represents errors caused by passing incorrectly formatted arguments or invalid format specifiers to methods.
		/// </exception>
		public static void FormatException(string filename, Action t)
		{
			try
			{
				t(); // perform task
			}
			catch (FormatException e)
			{
				throw new FormatException(string.Format("Could not read file {0}: {1}", filename, e.Message));
			}
		}
		
		/// <summary>Catch FormatException and re-throw it including filename; generic version</summary>
		/// <param name='filename'>the name of the file processed inside t</param>
		/// <param name='t'>the task to be performed</param>
		/// <exception cref='FormatException'>
		/// Represents errors caused by passing incorrectly formatted arguments or invalid format specifiers to methods.
		/// </exception>
		public static T FormatException<T>(string filename, Func<T> t)
		{
			try
			{
				return t(); // perform task
			}
			catch (FormatException e)
			{
				throw new FormatException(string.Format("Could not read file {0}: {1}", filename, e.Message));
			}
		}

	}
}

