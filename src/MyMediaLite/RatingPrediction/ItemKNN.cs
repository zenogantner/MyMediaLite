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
using MyMediaLite.DataType;
using MyMediaLite.Data;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted item-based kNN engine</summary>
	/// <remarks>This engine supports online updates.</remarks>
	public abstract class ItemKNN : KNN
	{
		/// <summary>Matrix indicating which item was rated by which user</summary>
		protected SparseBooleanMatrix data_item;

		///
		public override IRatings Ratings
		{
			set {
				base.Ratings = value;

				data_item = new SparseBooleanMatrix();
				for (int index = 0; index < Ratings.Count; index++)
					data_item[Ratings.Items[index], Ratings.Users[index]] = true;
			}
		}

		///
		public ItemKNN() : base() { }

		/// <summary>Get positively correlated entities</summary>
		protected Func<int, IList<int>> GetPositivelyCorrelatedEntities;

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the engine, a suitable average is returned.
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

			if ((user_id > MaxUserID) || (item_id > correlation.NumberOfRows - 1))
				return base.Predict(user_id, item_id);

			IList<int> relevant_items = GetPositivelyCorrelatedEntities(item_id);

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = K;
			foreach (int item_id2 in relevant_items)
				if (data_item[item_id2, user_id])
				{
					double rating = ratings.Get(user_id, item_id2, ratings.ByItem[item_id2]);
					double weight = correlation[item_id, item_id2];
					weight_sum += weight;
					sum += weight * (rating - base.Predict(user_id, item_id2));

					if (--neighbors == 0)
						break;
				}

			double result = base.Predict(user_id, item_id);
			if (weight_sum != 0)
				result += sum / weight_sum;

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
			data_item[item_id, user_id] = true;
			RetrainItem(item_id);
		}

		///
		public override void UpdateRating(int user_id, int item_id, double rating)
		{
			base.UpdateRating(user_id, item_id, rating);
			RetrainItem(item_id);
		}

		///
		public override void RemoveRating(int user_id, int item_id)
		{
			base.RemoveRating(user_id, item_id);
			data_item[item_id, user_id] = false;
			RetrainItem(user_id);
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddUser(item_id);
			correlation.AddEntity(item_id);
		}

		///
		public override void LoadModel(string filename)
		{
			base.LoadModel(filename);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}
	}
}