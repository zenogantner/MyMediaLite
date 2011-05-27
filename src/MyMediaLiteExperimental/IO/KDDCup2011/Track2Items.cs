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
using System.Collections.Generic;
using System.IO;

namespace MyMediaLite.IO.KDDCup2011
{
	/// <summary>Class that offers static methods for reading in test data from the KDD Cup 2011 files</summary>
	public class Track2Items
	{
		/// <summary>Read track 2 candidates from a file</summary>
		/// <param name="filename">the name of the file to read from, "-" if STDIN</param>
		/// <returns>the candidates</returns>
		static public Dictionary<int, IList<int>> Read(string filename)
		{
			if (filename.Equals("-"))
				return Read(Console.In);
			else
				using ( var reader = new StreamReader(filename) )
					return Read(reader);
		}

		/// <summary>Read track 2 candidates from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <returns>the candidates</returns>
		static public Dictionary<int, IList<int>>
			Read(TextReader reader)
		{
			var candidates = new Dictionary<int, IList<int>>();

			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				string[] tokens = line.Split('|');

				int user_id     = int.Parse(tokens[0]);
				int num_ratings = int.Parse(tokens[1]); // number of ratings for this user

				var user_candidates = new int[num_ratings];
				for (int i = 0; i < num_ratings; i++)
				{
					line = reader.ReadLine();

					user_candidates[i] = int.Parse(line);
				}
				
				candidates.Add(user_id, user_candidates);
			}
			return candidates;
		}
	}
}