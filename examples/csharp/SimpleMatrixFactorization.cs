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
using System.Globalization;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Simple matrix factorization class</summary>
	/// <remarks>
	/// Minimalistic MF implementation, without working LoadModel()/SaveModel().
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class SimpleMatrixFactorization : RatingPredictor, IIterativeModel
	{
		Matrix<double> user_factors;
		Matrix<double> item_factors;

		/// <summary>Number of latent factors</summary>
		public uint NumFactors { get; set;}
		/// <summary>Learn rate</summary>
		public double LearnRate { get; set; }
		/// <summary>Regularization parameter</summary>
		public virtual double Regularization { get; set; }
		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get; set; }

		/// <summary>Default constructor</summary>
		public SimpleMatrixFactorization()
		{
			// set default values
			Regularization = 0.1;
			LearnRate = 0.01;
			NumIter = 30;
			NumFactors = 10;
		}

		///
		public override void Train()
		{
			// init factor matrices
			user_factors = new Matrix<double>(MaxUserID + 1, NumFactors);
			item_factors = new Matrix<double>(MaxItemID + 1, NumFactors);
			MatrixUtils.InitNormal(user_factors, 0, 0.1);
			MatrixUtils.InitNormal(item_factors, 0, 0.1);

			// learn model parameters
			for (uint current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate();
		}

		/// <summary>Iterate once over rating data (stochastic gradient descent)</summary>
		/// <remarks></remarks>
		public virtual void Iterate()
		{
			foreach (int index in ratings.RandomIndex)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double p = Predict(u, i);
				double err = ratings[index] - p;

				 // Adjust factors
				 for (int f = 0; f < NumFactors; f++)
				 {
					double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					// compute factor updates
					double delta_u = err * i_f - Regularization * u_f;
					double delta_i = err * u_f - Regularization * i_f;

					// apply updates
					MatrixUtils.Inc(user_factors, u, f, LearnRate * delta_u);
					MatrixUtils.Inc(item_factors, i, f, LearnRate * delta_i);
				 }
			}
		}

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override double Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1)
				return 3;
			if (item_id >= item_factors.dim1)
				return 3;

			return MatrixUtils.RowScalarProduct(user_factors, user_id, item_factors, item_id);
		}

		///
		public double ComputeFit()
		{
			return Eval.Ratings.Evaluate(this, ratings)["RMSE"];
		}

		///
		public override void LoadModel(string file) { throw new NotImplementedException(); }

		///
		public override void SaveModel(string file) { throw new NotImplementedException(); }

		///
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
								 "MatrixFactorization num_factors={0} regularization={1} learn_rate={2} num_iter={3}",
								 NumFactors, Regularization, LearnRate, NumIter);
		}
	}
}