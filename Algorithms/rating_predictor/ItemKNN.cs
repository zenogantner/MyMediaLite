// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.data_type;
using MyMediaLite.data;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	/// <summary>This engine supports online updates.</summary>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public abstract class ItemKNN : KNN
	{
		/// <summary>
		/// Matrix indicating which item was rated by which user
		/// </summary>
		protected SparseBooleanMatrix data_item;

		/// <inheritdoc />
		public override RatingData Ratings
		{
			set
			{
				base.Ratings = value;

	            data_item = new SparseBooleanMatrix();
				foreach (RatingEvent r in ratings)
        	       	data_item[r.item_id, r.user_id] = true;
			}
		}

		protected Func<int, IList<int>> GetPositivelyCorrelatedEntities;

		/// <summary>
		/// Predict the rating of a given user for a given item.
		///
		/// If the user or the item are not known to the engine, a suitable average is returned.
		/// To avoid this behavior for unknown entities, use CanPredictRating() to check before.
		/// </summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
        public override double Predict(int user_id, int item_id)
        {
            if (user_id < 0)
                throw new ArgumentException("user is unknown: " + user_id);
            if (item_id < 0)
                throw new ArgumentException("item is unknown: " + item_id);			

            if ((user_id > MaxUserID) || (item_id > MaxItemID))
                return base.Predict(user_id, item_id);			
			
			// TODO think about also including negatively correlated entities
			IList<int> relevant_items = GetPositivelyCorrelatedEntities(item_id);

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = k;
			foreach (int item_id2 in relevant_items)
				if (data_item[item_id2, user_id])
				{
					RatingEvent r = ratings.ByItem[item_id2].FindRating(user_id, item_id2);
					double weight = correlation[item_id, item_id2];
					weight_sum += weight;
					sum += weight * (r.rating - base.Predict(user_id, item_id2));

					if (--neighbors == 0)
						break;
				}

			double result = base.Predict(user_id, item_id);
			if (weight_sum != 0)
			{
				double modification = sum / weight_sum;
				result += modification;
			}

			if (result > MaxRatingValue)
				result = MaxRatingValue;
            if (result < MinRatingValue)
				result = MinRatingValue;
			return result;
        }

        /// <inheritdoc/>
        public override void AddRating(int user_id, int item_id, double rating)
        {
			base.AddRating(user_id, item_id, rating);
			data_item[item_id, user_id] = true;
            RetrainItem(item_id);
        }

        /// <inheritdoc/>
        public override void UpdateRating(int user_id, int item_id, double rating)
        {
			base.UpdateRating(user_id, item_id, rating);
            RetrainItem(item_id);
        }

        /// <inheritdoc/>
        public override void RemoveRating(int user_id, int item_id)
        {
			base.RemoveRating(user_id, item_id);
			data_item[item_id, user_id] = false;
            RetrainItem(user_id);
        }

        /// <inheritdoc/>
        public override void AddItem(int item_id)
        {
            base.AddUser(item_id);
			correlation.AddEntity(item_id);
        }

		/// <inheritdoc/>
		public override void LoadModel(string filePath)
		{
			base.LoadModel(filePath);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}
	}
}