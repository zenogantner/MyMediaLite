// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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
	/// <summary>Class that offers methods for reading in static rating data</summary>
	public static class StaticRatingData
	{
		/// <summary>Read in static rating data from a file</summary>
		/// <param name="filename">the name of the file to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="rating_type">the data type to be used for storing the ratings</param>
		/// <param name="test_rating_format">whether there is a rating column in each line or not</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(
			string filename,
			IMapping user_mapping = null, IMapping item_mapping = null,
			RatingType rating_type = RatingType.FLOAT,
			TestRatingFileFormat test_rating_format = TestRatingFileFormat.WITH_RATINGS,
			bool ignore_first_line = false)
		{
			string binary_filename = filename + ".bin.StaticRatings";
			if (FileSerializer.Should(user_mapping, item_mapping) && File.Exists(binary_filename))
				return (IRatings) FileSerializer.Deserialize(binary_filename);

			int size = 0;
			using ( var reader = new StreamReader(filename) )
				while (reader.ReadLine() != null)
					size++;
			if (ignore_first_line)
				size--;

			return Wrap.FormatException<IRatings>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
				{
					var ratings = (StaticRatings) Read(reader, size, user_mapping, item_mapping, rating_type, test_rating_format);
					if (FileSerializer.Should(user_mapping, item_mapping) && FileSerializer.CanWrite(binary_filename))
						ratings.Serialize(binary_filename);
					return ratings;
				}
			});
		}

		/// <summary>Read in static rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="size">the number of ratings in the file</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="rating_type">the data type to be used for storing the ratings</param>
		/// <param name="test_rating_format">whether there is a rating column in each line or not</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(
			TextReader reader, int size,
			IMapping user_mapping = null, IMapping item_mapping = null,
			RatingType rating_type = RatingType.FLOAT,
			TestRatingFileFormat test_rating_format = TestRatingFileFormat.WITH_RATINGS,
			bool ignore_first_line = false)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			if (ignore_first_line)
				reader.ReadLine();

			IRatings ratings;
			if (rating_type == RatingType.BYTE)
				ratings = new StaticByteRatings(size);
			else if (rating_type == RatingType.FLOAT)
				ratings = new StaticRatings(size);
			else
				throw new FormatException(string.Format("Unknown rating type: {0}", rating_type));

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(Constants.SPLIT_CHARS);

				if (test_rating_format == TestRatingFileFormat.WITH_RATINGS && tokens.Length < 3)
					throw new FormatException("Expected at least 3 columns: " + line);
				if (test_rating_format == TestRatingFileFormat.WITHOUT_RATINGS && tokens.Length < 2)
					throw new FormatException("Expected at least 2 columns: " + line);

				int user_id = user_mapping.ToInternalID(tokens[0]);
				int item_id = item_mapping.ToInternalID(tokens[1]);
				float rating = test_rating_format == TestRatingFileFormat.WITH_RATINGS ? float.Parse(tokens[2], CultureInfo.InvariantCulture) : 0;

				ratings.Add(user_id, item_id, rating);
			}
			ratings.InitScale();
			return ratings;
		}
	}
}
