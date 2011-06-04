// Copyright (C) 2010 Zeno Gantner, Steffen Rendle, Christoph Freudenthaler
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
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Simple matrix factorization class</summary>
	/// <remarks>
	/// Factorizing the observed rating values using a factor matrix for users and one for items.
	/// This class can update the factorization online.
	///
	/// After training, an ArithmeticException is thrown if there are NaN values in the model.
	/// NaN values occur if values become too large or too small to be represented by the type double.
	/// If you encounter such problems, there are three ways to fix them:
	/// (1) (preferred) Use BiasedMatrixFactorization, which is more stable.
	/// (2) Change the range of rating values (1 to 5 works generally well with the default settings).
	/// (3) Change the learn_rate (decrease it if your range is larger than 1 to 5).
	/// </remarks>
	public class MatrixFactorization : RatingPredictor, IIterativeModel
	{
		/// <summary>Matrix containing the latent user factors</summary>
		protected Matrix<double> user_factors;

		/// <summary>Matrix containing the latent item factors</summary>
		protected Matrix<double> item_factors;

		/// <summary>The bias (global average)</summary>
		protected double global_bias;

		/// <summary>Mean of the normal distribution used to initialize the factors</summary>
		public double InitMean { get; set; }

		/// <summary>Standard deviation of the normal distribution used to initialize the factors</summary>
		public double InitStdev { get; set; }

		/// <summary>Number of latent factors</summary>
		public int NumFactors { get; set;}

		/// <summary>Learn rate</summary>
		public double LearnRate { get; set; }

		/// <summary>Regularization parameter</summary>
		public virtual double Regularization { get; set; }

		/// <summary>Number of iterations over the training data</summary>
		public int NumIter { get; set; }

		/// <summary>Create a new object</summary>
		public MatrixFactorization()
		{
			// set default values
			Regularization = 0.015;
			LearnRate = 0.01;
			NumIter = 30;
			InitStdev = 0.1;
			NumFactors = 10;
		}

		/// <summary>Initialize the model data structure</summary>
		protected override void InitModel()
		{
			base.InitModel();

			// init factor matrices
			user_factors = new Matrix<double>(Ratings.MaxUserID + 1, NumFactors);
			item_factors = new Matrix<double>(Ratings.MaxItemID + 1, NumFactors);
			MatrixUtils.InitNormal(user_factors, InitMean, InitStdev);
			MatrixUtils.InitNormal(item_factors, InitMean, InitStdev);
		}

		///
		public override void Train()
		{
			InitModel();

			// learn model parameters
			global_bias = Ratings.Average;
			LearnFactors(Ratings.RandomIndex, true, true);
		}

		///
		public virtual void Iterate()
		{
			Iterate(Ratings.RandomIndex, true, true);
		}

		/// <summary>Updates the latent factors on a user</summary>
		/// <param name="user_id">the user ID</param>
		public virtual void RetrainUser(int user_id)
		{
			if (UpdateUsers)
			{
				MatrixUtils.InitNormal(user_factors, InitMean, InitStdev, user_id);
				LearnFactors(Ratings.ByUser[(int)user_id], true, false);
			}
		}

		/// <summary>Updates the latent factors of an item</summary>
		/// <param name="item_id">the item ID</param>
		public virtual void RetrainItem(int item_id)
		{
			if (UpdateItems)
			{
				MatrixUtils.InitNormal(item_factors, InitMean, InitStdev, item_id);
				LearnFactors(Ratings.ByItem[(int)item_id], false, true);
			}
		}

		/// <summary>Iterate once over rating data and adjust corresponding factors (stochastic gradient descent)</summary>
		/// <param name="rating_indices">a list of indices pointing to the ratings to iterate over</param>
		/// <param name="update_user">true if user factors to be updated</param>
		/// <param name="update_item">true if item factors to be updated</param>
		protected virtual void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double p = Predict(u, i, false);
				double err = ratings[index] - p;

				 // Adjust factors
				 for (int f = 0; f < NumFactors; f++)
				 {
					double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					// compute factor updates
					double delta_u = err * i_f - Regularization * u_f;
					double delta_i = err * u_f - Regularization * i_f;

					// if necessary, apply updates
					if (update_user)
						MatrixUtils.Inc(user_factors, u, f, LearnRate * delta_u);
					if (update_item)
						MatrixUtils.Inc(item_factors, i, f, LearnRate * delta_i);
				 }
			}
		}

		private void LearnFactors(IList<int> rating_indices, bool update_user, bool update_item)
		{
			for (int current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate(rating_indices, update_user, update_item);
		}

		///
		protected double Predict(int user_id, int item_id, bool bound)
		{
			double result = global_bias + MatrixUtils.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			if (bound)
			{
				if (result > MaxRating)
					return MaxRating;
				if (result < MinRating)
					return MinRating;
			}
			return result;
		}

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the recommender, the global average is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override double Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1)
				return global_bias;
			if (item_id >= item_factors.dim1)
				return global_bias;

			return Predict(user_id, item_id, true);
		}

		///
		public override void AddRating(int user_id, int item_id, double rating)
		{
			base.AddRating(user_id, item_id, rating);
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		///
		public override void UpdateRating(int user_id, int item_id, double rating)
		{
			base.UpdateRating(user_id, item_id, rating);
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		///
		public override void RemoveRating(int user_id, int item_id)
		{
			base.RemoveRating(user_id, item_id);
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
			user_factors.AddRows(user_id + 1);
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);
			item_factors.AddRows(item_id + 1);
		}

		///
		public override void RemoveUser(int user_id)
		{
			base.RemoveUser(user_id);

			// set user factors to zero
			user_factors.SetRowToOneValue(user_id, 0);
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			// set item factors to zero
			item_factors.SetRowToOneValue(item_id, 0);
		}

		///
		public override void SaveModel(string filename)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
			{
				writer.WriteLine(global_bias.ToString(ni));
				IMatrixUtils.WriteMatrix(writer, user_factors);
				IMatrixUtils.WriteMatrix(writer, item_factors);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
			{
				var bias = double.Parse(reader.ReadLine(), ni);

				var user_factors = (Matrix<double>) IMatrixUtils.ReadMatrix(reader, new Matrix<double>(0, 0));
				var item_factors = (Matrix<double>) IMatrixUtils.ReadMatrix(reader, new Matrix<double>(0, 0));

				if (user_factors.NumberOfColumns != item_factors.NumberOfColumns)
					throw new Exception(
									string.Format("Number of user and item factors must match: {0} != {1}",
												  user_factors.NumberOfColumns, item_factors.NumberOfColumns));

				this.MaxUserID = user_factors.NumberOfRows - 1;
				this.MaxItemID = item_factors.NumberOfRows - 1;

				// assign new model
				this.global_bias = bias;
				if (this.NumFactors != user_factors.NumberOfColumns)
				{
					Console.Error.WriteLine("Set num_factors to {0}", user_factors.NumberOfColumns);
					this.NumFactors = user_factors.NumberOfColumns;
				}
				this.user_factors = user_factors;
				this.item_factors = item_factors;
			}
		}

		/// <summary>Compute fit (RMSE) on the training data</summary>
		/// <returns>the root mean square error (RMSE) on the training data</returns>
		public double ComputeFit()
		{
			double rmse_sum = 0;
			for (int i = 0; i < ratings.Count; i++)
				rmse_sum += Math.Pow(Predict(ratings.Users[i], ratings.Items[i]) - ratings[i], 2);

			return Math.Sqrt((double) rmse_sum / ratings.Count);
		}

		/// <summary>Compute the regularized loss</summary>
		/// <returns>the regularized loss</returns>
		public virtual double ComputeLoss()
		{
			double loss = 0;
			for (int i = 0; i < ratings.Count; i++)
			{
				int user_id = ratings.Users[i];
				int item_id = ratings.Items[i];
				loss += Math.Pow(Predict(user_id, item_id) - ratings[i], 2);
			}

			for (int u = 0; u <= MaxUserID; u++)
				loss += ratings.CountByUser[u] * Regularization * Math.Pow(VectorUtils.EuclideanNorm(user_factors.GetRow(u)), 2);

			for (int i = 0; i <= MaxItemID; i++)
				loss += ratings.CountByItem[i] * Regularization * Math.Pow(VectorUtils.EuclideanNorm(item_factors.GetRow(i)), 2);

			return loss;
		}

		///
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni,
								 "MatrixFactorization num_factors={0} regularization={1} learn_rate={2} num_iter={3} init_mean={4} init_stdev={5}",
								 NumFactors, Regularization, LearnRate, NumIter, InitMean, InitStdev);
		}
	}
}