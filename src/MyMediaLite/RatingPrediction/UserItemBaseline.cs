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
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>baseline method for rating prediction</summary>
	/// <remarks>
	/// Uses the average rating value, plus a regularized user and item bias
	/// for prediction.
	///
	/// The method is described in section 2.1 of
	/// Yehuda Koren: Factor in the Neighbors: Scalable and Accurate Collaborative Filtering,
	/// Transactions on Knowledge Discovery from Data (TKDD), 2009.
	///
	/// This engine supports online updates.
	/// </remarks>
	public class UserItemBaseline : RatingPredictor
	{
		/// <summary>Regularization parameter for the user biases</summary>
		/// <remarks>If not set, the recommender will try to find suitable values.</remarks>
		public double RegU { get; set; }

		/// <summary>Regularization parameter for the item biases</summary>
		/// <remarks>If not set, the recommender will try to find suitable values.</remarks>
		public double RegI { get; set; }

		private double global_average;
		private double[] user_biases;
		private double[] item_biases;

		/// <inheritdoc/>
		protected override void InitModel()
		{
			base.InitModel();

			user_biases = new double[MaxUserID + 1];
			item_biases = new double[MaxItemID + 1];
		}

		/// <inheritdoc/>
		public override void Train()
		{
			InitModel();

			global_average = Ratings.Average;

			int[] user_ratings_count = new int[MaxUserID + 1];
			int[] item_ratings_count = new int[MaxItemID + 1];

			// compute item biases
			for (int index = 0; index < Ratings.Count; index++)
			{
				item_biases[Ratings.Items[index]] += Ratings[index] - global_average;
				item_ratings_count[Ratings.Items[index]]++;
			}
			for (int i = 0; i < item_biases.Length; i++)
				if (item_ratings_count[i] != 0)
					item_biases[i] = item_biases[i] / (RegI + item_ratings_count[i]);

			// compute user biases
			for (int index = 0; index < Ratings.Count; index++)
			{
				user_biases[Ratings.Users[index]] += Ratings[index] - global_average - item_biases[Ratings.Items[index]];
				user_ratings_count[Ratings.Users[index]]++;
			}
			for (int u = 0; u < user_biases.Length; u++)
				if (user_ratings_count[u] != 0)
					user_biases[u] = user_biases[u] / (RegU + user_ratings_count[u]);
		}

		/// <inheritdoc/>
		public override double Predict(int user_id, int item_id)
		{
			double user_bias = (user_id <= MaxUserID && user_id >= 0) ? user_biases[user_id] : 0;
			double item_bias = (item_id <= MaxItemID && item_id >= 0) ? item_biases[item_id] : 0;
			double result = global_average + user_bias + item_bias;

			if (result > MaxRating)
				result = MaxRating;
			if (result < MinRating)
				result = MinRating;

			return result;
		}

		/// <inheritdoc/>
		protected virtual void RetrainUser(int user_id)
		{
			if (UpdateUsers)
			{
				foreach (int index in ratings.ByUser[user_id])
					user_biases[user_id] += Ratings[index] - global_average - item_biases[Ratings.Items[index]];
				if (ratings.ByUser[user_id].Count != 0)
					user_biases[user_id] = user_biases[user_id] / (RegU + ratings.ByUser[user_id].Count);
			}
		}

		/// <inheritdoc/>
		protected virtual void RetrainItem(int item_id)
		{
			if (UpdateItems)
			{
				foreach (int index in ratings.ByItem[item_id])
					item_biases[item_id] += Ratings[index] - global_average;
				if (ratings.ByItem[item_id].Count != 0)
					item_biases[item_id] = item_biases[item_id] / (RegI + ratings.ByItem[item_id].Count);
			}
		}

		/// <inheritdoc/>
		public override void Add(int user_id, int item_id, double rating)
		{
			base.Add(user_id, item_id, rating);
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

		/// <inheritdoc/>
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
			{
				// TODO
			}
		}

		/// <inheritdoc/>
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
			{
				// TODO
			}
			Train(); // instead ;-)
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni, "UserItemBaseline reg_u={0} reg_i={1}", RegU, RegI);
		}
	}
}