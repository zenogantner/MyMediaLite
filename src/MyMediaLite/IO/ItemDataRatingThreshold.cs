// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.IO
{
	/// <summary>Class that contains static methods for reading in implicit feedback data for ItemRecommender</summary>
	public static class ItemDataRatingThreshold
	{
		/// <summary>Read in rating data which will be interpreted as implicit feedback data from a file</summary>
		/// <param name="filename">name of the file to be read from</param>
		/// <param name="rating_threshold">the minimum rating value needed to be accepted as positive feedback</param>
		/// <param name="user_mapping">user <see cref="IEntityMapping"/> object</param>
		/// <param name="item_mapping">item <see cref="IEntityMapping"/> object</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>a <see cref="IPosOnlyFeedback"/> object with the user-wise collaborative data</returns>
		static public IPosOnlyFeedback Read(string filename, double rating_threshold, IEntityMapping user_mapping = null, IEntityMapping item_mapping = null, bool ignore_first_line = false)
		{
			return Wrap.FormatException<IPosOnlyFeedback>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
					return Read(reader, rating_threshold, user_mapping, item_mapping);
			});
		}

		/// <summary>Read in rating data which will be interpreted as implicit feedback data from a TextReader</summary>
		/// <param name="reader">the TextReader to be read from</param>
		/// <param name="rating_threshold">the minimum rating value needed to be accepted as positive feedback</param>
		/// <param name="user_mapping">user <see cref="IEntityMapping"/> object</param>
		/// <param name="item_mapping">item <see cref="IEntityMapping"/> object</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>a <see cref="IPosOnlyFeedback"/> object with the user-wise collaborative data</returns>
		static public IPosOnlyFeedback Read(TextReader reader, double rating_threshold, IEntityMapping user_mapping = null, IEntityMapping item_mapping = null, bool ignore_first_line = false)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			if (ignore_first_line)
				reader.ReadLine();

			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();

			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.Trim().Length == 0)
					continue;

				string[] tokens = line.Split(Constants.SPLIT_CHARS);

				if (tokens.Length < 3)
					throw new FormatException("Expected at least 3 columns: " + line);

				int user_id   = user_mapping.ToInternalID(long.Parse(tokens[0]));
				int item_id   = item_mapping.ToInternalID(long.Parse(tokens[1]));
				double rating = double.Parse(tokens[2], CultureInfo.InvariantCulture);

				if (rating >= rating_threshold)
					feedback.Add(user_id, item_id);
			}

			return feedback;
		}

		/// <summary>Read in rating data which will be interpreted as implicit feedback data from an IDataReader, e.g. a database via DbDataReader</summary>
		/// <param name="reader">the IDataReader to be read from</param>
		/// <param name="rating_threshold">the minimum rating value needed to be accepted as positive feedback</param>
		/// <param name="user_mapping">user <see cref="IEntityMapping"/> object</param>
		/// <param name="item_mapping">item <see cref="IEntityMapping"/> object</param>
		/// <returns>a <see cref="IPosOnlyFeedback"/> object with the user-wise collaborative data</returns>
		static public IPosOnlyFeedback Read(IDataReader reader, double rating_threshold, IEntityMapping user_mapping, IEntityMapping item_mapping)
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();

			if (reader.FieldCount < 3)
				throw new Exception("Expected at least 3 columns.");

			while (reader.Read())
			{
				int user_id = user_mapping.ToInternalID(reader.GetInt32(0));
				int item_id = item_mapping.ToInternalID(reader.GetInt32(1));
				double rating = reader.GetDouble(2);

				if (rating >= rating_threshold)
				feedback.Add(user_id, item_id);
			}

			return feedback;
		}
	}
}