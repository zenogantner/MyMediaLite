// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Class that contains static methods for rating prediction</summary>
	public static class Extensions
	{
		/// <summary>Rate a given set of instances and write it to a TextWriter</summary>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">test cases</param>
		/// <param name="writer">the TextWriter to write the predictions to</param>
		/// <param name="user_mapping">an <see cref="Mapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="Mapping"/> object for the item IDs</param>
		/// <param name="line_format">a format string specifying the line format; {0} is the user ID, {1} the item ID, {2} the rating</param>
		/// <param name="header">if specified, write this string at the start of the output</param>
		public static void WritePredictions(
			this IRecommender recommender,
			IRatings ratings,
			TextWriter writer,
			IMapping user_mapping = null,
			IMapping item_mapping = null,
			string line_format = "{0}\t{1}\t{2}",
			string header = null)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();

			if (header != null)
				writer.WriteLine(header);

			if (line_format == "ranking")
			{
				foreach (int user_id in ratings.AllUsers)
					if (ratings.ByUser[user_id].Count > 0)
						recommender.WritePredictions(
							user_id,
							new List<int>(from index in ratings.ByUser[user_id] select ratings.Items[index]),
							new int[] { },
							ratings.ByUser[user_id].Count,
							writer,
							user_mapping, item_mapping);
			}
			else
				for (int index = 0; index < ratings.Count; index++)
					writer.WriteLine(
						line_format,
						user_mapping.ToOriginalID(ratings.Users[index]),
						item_mapping.ToOriginalID(ratings.Items[index]),
						recommender.Predict(ratings.Users[index], ratings.Items[index]).ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>Rate a given set of instances and write it to a file</summary>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">test cases</param>
		/// <param name="filename">the name of the file to write the predictions to</param>
		/// <param name="user_mapping">an <see cref="Mapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="Mapping"/> object for the item IDs</param>
		/// <param name="line_format">a format string specifying the line format; {0} is the user ID, {1} the item ID, {2} the rating</param>
		/// <param name="header">if specified, write this string to the first line</param>
		public static void WritePredictions(
			this IRecommender recommender,
			IRatings ratings,
			string filename,
			IMapping user_mapping = null, IMapping item_mapping = null,
			string line_format = "{0}\t{1}\t{2}",
			string header = null)
		{
			using (var writer = new StreamWriter(filename))
				WritePredictions(recommender, ratings, writer, user_mapping, item_mapping, line_format);
		}
	}
}
