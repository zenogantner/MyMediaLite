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
using System.Runtime.Serialization;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.IO
{
	/// <summary>Class that contains static methods for reading in implicit feedback data for ItemRecommender, derived from rating data</summary>
	public static class ItemDataRatingThreshold
	{
		/// <summary>Read in rating data which will be interpreted as implicit feedback data from a file</summary>
		/// <param name="filename">name of the file to be read from</param>
		/// <param name="rating_threshold">the minimum rating value needed to be accepted as positive feedback</param>
		/// <param name="user_mapping">user <see cref="IMapping"/> object</param>
		/// <param name="item_mapping">item <see cref="IMapping"/> object</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>a <see cref="IPosOnlyFeedback"/> object with the user-wise collaborative data</returns>
		static public IPosOnlyFeedback Read(string filename, float rating_threshold, IMapping user_mapping = null, IMapping item_mapping = null, bool ignore_first_line = false)
		{
			string binary_filename = string.Format(CultureInfo.InvariantCulture, "{0}.bin.PosOnlyFeedbackThreshold-{1}", filename, rating_threshold);
			if (FileSerializer.Should(user_mapping, item_mapping) && File.Exists(binary_filename))
				return (IPosOnlyFeedback) FileSerializer.Deserialize(binary_filename);

			return Wrap.FormatException<IPosOnlyFeedback>(filename, delegate() {
				using ( var reader = new StreamReader(filename) )
				{
					var feedback_data = (ISerializable) Read(reader, rating_threshold, user_mapping, item_mapping);
					if (FileSerializer.Should(user_mapping, item_mapping) && FileSerializer.CanWrite(binary_filename))
						feedback_data.Serialize(binary_filename);
					return (IPosOnlyFeedback) feedback_data;
				}
			});
		}

		/// <summary>Read in rating data which will be interpreted as implicit feedback data from a TextReader</summary>
		/// <param name="reader">the TextReader to be read from</param>
		/// <param name="rating_threshold">the minimum rating value needed to be accepted as positive feedback</param>
		/// <param name="user_mapping">user <see cref="IMapping"/> object</param>
		/// <param name="item_mapping">item <see cref="IMapping"/> object</param>
		/// <param name="ignore_first_line">if true, ignore the first line</param>
		/// <returns>a <see cref="IPosOnlyFeedback"/> object with the user-wise collaborative data</returns>
		static public IPosOnlyFeedback Read(TextReader reader, float rating_threshold, IMapping user_mapping = null, IMapping item_mapping = null, bool ignore_first_line = false)
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

				int user_id   = user_mapping.ToInternalID(tokens[0]);
				int item_id   = item_mapping.ToInternalID(tokens[1]);
				float rating  = float.Parse(tokens[2], CultureInfo.InvariantCulture);

				if (rating >= rating_threshold)
					feedback.Add(user_id, item_id);
			}

			return feedback;
		}

		/// <summary>Read in rating data which will be interpreted as implicit feedback data from an IDataReader, e.g. a database via DbDataReader</summary>
		/// <param name="reader">the IDataReader to be read from</param>
		/// <param name="rating_threshold">the minimum rating value needed to be accepted as positive feedback</param>
		/// <param name="user_mapping">user <see cref="IMapping"/> object</param>
		/// <param name="item_mapping">item <see cref="IMapping"/> object</param>
		/// <returns>a <see cref="IPosOnlyFeedback"/> object with the user-wise collaborative data</returns>
		static public IPosOnlyFeedback Read(IDataReader reader, float rating_threshold, IMapping user_mapping, IMapping item_mapping)
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();

			if (reader.FieldCount < 3)
				throw new FormatException("Expected at least 3 columns.");
			Func<string> get_user_id = reader.GetStringGetter(0);
			Func<string> get_item_id = reader.GetStringGetter(1);
			Func<float>  get_rating  = reader.GetFloatGetter(2);

			while (reader.Read())
			{
				int user_id  = user_mapping.ToInternalID(get_user_id());
				int item_id  = item_mapping.ToInternalID(get_item_id());
				float rating = get_rating();

				if (rating >= rating_threshold)
					feedback.Add(user_id, item_id);
			}

			return feedback;
		}
	}
}