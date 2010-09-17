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
	public class RatingPredictionData
	{
		// TODO find better name for RatingData - isn't this a 'sparse' matrix with column/row indices?
		static public RatingData Read(string filename, double min_rating, double max_rating)
		{
            using ( StreamReader reader = new StreamReader(filename) )
			{
				return Read(reader, min_rating, max_rating);
			}
		}

		static public RatingData Read(StreamReader reader, double min_rating, double max_rating)
		{
		    RatingData ratings = new RatingData(true, true, true);

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