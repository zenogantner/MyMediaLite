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
using MyMediaLite.Util;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers methods for reading in rating data with time information</summary>
	public static class TimedRatingData
	{
		/// <summary>Read in rating data from a file</summary>
		/// <param name="filename">the name of the file to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public ITimedRatings Read(string filename, IEntityMapping user_mapping, IEntityMapping item_mapping)
		{
			return Wrap.FormatException<ITimedRatings>(filename, delegate() {
				using (var reader = new StreamReader(filename))
					return Read(reader, user_mapping, item_mapping);
			});
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public ITimedRatings Read(TextReader reader, IEntityMapping user_mapping, IEntityMapping item_mapping)
		{
			var ratings = new MyMediaLite.Data.TimedRatings();

			string line;
			while ((line = reader.ReadLine()) != null) {
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(Constants.SPLIT_CHARS);

				if (tokens.Length < 4)
					throw new FormatException("Expected at least 4 columns: " + line);

				int user_id = user_mapping.ToInternalID(long.Parse (tokens [0]));
				int item_id = item_mapping.ToInternalID(long.Parse (tokens [1]));
				double rating = double.Parse(tokens [2], CultureInfo.InvariantCulture);
				string date_string = tokens [3];
				if (tokens [3].StartsWith("\"") && tokens.Length > 4 && tokens [4].EndsWith("\"")) {
					date_string = tokens [3] + " " + tokens [4];
					date_string = date_string.Substring(1, date_string.Length - 2);
				}

				DateTime time = DateTime.Parse(date_string, CultureInfo.InvariantCulture);
				ratings.Add(user_id, item_id, rating, time);

				if (ratings.Count % 200000 == 199999)
					Console.Error.Write(".");
				if (ratings.Count % 12000000 == 11999999)
					Console.Error.WriteLine();
			}
			return ratings;
		}
	}
}