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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MyMediaLite.Data;

namespace MovieDemo
{
	public sealed class Movie
	{
		public int ID { get; private set; }
		public string Title { get; private set; }
		public int Year { get; private set; }
		public string IMDBKey { get; private set; }

		public Movie(int id, string title, int year, string imdb_key)
		{
			ID = id;
			Title = title;
			Year = year;
			IMDBKey = imdb_key;
		}
	}

	// TODO move to main library
	public sealed class MovieLensMovieInfo
	{
		public List<Movie> movie_list;

		public Dictionary<string, int> IMDB_KEY_To_ID;

		/// <summary>Read movie data from a file</summary>
		/// <param name="filename">the name of the file to be read from</param>
		/// <param name="encoding">the encoding of the file</param>
		/// <param name="item_mapping">ID mapping for the movies</param>
		public void Read(string filename, Encoding encoding, Mapping item_mapping)
		{
			using ( var reader = new StreamReader(filename, encoding) )
				Read(reader, item_mapping);
		}

		/// <summary>Read movie data from a file</summary>
		/// <param name="filename">the name of the file to be read from</param>
		/// <param name="item_mapping">ID mapping for the movies</param>
		public void Read(string filename, Mapping item_mapping)
		{
			using ( var reader = new StreamReader(filename) )
				Read(reader, item_mapping);
		}

		/// <summary>Read movie data from a StreamReader</summary>
		/// <param name="reader">a StreamReader to be read from</param>
		/// <param name="item_mapping">ID mapping for the movies</param>
		public void Read(StreamReader reader, Mapping item_mapping)
		{
			movie_list = new List<Movie>();
			IMDB_KEY_To_ID = new Dictionary<string, int>();

			var separators = new string[] { "::" };

			string line;

			while (!reader.EndOfStream)
			{
				line = reader.ReadLine();

				string[] tokens = line.Split(separators, StringSplitOptions.None);

				if (tokens.Length != 3)
					throw new FormatException("Expected exactly 3 columns: " + line);

				int movie_id          = item_mapping.ToInternalID(tokens[0]);
				string movie_imdb_key = tokens[1];
				//string[] movie_genres = tokens[2].Split('|');

				int movie_year = 1900;
				string movie_title = movie_imdb_key;

				movie_list.Add(new Movie(movie_id, movie_title, movie_year, movie_imdb_key));
				IMDB_KEY_To_ID[movie_imdb_key] =  movie_id;
			}
		}
	}
}

