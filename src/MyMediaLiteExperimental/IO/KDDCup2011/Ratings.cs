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

using System;
using System.IO;
using MyMediaLite.Data;

namespace MyMediaLite.IO.KDDCup2011
{
	/// <summary>Class that offers static methods for reading in rating data from the KDD Cup 2011 files</summary>
	public static class Ratings
	{
		/// <summary>Read in rating data from a file</summary>
		/// <param name="filename">the name of the file to read from, "-" if STDIN</param>
		/// <param name="num_ratings">the number of ratings</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(string filename, int num_ratings)
		{
			if (filename.Equals("-"))
				return Read(Console.In, num_ratings);
			else
				using ( var reader = new StreamReader(filename) )
					return Read(reader, num_ratings);
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="num_ratings">the number of ratings</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(TextReader reader, int num_ratings)
		{
			IRatings ratings = new StaticRatings(num_ratings);

			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				string[] tokens = line.Split('|');

				int user_id          = int.Parse(tokens[0]);
				int num_user_ratings = int.Parse(tokens[1]); // number of ratings for this user

				for (int i = 0; i < num_user_ratings; i++)
				{
					line = reader.ReadLine();

					tokens = line.Split('\t');

					int item_id = int.Parse(tokens[0]);
					byte rating = byte.Parse(tokens[1]);

					ratings.Add(user_id, item_id, rating);
				}
			}
			return ratings;
		}

		/// <summary>Read in test rating data (Track 1) from a file</summary>
		/// <param name="filename">the name of the file to read from, "-" if STDIN</param>
		/// <param name="num_ratings">the number of ratings</param>
		/// <returns>the rating data</returns>
		static public IRatings ReadTest(string filename, int num_ratings)
		{
			using ( var reader = new StreamReader(filename) )
				return ReadTest(reader, num_ratings);
		}

		/// <summary>Read in rating test data (Track 1) from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="num_ratings">the number of ratings</param>
		/// <returns>the rating data</returns>
		static public IRatings ReadTest(TextReader reader, int num_ratings)
		{
			IRatings ratings = new StaticRatings(num_ratings);

			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				string[] tokens = line.Split('|');

				int user_id     = int.Parse(tokens[0]);
				int num_user_ratings = int.Parse(tokens[1]); // number of ratings for this user

				for (int i = 0; i < num_user_ratings; i++)
				{
					line = reader.ReadLine();

					tokens = line.Split('\t');

					int item_id = int.Parse(tokens[0]);

					ratings.Add(user_id, item_id, 0);
				}
			}
			return ratings;
		}
	}
}