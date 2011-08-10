// Copyright (C) 2011 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.GroupRecommendation
{
	/// <summary>Group recommender that averages user scores weighted by the rating frequency of the individual users</summary>
	public class WeightedAverage : GroupRecommender
	{
		///
		public WeightedAverage(IRecommender recommender) : base(recommender) { }

		///
		public override IList<int> RankItems(ICollection<int> users, ICollection<int> items)
		{
			var item_recommender = recommender as ItemRecommender;
			var user_weights = new Dictionary<int, int>();
			foreach (int user_id in users)
				user_weights[user_id] = item_recommender.Feedback.UserMatrix.GetEntriesByRow(user_id).Count;
			
			var average_scores = new Dictionary<int, double>();

			foreach (int item_id in items)
			{
				average_scores[item_id] = 0;
				foreach (int user_id in users) // TODO consider taking CanPredict into account
					average_scores[item_id] += user_weights[user_id] * recommender.Predict(user_id, item_id);
				average_scores[item_id] /= users.Count;
			}

			var ranked_items = new List<int>(items);
			ranked_items.Sort(delegate(int i1, int i2) { return average_scores[i2].CompareTo(average_scores[i1]); } );

			return ranked_items;
		}
	}
}

