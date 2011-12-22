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

using System.Collections.Generic;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Data
{
	/// <summary>Represents KDD Cup 2011 items like album, track, artist, or genre</summary>
	public sealed class KDDCupItems
	{
		IList<IList<int>> genres;
		IList<int> artists;
		IList<int> albums;
		IList<KDDCupItemType> types;

		static int[] empty_list = new int[0];

		/// <summary>Create item information object</summary>
		/// <param name="size">the number of items</param>
		public KDDCupItems(int size)
		{
			genres  = new IList<int>[size];
			artists = new int[size];
			albums  = new int[size];
			types   = new KDDCupItemType[size];

			for (int i = 0; i < size; i++)
			{
				artists[i] = -1;
				albums[i]  = -1;
				types[i]   = KDDCupItemType.None;
			}
		}

		/// <summary>Insert information about an entry to the data structure</summary>
		/// <param name="item_id">the item ID</param>
		/// <param name="type">the <see cref="KDDCupItemType"/> of the item</param>
		/// <param name="album">the album ID if the item is a track or album, -1 otherwise</param>
		/// <param name="artist">the artist ID if the item is a track, an album, or an artist, -1 otherwise</param>
		/// <param name="genres">a (possibly empty or null) list of genre IDs</param>
		public void Insert(int item_id, KDDCupItemType type, int album, int artist, IList<int> genres)
		{
			this.types[item_id]   = type;
			this.albums[item_id]  = album;
			this.artists[item_id] = artist;
			this.genres[item_id]  = genres;
		}

		/// <summary>Get the type of a given item</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>the <see cref="KDDCupItemType"/> of the given item</returns>
		public KDDCupItemType GetType(int item_id)
		{
			return types[item_id];
		}

		/// <summary>Get a list of genres for a given item</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>a list of genres</returns>
		public IList<int> GetGenres(int item_id)
		{
			return genres[item_id] != null ? genres[item_id] : empty_list;
		}

		/// <summary>Get the artist for a given item</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>the artist ID</returns>
		public int GetArtist(int item_id)
		{
			return artists[item_id];
		}

		/// <summary>Get the album for a given item</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>the album ID</returns>
		public int GetAlbum(int item_id)
		{
			return albums[item_id];
		}

		/// <summary>Check whether the given item is associated with an album</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>true if it is associated with an album, false otherwise</returns>
		public bool HasAlbum(int item_id)
		{
			return albums[item_id] != -1;
		}

		/// <summary>Check whether the given item is associated with an artist</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>true if it is associated with an artist, false otherwise</returns>
		public bool HasArtist(int item_id)
		{
			return artists[item_id] != -1;
		}

		/// <summary>Check whether the given item is associated with one or more genres</summary>
		/// <param name="item_id">the item ID</param>
		/// <returns>true if it is associated with at least one genre, false otherwise</returns>
		public bool HasGenres(int item_id)
		{
			if (genres[item_id] == null)
				return false;
			return genres[item_id].Count > 0;
		}

		/// <summary>Gives a textual summary of the item data</summary>
		public override string ToString()
		{
			int num_tracks = 0, num_albums = 0, num_artists = 0, num_genres = 0;
			foreach (var type in types)
				switch (type)
				{
					case KDDCupItemType.Track:	num_tracks++;  break;
					case KDDCupItemType.Album: 	num_albums++;  break;
					case KDDCupItemType.Artist: num_artists++; break;
					case KDDCupItemType.Genre:  num_genres++;  break;
				}

			return string.Format("{0} tracks, {1} albums, {2} artists, {3} genres", num_tracks, num_albums, num_artists, num_genres);
		}
	}
}

