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
using System.Globalization;
using System.IO;
using MyMediaLite.Data;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers methods for reading in rating data with time information</summary>
	public static class TimedRatingData
	{
		/// <summary>Read in rating data from a file</summary>
		/// <param name="filename">the name of the file to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>the rating data</returns>
		static public ITimedRatings Read(string filename, IMapping user_mapping = null, IMapping item_mapping = null, bool ignore_first_line = false)
		{
			string binary_filename = filename + ".bin.TimedRatings";
			if (FileSerializer.Should(user_mapping, item_mapping) && File.Exists(binary_filename))
				return (ITimedRatings) FileSerializer.Deserialize(binary_filename);

			return Wrap.FormatException<ITimedRatings>(filename, delegate() {
				using (var reader = new StreamReader(filename))
				{
					var ratings = (TimedRatings) Read(reader, user_mapping, item_mapping);
					if (FileSerializer.Should(user_mapping, item_mapping) && FileSerializer.CanWrite(binary_filename))
						ratings.Serialize(binary_filename);
					return ratings;
				}
			});
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>the rating data</returns>
		static public ITimedRatings Read(TextReader reader, IMapping user_mapping = null, IMapping item_mapping = null, bool ignore_first_line = false)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			if (ignore_first_line)
				reader.ReadLine();

			var ratings = new MyMediaLite.Data.TimedRatings();
			var time_split_chars = new char[] { ' ', '-', ':' };

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(Constants.SPLIT_CHARS);

				if (tokens.Length < 4)
					throw new FormatException("Expected at least 4 columns: " + line);

				int user_id = user_mapping.ToInternalID(tokens[0]);
				int item_id = item_mapping.ToInternalID(tokens[1]);
				float rating = float.Parse(tokens[2], CultureInfo.InvariantCulture);
				string date_string = tokens[3];
				if (tokens[3].StartsWith("\"") && tokens.Length > 4 && tokens[4].EndsWith("\""))
				{
					date_string = tokens[3] + " " + tokens[4];
					date_string = date_string.Substring(1, date_string.Length - 2);
				}

				uint seconds;
				if (date_string.Length == 19) // format "yyyy-mm-dd hh:mm:ss"
				{
					var date_time_tokens = date_string.Split(time_split_chars);
					ratings.Add(
						user_id, item_id, rating,
						new DateTime(
							int.Parse(date_time_tokens[0]),
							int.Parse(date_time_tokens[1]),
							int.Parse(date_time_tokens[2]),
							int.Parse(date_time_tokens[3]),
							int.Parse(date_time_tokens[4]),
							int.Parse(date_time_tokens[5])));
				}
				else if (date_string.Length == 10 && date_string[4] == '-') // format "yyyy-mm-dd"
				{
					var date_time_tokens = date_string.Split(time_split_chars);
					ratings.Add(
						user_id, item_id, rating,
						new DateTime(
							int.Parse(date_time_tokens[0]),
							int.Parse(date_time_tokens[1]),
							int.Parse(date_time_tokens[2])));
				}
				else if (uint.TryParse(date_string, out seconds)) // unsigned integer value, interpreted as seconds since Unix epoch
				{
					var time = new DateTime(seconds * 10000000L).AddYears(1969);
					var offset = TimeZone.CurrentTimeZone.GetUtcOffset(time);
					ratings.Add(user_id, item_id, rating, time - offset);
				}
				else
					ratings.Add(user_id, item_id, rating, DateTime.Parse(date_string, CultureInfo.InvariantCulture));

				if (ratings.Count % 200000 == 199999)
					Console.Error.Write(".");
				if (ratings.Count % 12000000 == 11999999)
					Console.Error.WriteLine();
			}
			ratings.InitScale();
			return ratings;
		}
	}
}