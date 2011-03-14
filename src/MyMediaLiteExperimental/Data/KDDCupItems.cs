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
	public class KDDCupItems
	{
		IList<IList<int>> genres;
		IList<int> artists;
		IList<int> albums;
		IList<KDDCupItemType> types;

		public KDDCupItems(int size)
		{
			genres  = new IList<int>[size];
			artists = new int[size];
			albums  = new int[size];
			types   = new KDDCupItemType[size];
		}

		public void Insert(int item_id, KDDCupItemType type, int album, int artist, IList<int> genres)
		{
			this.types[item_id]   = type;
			this.albums[item_id]  = album;
			this.artists[item_id] = artist;
			this.genres[item_id]  = genres;
		}

		public KDDCupItemType GetType(int item_id)
		{
			return types[item_id];
		}

		public IList<int> GetGenres(int item_id)
		{
			return genres[item_id];
		}

		public int GetArtist(int item_id)
		{
			return artists[item_id];
		}

		public int GetAlbum(int item_id)
		{
			return albums[item_id];
		}

		public bool HasAlbum(int item_id)
		{
			return albums[item_id] != -1;
		}

		public bool HasArtist(int item_id)
		{
			return artists[item_id] != -1;
		}

		public bool HasGenres(int item_id)
		{
			if (genres[item_id] == null)
				return false;
			return genres[item_id].Count > 0;
		}
	}
}

