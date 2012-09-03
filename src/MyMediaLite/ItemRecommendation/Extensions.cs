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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Class that contains static methods for item prediction</summary>
	public static class Extensions
	{
		/// <summary>Write item predictions (scores) to a file</summary>
		/// <param name="recommender">the <see cref="IRecommender"/> to use for making the predictions</param>
		/// <param name="train">a user-wise <see cref="IPosOnlyFeedback"/> containing the items already observed</param>
		/// <param name="candidate_items">list of candidate items</param>
		/// <param name="num_predictions">number of items to return per user, -1 if there should be no limit</param>
		/// <param name="filename">the name of the file to write to</param>
		/// <param name="users">a list of users to make recommendations for</param>
		/// <param name="user_mapping">an <see cref="IMapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="IMapping"/> object for the item IDs</param>
		/// <param name="repeated_items">true if items that a user has already accessed shall also be predicted</param>
		static public void WritePredictions(
			this IRecommender recommender,
			IPosOnlyFeedback train,
			IList<int> candidate_items,
			int num_predictions,
			string filename,
			IList<int> users = null,
			IMapping user_mapping = null, IMapping item_mapping = null,
			bool repeated_items = false)
		{
			using (var writer = new StreamWriter(filename))
				WritePredictions(recommender, train, candidate_items, num_predictions, writer, users, user_mapping, item_mapping, repeated_items);
		}

		/// <summary>Write item predictions (scores) to a TextWriter object</summary>
		/// <param name="recommender">the <see cref="IRecommender"/> to use for making the predictions</param>
		/// <param name="train">a user-wise <see cref="IPosOnlyFeedback"/> containing the items already observed</param>
		/// <param name="candidate_items">list of candidate items</param>
		/// <param name="num_predictions">number of items to return per user, -1 if there should be no limit</param>
		/// <param name="writer">the <see cref="TextWriter"/> to write to</param>
		/// <param name="users">a list of users to make recommendations for; if null, all users in train will be provided with recommendations</param>
		/// <param name="user_mapping">an <see cref="IMapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="IMapping"/> object for the item IDs</param>
		/// <param name="repeated_items">true if items that a user has already accessed shall also be predicted</param>
		static public void WritePredictions(
			this IRecommender recommender,
			IPosOnlyFeedback train,
			ICollection<int> candidate_items,
			int num_predictions,
			TextWriter writer,
			IList<int> users = null,
			IMapping user_mapping = null, IMapping item_mapping = null,
			bool repeated_items = false)
		{
			if (users == null)
				users = new List<int>(train.AllUsers);

			ICollection<int> ignore_items = new int[0];
			foreach (int user_id in users)
			{
				if (!repeated_items)
					ignore_items = train.UserMatrix[user_id];
				WritePredictions(recommender, user_id, candidate_items, ignore_items, num_predictions, writer, user_mapping, item_mapping);
			}
		}

		/// <summary>Write item predictions (scores) for a given user to a TextWriter object</summary>
		/// <param name="recommender">the <see cref="IRecommender"/> to use for making the predictions</param>
		/// <param name="user_id">ID of the user to make recommendations for</param>
		/// <param name="candidate_items">list of candidate items</param>
		/// <param name="ignore_items">list of items for which no predictions should be made</param>
		/// <param name="num_predictions">the number of items to return per user, -1 if there should be no limit</param>
		/// <param name="writer">the <see cref="TextWriter"/> to write to</param>
		/// <param name="user_mapping">an <see cref="IMapping"/> object for the user IDs</param>
		/// <param name="item_mapping">an <see cref="IMapping"/> object for the item IDs</param>
		static public void WritePredictions(
			this IRecommender recommender,
			int user_id,
			ICollection<int> candidate_items,
			ICollection<int> ignore_items,
			int num_predictions,
			TextWriter writer,
			IMapping user_mapping, IMapping item_mapping)
		{
			if (user_mapping == null)
				user_mapping = new IdentityMapping();
			if (item_mapping == null)
				item_mapping = new IdentityMapping();
			
			
			
			var ordered_items = recommender.Recommend(
				user_id, n:num_predictions,
				ignore_items:ignore_items, candidate_items:candidate_items);

			writer.Write("{0}\t[", user_mapping.ToOriginalID(user_id));
			if (ordered_items.Count > 0)
			{
				writer.Write("{0}:{1}", item_mapping.ToOriginalID(ordered_items[0].Item1), ordered_items[0].Item2.ToString(CultureInfo.InvariantCulture));
				for (int i = 1; i < ordered_items.Count; i++)
				{
					int item_id = ordered_items[i].Item1;
					float score = ordered_items[i].Item2;
					writer.Write(",{0}:{1}", item_mapping.ToOriginalID(item_id), score.ToString(CultureInfo.InvariantCulture));
				}
			}
			writer.WriteLine("]");
		}
	}
}