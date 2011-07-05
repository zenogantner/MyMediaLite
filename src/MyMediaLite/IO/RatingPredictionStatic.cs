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
	/// <summary>Class that offers methods for reading in static rating data</summary>
	public class RatingPredictionStatic
	{
		/// <summary>Read in static rating data from a file</summary>
		/// <param name="filename">the name of the file to read from, "-" if STDIN</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="rating_type">the data type to be used for storing the ratings</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(string filename,
		                            IEntityMapping user_mapping, IEntityMapping item_mapping,
		                            RatingType rating_type)
		{
			int size = 0;
			using ( var reader = new StreamReader(filename) )
				while (reader.ReadLine() != null)
					size++;

			using ( var reader = new StreamReader(filename) )
				return Read(reader, size, user_mapping, item_mapping, rating_type);
		}

		/// <summary>Read in static rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="size">the number of ratings in the file</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="rating_type">the data type to be used for storing the ratings</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(TextReader reader, int size,
		                            IEntityMapping user_mapping, IEntityMapping item_mapping,
		                            RatingType rating_type)
		{
			IRatings ratings;
			if (rating_type == RatingType.BYTE)
				ratings = new StaticByteRatings(size);
			else if (rating_type == RatingType.FLOAT)
				ratings = new StaticFloatRatings(size);
			else
				ratings = new StaticRatings(size);

			var split_chars = new char[]{ '\t', ' ', ',' };
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(split_chars);

				if (tokens.Length < 3)
					throw new IOException("Expected at least three columns: " + line);

				int user_id = user_mapping.ToInternalID(int.Parse(tokens[0]));
				int item_id = item_mapping.ToInternalID(int.Parse(tokens[1]));
				double rating = double.Parse(tokens[2], CultureInfo.InvariantCulture);

				ratings.Add(user_id, item_id, rating);
			}
			return ratings;
		}
	}
}