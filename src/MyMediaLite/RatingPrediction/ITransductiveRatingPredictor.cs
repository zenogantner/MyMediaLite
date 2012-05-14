// Copyright (C) 2012 Zeno Gantner
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
//

using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Data;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Rating predictor that knows beforehand what it will have to rate</summary>
	/// <remarks>
	/// This is not so interesting for real-world use, but it useful for rating prediction
	/// competitions like the Netflix Prize.
	/// </remarks>
	public interface ITransductiveRatingPredictor : IRatingPredictor
	{
		/// <summary>user-item combinations that are known to be queried</summary>
		IDataSet AdditionalFeedback { get; set; }
	}

	/// <summary>Helper methods for ITransductiveRatingPredictor</summary>
	public static class TransductiveRatingPredictorExtensions
	{
		/// <summary>For each item, get the users who rated it, both from the training and the test data</summary>
		/// <returns>array of array of user IDs</returns>
		/// <param name='recommender'>the recommender to retrieve the data from</param>
		public static int[][] UsersWhoRated(this ITransductiveRatingPredictor recommender)
		{
			var ratings             = recommender.Ratings;
			var additional_feedback = recommender.AdditionalFeedback;
			int max_item_id = Math.Max(ratings.MaxItemID, additional_feedback.MaxItemID);

			var users_who_rated_the_item = new int[max_item_id + 1][];
			for (int item_id = 0; item_id <= max_item_id; item_id++)
			{
				var training_users = item_id <= ratings.MaxItemID             ? from index in             ratings.ByItem[item_id] select             ratings.Users[index] : new int[0];
				var test_users     = item_id <= additional_feedback.MaxItemID ? from index in additional_feedback.ByItem[item_id] select additional_feedback.Users[index] : new int[0];

				users_who_rated_the_item[item_id] = training_users.Union(test_users).ToArray();
			}
			return users_who_rated_the_item;
		}

		/// <summary>For each user, get the items they rated, both from the training and the test data</summary>
		/// <returns>array of array of item IDs</returns>
		/// <param name='recommender'>the recommender to retrieve the data from</param>
		public static int[][] ItemsRatedByUser(this ITransductiveRatingPredictor recommender)
		{
			var ratings             = recommender.Ratings;
			var additional_feedback = recommender.AdditionalFeedback;
			int max_user_id = Math.Max(ratings.MaxUserID, additional_feedback.MaxUserID);

			var items_rated_by_user = new int[max_user_id + 1][];
			for (int user_id = 0; user_id <= max_user_id; user_id++)
			{
				var training_items = user_id <= ratings.MaxUserID             ? from index in             ratings.ByUser[user_id] select             ratings.Items[index] : new int[0];
				var test_items     = user_id <= additional_feedback.MaxUserID ? from index in additional_feedback.ByUser[user_id] select additional_feedback.Items[index] : new int[0];

				items_rated_by_user[user_id] = training_items.Union(test_items).ToArray();
			}
			return items_rated_by_user;
		}

		/// <summary>Compute the number of feedback events per user</summary>
		/// <returns>number of feedback events in both the training and tests data sets, per user</returns>
		/// <param name='recommender'>the recommender to get the data from</param>
		public static int[] UserFeedbackCounts(this ITransductiveRatingPredictor recommender)
		{
			int max_user_id = Math.Max(recommender.Ratings.MaxUserID, recommender.AdditionalFeedback.MaxUserID);
			var result = new int[max_user_id + 1];

			for (int user_id = 0; user_id <= max_user_id; user_id++)
			{
				if (user_id <= recommender.Ratings.MaxUserID)
					result[user_id] += recommender.Ratings.CountByUser[user_id];
				if (user_id <= recommender.AdditionalFeedback.MaxUserID)
					result[user_id] += recommender.AdditionalFeedback.CountByUser[user_id];
			}
			return result;
		}

		/// <summary>Compute the number of feedback events per item</summary>
		/// <returns>number of feedback events in both the training and tests data sets, per item</returns>
		/// <param name='recommender'>the recommender to get the data from</param>
		public static int[] ItemFeedbackCounts(this ITransductiveRatingPredictor recommender)
		{
			int max_item_id = Math.Max(recommender.Ratings.MaxItemID, recommender.AdditionalFeedback.MaxItemID);
			var result = new int[max_item_id + 1];

			for (int item_id = 0; item_id <= max_item_id; item_id++)
			{
				if (item_id <= recommender.Ratings.MaxItemID)
					result[item_id] += recommender.Ratings.CountByItem[item_id];
				if (item_id <= recommender.AdditionalFeedback.MaxItemID)
					result[item_id] += recommender.AdditionalFeedback.CountByItem[item_id];
			}
			return result;
		}
	}
}
