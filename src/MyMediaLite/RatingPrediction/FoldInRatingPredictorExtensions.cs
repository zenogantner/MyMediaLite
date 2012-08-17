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
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Extension methods for IFoldInRatingPredictor</summary>
	public static class FoldInRatingPredictorExtensions
	{
		/// <summary>Recommend top N items, based on a user description by ratings</summary>
		/// <returns>a list of item IDs with scores</returns>
		/// <param name='recommender'>the IFoldInRatingPredictor recommender</param>
		/// <param name='rated_items'>a list of item IDs and ratings describing the user</param>
		/// <param name='candidate_items'>the recommendation candidates</param>
		/// <param name='n'>the number of items to recommend</param>
		public static IList<Tuple<int, float>> RecommendItems(this IFoldInRatingPredictor recommender, IList<Tuple<int, float>> rated_items, IList<int> candidate_items, int n)
		{
			var scored_items = recommender.ScoreItems(rated_items, candidate_items);
			return scored_items.OrderByDescending(x => x.Item2).Take(n).ToArray();
		}

		/// <summary>Recommend top N items, based on a user description by ratings</summary>
		/// <returns>a list of item IDs with scores</returns>
		/// <param name='recommender'>the IFoldInRatingPredictor recommender</param>
		/// <param name='rated_items'>a list of item IDs and ratings describing the user</param>
		/// <param name='n'>the number of items to recommend</param>
		public static IList<Tuple<int, float>> RecommendItems(this IFoldInRatingPredictor recommender, IList<Tuple<int, float>> rated_items, int n)
		{
			var candidate_items = Enumerable.Range(0, ((RatingPredictor)recommender).Ratings.MaxItemID - 1).ToArray();
			var scored_items = recommender.ScoreItems(rated_items, candidate_items);
			return scored_items.OrderByDescending(x => x.Item2).Take(n).ToArray();
		}

		/// <summary>Recommend top N items, based on a user description by ratings</summary>
		/// <returns>a list of item IDs with scores</returns>
		/// <param name='recommender'>the IFoldInRatingPredictor recommender</param>
		/// <param name='rated_items'>a list of item IDs and ratings describing the user</param>
		public static IList<Tuple<int, float>> ScoreItems(this IFoldInRatingPredictor recommender, IList<Tuple<int, float>> rated_items)
		{
			var candidate_items = Enumerable.Range(0, ((RatingPredictor)recommender).Ratings.MaxItemID - 1).ToArray();
			return recommender.ScoreItems(rated_items, candidate_items);
		}
	}
}
