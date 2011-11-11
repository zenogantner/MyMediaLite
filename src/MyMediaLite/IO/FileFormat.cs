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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

/*! \namespace MyMediaLite.IO
 *  \brief This namespace contains I/O routines, e.g. to load interaction data from files and databases, or for loading recommender models from disk.
 */
namespace MyMediaLite.IO
{
	/// <summary>Represents different rating file formats</summary>
	public enum RatingFileFormat
	{
		/// <summary>The default tab/comma separated rating format (e.g. MovieLens 100K, Apache Mahout)</summary>
		DEFAULT,
		/// <summary>Like the default format, but ignore the first line</summary>
		IGNORE_FIRST_LINE,
		/// <summary>The MovieLens 1M/10M format (fields separated by "::")</summary>
		MOVIELENS_1M,
		/// <summary>The KDD Cup 2011 rating format</summary>
		KDDCUP_2011,
	}
	
	/// <summary>Represents different rating file formats</summary>
	public enum ItemDataFileFormat
	{
		/// <summary>The default tab/comma separated rating format (e.g. MovieLens 100K, Apache Mahout)</summary>
		DEFAULT,
		/// <summary>Like the default format, but ignore the first line</summary>
		IGNORE_FIRST_LINE,
		// /// <summary>The MovieLens 1M/10M format (fields separated by "::")</summary>
		// MOVIELENS_1M,
		// /// <summary>The KDD Cup 2011 rating format</summary>
		// KDDCUP_2011,
	}

}
