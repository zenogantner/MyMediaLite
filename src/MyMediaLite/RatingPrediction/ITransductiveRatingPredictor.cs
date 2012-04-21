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

	public static class TransductiveRatingPredictorExtensions
	{
		public static int[][] UsersWhoRated(this ITransductiveRatingPredictor recommender)
		{
			var ratings             = recommender.Ratings;
			var additional_feedback = recommender.AdditionalFeedback;
			int max_item_id = Math.Max(ratings.MaxItemID, additional_feedback.MaxItemID);

			var users_who_rated_the_item = new int[max_item_id + 1][];
			for (int item_id = 0; item_id <= max_item_id; item_id++)
			{
				IEnumerable<int> index_list = (item_id <= additional_feedback.MaxItemID)
					? ratings.ByItem[item_id].Concat(additional_feedback.ByUser[item_id])
					: ratings.ByItem[item_id];
				users_who_rated_the_item[item_id] = (from index in index_list select ratings.Users[index]).ToArray();
			}
			return users_who_rated_the_item;
		}

		public static int[][] ItemsRatedByUser(this ITransductiveRatingPredictor recommender)
		{
			var ratings             = recommender.Ratings;
			var additional_feedback = recommender.AdditionalFeedback;
			int max_user_id = Math.Max(ratings.MaxUserID, additional_feedback.MaxUserID);

			var items_rated_by_user = new int[max_user_id + 1][];
			for (int u = 0; u <= max_user_id; u++)
			{
				IEnumerable<int> index_list = (u <= additional_feedback.MaxUserID)
					? ratings.ByUser[u].Concat(additional_feedback.ByUser[u])
					: ratings.ByUser[u];
				items_rated_by_user[u] = (from index in index_list select ratings.Items[index]).ToArray();
			}
			return items_rated_by_user;
		}
	}
}
