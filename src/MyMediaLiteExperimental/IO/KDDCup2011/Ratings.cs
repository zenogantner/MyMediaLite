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
		/// <returns>the rating data</returns>
		static public IRatings Read(string filename)
		{
			if (filename.Equals("-"))
				return Read(Console.In);
			else
				using ( var reader = new StreamReader(filename) )
					return Read(reader);
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <returns>the rating data</returns>
		static public IRatings
			Read(TextReader reader)
		{
			IRatings ratings = new MyMediaLite.Data.Ratings();

			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				string[] tokens = line.Split('|');

				int user_id     = int.Parse(tokens[0]);
				int num_ratings = int.Parse(tokens[1]); // number of ratings for this user

				for (int i = 0; i < num_ratings; i++)
				{
					line = reader.ReadLine();

					tokens = line.Split('\t');

					int item_id = int.Parse(tokens[0]);
					double rating  = (double) uint.Parse(tokens[1]);

					ratings.Add(user_id, item_id, rating);
				}
			}
			return ratings;
		}
	}
}