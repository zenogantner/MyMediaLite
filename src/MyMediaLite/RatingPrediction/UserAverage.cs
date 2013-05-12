// Copyright (C) 2011, 2012, 2013 Zeno Gantner
// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Uses the average rating value of a user for predictions</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// </remarks>
	public class UserAverage : EntityAverage, IFoldInRatingPredictor
	{
		///
		public override void Train()
		{
			base.Train(EntityType.USER);
		}

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= Interactions.MaxUserID);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id < entity_averages.Count)
				return entity_averages[user_id];
			else
				return global_average;
		}

		///
		public IList<Tuple<int, float>> ScoreItems(IList<Tuple<int, float>> rated_items, IList<int> candidate_items)
		{
			float user_average = (float) (from pair in rated_items select pair.Item2).Average();

			// score the items
			var result = new Tuple<int, float>[candidate_items.Count];
			for (int i = 0; i < candidate_items.Count; i++)
			{
				int item_id = candidate_items[i];
				result[i] = Tuple.Create(item_id, user_average);
			}
			return result;
		}
	}
}