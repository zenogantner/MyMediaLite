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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MyMediaLite.data;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	// TODO: think about running CV internally in order to find suitable hyperparameters

    /// <summary>
    /// Uses the average rating value, plus a regularized user and item bias
    /// for prediction.
    ///
	/// The method is described in section 2.1 of
	/// Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	/// Transactions on Knowledge Discovery from Data (TKDD), 2009
	///
	/// This engine supports online updates.
    /// </summary>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class UserItemBaseline : Memory
    {
		public double reg_u = 25;
		public double reg_i = 10;

        public bool UpdateUsers { get; set; }
        public bool UpdateItems { get; set; }

		private double global_average;
		private double[] user_biases;
		private double[] item_biases;

        /// <inheritdoc />
        public override void Train()
        {
			global_average = ratings.Average;
			user_biases = new double[MaxUserID + 1];
			item_biases = new double[MaxItemID + 1];

			int[] user_ratings_count = new int[MaxUserID + 1];
			int[] item_ratings_count = new int[MaxItemID + 1];

			// compute item biases
			foreach (RatingEvent r in ratings)
			{
				item_biases[r.item_id] += r.rating - global_average;
				item_ratings_count[r.item_id]++;
			}
			for (int i = 0; i < item_biases.Length; i++)
				if (item_ratings_count[i] != 0)
					item_biases[i] = item_biases[i] / (reg_i + item_ratings_count[i]);

			// compute user biases
			foreach (RatingEvent r in ratings)
			{
				user_biases[r.user_id] += r.rating - global_average - item_biases[r.item_id];
				user_ratings_count[r.user_id]++;
			}
			for (int u = 0; u < user_biases.Length; u++)
				if (user_ratings_count[u] != 0)
					user_biases[u] = user_biases[u] / (reg_u + user_ratings_count[u]);
        }

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
			double user_bias = (user_id <= MaxUserID && user_id >= 0) ? user_biases[user_id] : 0;
			double item_bias = (item_id <= MaxItemID && item_id >= 0) ? item_biases[item_id] : 0;
			double result = global_average + user_bias + item_bias;

			if (result > MaxRatingValue)
				result = MaxRatingValue;
            if (result < MinRatingValue)
				result = MinRatingValue;

			return result;
        }

		/// <inheritdoc />
		protected virtual void RetrainUser(int user_id)
		{
			if (UpdateUsers)
			{
				foreach (RatingEvent r in ratings.ByUser[user_id])
					user_biases[user_id] += r.rating - global_average - item_biases[r.item_id];
				if (ratings.ByUser[user_id].Count != 0)
					user_biases[user_id] = user_biases[user_id] / (reg_u + ratings.ByUser[user_id].Count);
			}
		}

		/// <inheritdoc />
		protected virtual void RetrainItem(int item_id)
		{
			if (UpdateItems)
			{
				foreach (RatingEvent r in ratings.ByItem[item_id])
					item_biases[item_id] += r.rating - global_average;
				if (ratings.ByItem[item_id].Count != 0)
					item_biases[item_id] = item_biases[item_id] / (reg_i + ratings.ByItem[item_id].Count);
			}
		}

        /// <inheritdoc/>
        public override void AddRating(int user_id, int item_id, double rating)
        {
			base.AddRating(user_id, item_id, rating);
			RetrainItem(item_id);
			RetrainUser(user_id);
        }

        /// <inheritdoc/>
        public override void UpdateRating(int user_id, int item_id, double rating)
        {
			base.UpdateRating(user_id, item_id, rating);
			RetrainItem(item_id);
			RetrainUser(user_id);
        }

        /// <inheritdoc/>
        public override void RemoveRating(int user_id, int item_id)
        {
            base.RemoveRating(user_id, item_id);
			RetrainItem(item_id);
			RetrainUser(user_id);
        }

        /// <inheritdoc/>
        public override void AddUser(int user_id)
        {
            base.AddUser(user_id);
			if (user_id >= this.user_biases.Length)
			{
				double[] user_biases = new double[this.MaxUserID + 1];
				Array.Copy(this.user_biases, user_biases, this.user_biases.Length);
				this.user_biases = user_biases;
			}
        }

        /// <inheritdoc/>
        public override void AddItem(int item_id)
        {
            base.AddItem(item_id);
			if (item_id >= this.item_biases.Length)
			{
				double[] item_biases = new double[this.MaxItemID + 1];
				Array.Copy(this.item_biases, item_biases, this.item_biases.Length);
				this.item_biases = item_biases;
			}
        }

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
			{
				// TODO
			}
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
				// TODO
			}
			Train();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("user-item-baseline reg_u={0} reg_i={1}", reg_u, reg_i);
		}
    }
}