// Copyright (C) 2012, 2013 Zeno Gantner
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
		IInteractions AdditionalInteractions { get; set; }
	}

	/// <summary>Helper methods for ITransductiveRatingPredictor</summary>
	public static class TransductiveRatingPredictorExtensions
	{
		/// <summary>For each item, get the users who rated it, both from the training and the test data</summary>
		/// <returns>array of array of user IDs</returns>
		/// <param name='recommender'>the recommender to retrieve the data from</param>
		public static int[][] UsersWhoRated(this ITransductiveRatingPredictor recommender)
		{
			var interactions            = recommender.Interactions;
			var additional_interactions = recommender.AdditionalInteractions;
			int max_item_id = Math.Max(interactions.MaxItemID, additional_interactions.MaxItemID);

			var users_who_rated_the_item = new int[max_item_id + 1][];
			for (int item_id = 0; item_id <= max_item_id; item_id++)
			{
				var training_users = item_id <= interactions.MaxItemID            ?            interactions.ByItem(item_id).Users : new HashSet<int>();
				var test_users     = item_id <= additional_interactions.MaxItemID ? additional_interactions.ByItem(item_id).Users : new HashSet<int>();

				users_who_rated_the_item[item_id] = training_users.Union(test_users).ToArray();
			}
			return users_who_rated_the_item;
		}

		/// <summary>For each user, get the items they rated, both from the training and the test data</summary>
		/// <returns>array of array of item IDs</returns>
		/// <param name='recommender'>the recommender to retrieve the data from</param>
		public static int[][] ItemsRatedByUser(this ITransductiveRatingPredictor recommender)
		{
			var interactions            = recommender.Interactions;
			var additional_interactions = recommender.AdditionalInteractions;
			int max_user_id = Math.Max(interactions.MaxUserID, additional_interactions.MaxUserID);

			var items_rated_by_user = new int[max_user_id + 1][];
			for (int user_id = 0; user_id <= max_user_id; user_id++)
			{
				var training_items = user_id <= interactions.MaxUserID            ?            interactions.ByUser(user_id).Items : new HashSet<int>();
				var test_items     = user_id <= additional_interactions.MaxUserID ? additional_interactions.ByUser(user_id).Items : new HashSet<int>();

				items_rated_by_user[user_id] = training_items.Union(test_items).ToArray();
			}
			return items_rated_by_user;
		}

		/// <summary>Compute the number of feedback events per user</summary>
		/// <returns>number of feedback events in both the training and tests data sets, per user</returns>
		/// <param name='recommender'>the recommender to get the data from</param>
		public static int[] UserFeedbackCounts(this ITransductiveRatingPredictor recommender)
		{
			int max_user_id = Math.Max(recommender.Interactions.MaxUserID, recommender.AdditionalInteractions.MaxUserID);
			var result = new int[max_user_id + 1];

			for (int user_id = 0; user_id <= max_user_id; user_id++)
			{
				if (user_id <= recommender.Interactions.MaxUserID)
					result[user_id] += recommender.Interactions.ByUser(user_id).Count;
				if (user_id <= recommender.AdditionalInteractions.MaxUserID)
					result[user_id] += recommender.AdditionalInteractions.ByUser(user_id).Count;
			}
			return result;
		}

		/// <summary>Compute the number of feedback events per item</summary>
		/// <returns>number of feedback events in both the training and tests data sets, per item</returns>
		/// <param name='recommender'>the recommender to get the data from</param>
		public static int[] ItemFeedbackCounts(this ITransductiveRatingPredictor recommender)
		{
			int max_item_id = Math.Max(recommender.Interactions.MaxItemID, recommender.AdditionalInteractions.MaxItemID);
			var result = new int[max_item_id + 1];

			for (int item_id = 0; item_id <= max_item_id; item_id++)
			{
				if (item_id <= recommender.Interactions.MaxItemID)
					result[item_id] += recommender.Interactions.ByItem(item_id).Count;
				if (item_id <= recommender.AdditionalInteractions.MaxItemID)
					result[item_id] += recommender.AdditionalInteractions.ByItem(item_id).Count;
			}
			return result;
		}
	}
}
