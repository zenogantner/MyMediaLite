// Copyright (C) 2010, 2011 Zeno Gantner
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

using System.Globalization;
using System.IO;
using MyMediaLite.Data;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Class that contains static methods for rating prediction</summary>
	public class Prediction
	{
		/// <summary>Rate a given set of instances and write it to a TextWriter</summary>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">test cases</param>
		/// <param name="writer">the TextWriter to write the predictions to</param>
		/// <param name="user_mapping">an <see cref="EntityMapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="EntityMapping"/> object for the item IDs</param>
		/// <param name="line_format">a format string specifying the line format; {0} is the user ID, {1} the item ID, {2} the rating</param>
		public static void WritePredictions(
			IRecommender recommender,
			IRatings ratings,
			TextWriter writer,
			IEntityMapping user_mapping = null, IEntityMapping item_mapping = null,
			string line_format = "{0}\t{1}\t{2}")
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();

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
		/// <param name="user_mapping">an <see cref="EntityMapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="EntityMapping"/> object for the item IDs</param>
		/// <param name="line_format">a format string specifying the line format; {0} is the user ID, {1} the item ID, {2} the rating</param>
		public static void WritePredictions(
			IRecommender recommender,
			IRatings ratings,
			string filename,
			IEntityMapping user_mapping = null, IEntityMapping item_mapping = null,
			string line_format = "{0}\t{1}\t{2}")
		{
			using (var writer = new StreamWriter(filename))
				WritePredictions(recommender, ratings, writer, user_mapping, item_mapping, line_format);
		}
	}
}
