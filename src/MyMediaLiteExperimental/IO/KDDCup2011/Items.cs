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
using MyMediaLite.Taxonomy;

namespace MyMediaLite.IO.KDDCup2011
{
	public class Items
	{
		/// <summary>Read in the item data from several files</summary>
		/// <returns>the rating data</returns>
		static public KDDCupItems Read(string tracks_filename, string albums_filename, string artists_filename, string genres_filename, bool track1)
		{
			KDDCupItems items = new KDDCupItems(track1 ? 624961 : 296111);

			using ( var reader = new StreamReader(tracks_filename) )
				ReadTracks(reader, items);

			using ( var reader = new StreamReader(albums_filename) )
				ReadAlbums(reader, items);

			using ( var reader = new StreamReader(artists_filename) )
				ReadArtists(reader, items);

			using ( var reader = new StreamReader(genres_filename) )
				ReadGenres(reader, items);
			
			return items;
		}

		static public void ReadTracks(TextReader reader, KDDCupItems items)
		{
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				string[] tokens = line.Split('|');

				int track_id  = int.Parse(tokens[0]);
				int album_id  = tokens[1] == "None" ? -1 : int.Parse(tokens[1]);
				int artist_id = tokens[2] == "None" ? -1 : int.Parse(tokens[2]);

				var genres = new int[tokens.Length - 3];
				for (int i = 0; i < genres.Length; i++)
					genres[i] = int.Parse(tokens[3 + i]);

				items.Insert(track_id, KDDCupItemType.Track, album_id, artist_id, genres);
			}
		}

		static public void ReadAlbums(TextReader reader, KDDCupItems items)
		{
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				string[] tokens = line.Split('|');

				int album_id  = int.Parse(tokens[0]);
				int artist_id = tokens[1] == "None" ? -1 : int.Parse(tokens[1]);

				var genres = new int[tokens.Length - 2];
				for (int i = 0; i < genres.Length; i++)
					genres[i] = int.Parse(tokens[2 + i]);

				items.Insert(album_id, KDDCupItemType.Album, -1, artist_id, genres);
			}
		}

		static public void ReadArtists(TextReader reader, KDDCupItems items)
		{
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				int artist_id = int.Parse(line);

				items.Insert(artist_id, KDDCupItemType.Artist, -1, -1, null);
			}
		}

		static public void ReadGenres(TextReader reader, KDDCupItems items)
		{
			string line;

			while ( (line = reader.ReadLine()) != null )
			{
				int genre_id = int.Parse(line);

				items.Insert(genre_id, KDDCupItemType.Genre, -1, -1, null);
			}
		}

	}
}

