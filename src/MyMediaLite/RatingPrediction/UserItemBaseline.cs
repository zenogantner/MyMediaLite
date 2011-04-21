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
	// TODO run CV internally in order to find suitable hyperparameters

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
		public double RegU { get { return reg_u; } set { reg_u = value; } }
		private double reg_u = double.NaN;

		/// <summary>Regularization parameter for the item biases</summary>
		/// <remarks>If not set, the recommender will try to find suitable values.</remarks>
		public double RegI { get { return reg_i; } set { reg_i = value; } }
		private double reg_i = double.NaN;

		private double global_average;
		private double[] user_biases;
		private double[] item_biases;

		/// <inheritdoc/>
		public override void Train()
		{
			if (double.IsNaN(RegU) || double.IsNaN(RegI)) // TODO handle separately
			{
				var ni = new NumberFormatInfo();

				// save ratings for later use
				var all_ratings = Ratings;

				// hyperparameter search
				//var split = new RatingCrossValidationSplit(Ratings, 5);
				var split = new RatingsSimpleSplit(Ratings, 0.2);
				int basis = 2;
				// rough search
				var hp_values_u = new double[] {-5, -3, -1, 0, 1, 3, 5};
				var hp_values_i = new double[] {-5, -3, -1, 0, 1, 3, 5};
				double estimate = GridSearch.FindMinimumExponential("RMSE", "reg_u", "reg_i", hp_values_u, hp_values_i, basis, this, delegate() { TrainModel(); }, split);
				Console.Error.WriteLine("estimated RMSE for {0}, {1}: {2}", RegU, RegI, estimate.ToString(ni));
				// check for edges
				double lower_edge_u = -5;
				double upper_edge_u = 5;
				double lower_edge_i = -5;
				double upper_edge_i = 5;
				while ( (Math.Log(RegU, basis) == lower_edge_u) || Math.Log(RegU, basis) == upper_edge_u
				       || Math.Log(RegI, basis) == lower_edge_i || Math.Log(RegI, basis) == upper_edge_i)
				{
					// search around the current values
					hp_values_u = new double[5];
					for (int i = 0; i < hp_values_u.Length; i++)
						hp_values_u[i] = Math.Log(RegU, basis) + i - 2;
					hp_values_i = new double[5];
					for (int i = 0; i < hp_values_i.Length; i++)
						hp_values_i[i] = Math.Log(RegI, basis) + i - 2;
					estimate = GridSearch.FindMinimumExponential("RMSE", "reg_u", "reg_i", hp_values_u, hp_values_i, basis, this, delegate() { TrainModel(); }, split);
					Console.Error.WriteLine("estimated RMSE for {0}, {1}: {2}", RegU, RegI, estimate.ToString(ni));
				}
				// refine search
				hp_values_u = new double[5];
				for (int i = 0; i < hp_values_u.Length; i++)
					hp_values_u[i] = Math.Log(RegU, basis) + (double) (i - 2) / 2;
				hp_values_i = new double[5];
				for (int i = 0; i < hp_values_i.Length; i++)
					hp_values_i[i] = Math.Log(RegI, basis) + (double) (i - 2) / 2;
				estimate = GridSearch.FindMinimumExponential("RMSE", "reg_u", "reg_i", hp_values_u, hp_values_i, basis, this, delegate() { TrainModel(); }, split);
				Console.Error.WriteLine("estimated RMSE for {0}, {1}: {2}", RegU, RegI, estimate.ToString(ni));
				// TODO this is a rather general grid search, move it to the GridSearch class

				// reset ratings
				Ratings = all_ratings;

				Console.WriteLine(this);
			}

			TrainModel();
		}

		void TrainModel()
		{
			global_average = Ratings.Average;

			user_biases = new double[MaxUserID + 1];
			item_biases = new double[MaxItemID + 1];

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
					item_biases[i] = item_biases[i] / (reg_i + item_ratings_count[i]);

			// compute user biases
			for (int index = 0; index < Ratings.Count; index++)
			{
				user_biases[Ratings.Users[index]] += Ratings[index] - global_average - item_biases[Ratings.Items[index]];
				user_ratings_count[Ratings.Users[index]]++;
			}
			for (int u = 0; u < user_biases.Length; u++)
				if (user_ratings_count[u] != 0)
					user_biases[u] = user_biases[u] / (reg_u + user_ratings_count[u]);
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
					user_biases[user_id] = user_biases[user_id] / (reg_u + ratings.ByUser[user_id].Count);
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
					item_biases[item_id] = item_biases[item_id] / (reg_i + ratings.ByItem[item_id].Count);
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

			return string.Format(ni, "UserItemBaseline reg_u={0} reg_i={1}", reg_u, reg_i);
		}
	}
}