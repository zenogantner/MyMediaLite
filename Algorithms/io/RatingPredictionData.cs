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
using System.Globalization;
using System.IO;
using MyMediaLite.data;
using MyMediaLite.util;


namespace MyMediaLite.io
{
	/// <summary>
	/// Class that offers static methods for reading in rating data
	/// </summary>
	public class RatingPredictionData
	{
		/// <summary>
		/// Read in rating data from a StreamReader
		/// </summary>
		/// <param name="filename">the name of the file to read from</param>
		/// <param name="num_users">the number of users (-1 means do not create a per-user data structure)</param>
		/// <param name="num_items">the number of items (-1 means do not create a per-item data structure)</param>
		/// <param name="num_ratings">the expected number of ratings</param>
		/// <param name="min_rating">the lowest possible rating value, warn on out of range ratings</param>
		/// <param name="max_rating">the highest possible rating value, warn on out of range ratings</param>
		/// <returns>the rating data</returns>		
		static public RatingData Read(string filename, double min_rating, double max_rating)
		{
			/*
			if (filename.Equals("--"))
			{
				return Read(Console.In, num_users, num_items, num_ratings, min_rating, max_rating);
			}
			else
			{
			*/
	            using ( StreamReader reader = new StreamReader(filename) )
				{
					return Read(reader, min_rating, max_rating);
				}
			//}
		}
		
		/// <summary>
		/// Read in rating data from a StreamReader
		/// </summary>
		/// <param name="reader">the <see cref="StreamReader"/> to read from</param>
		/// <param name="min_rating">the lowest possible rating value, warn on out of range ratings</param>
		/// <param name="max_rating">the highest possible rating value, warn on out of range ratings</param>
		/// <returns>the rating data</returns>
		static public RatingData Read(StreamReader reader,
		                              double min_rating, double max_rating)
		{
		    RatingData ratings = new RatingData();

			bool out_of_range_warning_issued = false;
			NumberFormatInfo ni = new NumberFormatInfo(); ni.NumberDecimalDigits = '.';
			char[] split_chars = new char[]{ '\t', ' ' };
			string line;

			while (!reader.EndOfStream)
			{
	           	line = reader.ReadLine();
				if (line.Trim().Equals(String.Empty))
					continue;

	            string[] tokens = line.Split(split_chars);

				if (tokens.Length < 2)
					throw new IOException("Expected at least two columns: " + line);

                RatingEvent rating = new RatingEvent();
                rating.user_id = int.Parse(tokens[0]);
                rating.item_id = int.Parse(tokens[1]);
                rating.rating = double.Parse(tokens[2]);

				if (!out_of_range_warning_issued)
				{
					if (rating.rating > max_rating || rating.rating < min_rating)
					{
						Console.Error.WriteLine("WARNING: rating value out of range [{0}, {1}]: {2} for user {3}, item {4}",
						                        min_rating, max_rating, rating.rating, rating.user_id, rating.item_id);
						out_of_range_warning_issued = true;
					}
				}

                ratings.AddRating(rating);
            }
			return ratings;
        }
	}
}