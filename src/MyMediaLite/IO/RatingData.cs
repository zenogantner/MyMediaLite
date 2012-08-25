// Copyright (C) 2010, 2011, 2012 Zeno Gantner
// Copyright (C) 2011 Artus Krohn-Grimberghe
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
using System.Data;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;

namespace MyMediaLite.IO
{
	/// <summary>Class that offers methods for reading in rating data</summary>
	public static class RatingData
	{
		/// <summary>Read in rating data from a file</summary>
		/// <param name="filename">the name of the file to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>the rating data</returns>
		static public IRatings Read(string filename, IMapping user_mapping = null, IMapping item_mapping = null, bool ignore_first_line = false)
		{
			if (!(user_mapping is Mapping) && !(item_mapping is Mapping) && File.Exists(filename + ".bin.Ratings"))
				return (IRatings) FileSerializer.Deserialize(filename + ".bin.Ratings");

			return Wrap.FormatException<IRatings>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
				{
					var ratings = (Ratings) Read(reader, user_mapping, item_mapping);
					if (!(user_mapping is Mapping) && !(item_mapping is Mapping))
						ratings.Serialize(filename + ".bin.Ratings");
					return ratings;
				}
			});
		}

		/// <summary>Read in rating data from a TextReader</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>the rating data</returns>
		static public IRatings
			Read(TextReader reader, IMapping user_mapping = null, IMapping item_mapping = null, bool ignore_first_line = false)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			if (ignore_first_line)
				reader.ReadLine();

			var ratings = new Ratings();

			string line;
			while ( (line = reader.ReadLine()) != null )
			{
				if (line.Length == 0)
					continue;

				string[] tokens = line.Split(Constants.SPLIT_CHARS);

				if (tokens.Length < 3)
					throw new FormatException("Expected at least 3 columns: " + line);

				int user_id = user_mapping.ToInternalID(tokens[0]);
				int item_id = item_mapping.ToInternalID(tokens[1]);
				float rating = float.Parse(tokens[2], CultureInfo.InvariantCulture);

				ratings.Add(user_id, item_id, rating);
			}
			ratings.InitScale();
			return ratings;
		}

		/// <summary>Read in rating data from an IDataReader, e.g. a database via DbDataReader</summary>
		/// <param name="reader">the <see cref="IDataReader"/> to read from</param>
		/// <param name="user_mapping">mapping object for user IDs</param>
		/// <param name="item_mapping">mapping object for item IDs</param>
		/// <returns>the rating data</returns>
		static public IRatings
			Read(IDataReader reader, IMapping user_mapping, IMapping item_mapping)
		{
			var ratings = new Ratings();

			if (reader.FieldCount < 3)
				throw new FormatException("Expected at least 3 columns.");

			Func<string> get_user_id = reader.GetStringGetter(0);
			Func<string> get_item_id = reader.GetStringGetter(1);
			Func<float>  get_rating  = reader.GetFloatGetter(2);

			while (reader.Read())
			{
				int user_id = user_mapping.ToInternalID(get_user_id());
				int item_id = item_mapping.ToInternalID(get_item_id());
				float rating = get_rating();

				ratings.Add(user_id, item_id, rating);
			}
			ratings.InitScale();
			return ratings;
		}
	}
}