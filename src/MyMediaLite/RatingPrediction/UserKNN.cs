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

using System;
using System.Collections.Generic;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted user-based kNN engine</summary>
	/// <remarks>This engine supports online updates.</remarks>
	public abstract class UserKNN : KNN
	{
		/// <summary>boolean matrix indicating which user rated which item</summary>
		protected SparseBooleanMatrix data_user;

		///
		public UserKNN() : base() { }

		///
		public override IRatings Ratings
		{
			set	{
				base.Ratings = value;
				data_user = new SparseBooleanMatrix();
				for (int index = 0; index < Ratings.Count; index++)
					data_user[ratings.Users[index], ratings.Items[index]] = true;
			}
		}

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the engine, a suitable average rating is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override double Predict(int user_id, int item_id)
		{
			if (user_id < 0)
				throw new ArgumentException("user is unknown: " + user_id);
			if (item_id < 0)
				throw new ArgumentException("item is unknown: " + item_id);

			if ((user_id > correlation.NumberOfRows - 1) || (item_id > MaxItemID))
				return base.Predict(user_id, item_id);

			IList<int> relevant_users = correlation.GetPositivelyCorrelatedEntities(user_id);

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = K;
			foreach (int user_id2 in relevant_users)
			{
				if (data_user[user_id2, item_id])
				{
					double rating = ratings.Get(user_id2, item_id, ratings.ByUser[user_id2]);

					double weight = correlation[user_id, user_id2];
					weight_sum += weight;
					sum += weight * (rating - base.Predict(user_id2, item_id));

					if (--neighbors == 0)
						break;
				}
			}

			double result = base.Predict(user_id, item_id);
			if (weight_sum != 0)
			{
				double modification = sum / weight_sum;
				result += modification;
			}

			if (result > MaxRating)
				result = MaxRating;
			if (result < MinRating)
				result = MinRating;
			return result;
		}

		///
		public override void AddRating(int user_id, int item_id, double rating)
		{
			base.AddRating(user_id, item_id, rating);
			data_user[user_id, item_id] = true;
			RetrainUser(user_id);
		}

		///
		public override void UpdateRating(int user_id, int item_id, double rating)
		{
			base.UpdateRating(user_id, item_id, rating);
			RetrainUser(user_id);
		}

		///
		public override void RemoveRating(int user_id, int item_id)
		{
			base.RemoveRating(user_id, item_id);
			data_user[user_id, item_id] = false;
			RetrainUser(user_id);
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
			correlation.AddEntity(user_id);
		}
	}
}