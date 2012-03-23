// Copyright (C) 2011, 2012 Zeno Gantner
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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Matrix factorization with factor-wise learning</summary>
	/// <remarks>
	///   <para>
	///     Similar to the approach described in Simon Funk's seminal blog post: http://sifter.org/~simon/journal/20061211.html
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Robert Bell, Yehuda Koren, Chris Volinsky:
	///         Modeling Relationships at Multiple Scales to Improve Accuracy of Large Recommender Systems,
	///         ACM Int. Conference on Knowledge Discovery and Data Mining (KDD'07), 2007.
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///      This recommender does NOT support incremental updates.
	///   </para>
	/// </remarks>
	public class FactorWiseMatrixFactorization : RatingPredictor, IIterativeModel
	{
		///
		public override IRatings Ratings
		{
			set {
				base.Ratings = value;
				global_effects.Ratings = value;
			}
		}

		/// <summary>regularization constant for the user bias of the underlying baseline predictor</summary>
		public float RegU { get { return global_effects.RegU; } set { global_effects.RegU = value; } }

		/// <summary>regularization constant for the item bias of the underlying baseline predictor</summary>
		public float RegI { get { return global_effects.RegI; } set { global_effects.RegI = value; } }

		/// <summary>Matrix containing the latent user factors</summary>
		Matrix<float> user_factors;

		/// <summary>Matrix containing the latent item factors</summary>
		Matrix<float> item_factors;

		UserItemBaseline global_effects = new UserItemBaseline();

		int num_learned_factors;

		/// <summary>Number of latent factors</summary>
		public uint NumFactors { get; set;}

		/// <summary>Number of iterations (in this case: number of latent factors)</summary>
		public uint NumIter { get; set;}

		/// <summary>Shrinkage parameter</summary>
		/// <remarks>
		/// alpha in the Bell et al. paper
		/// </remarks>
		public virtual double Shrinkage { get; set; }

		/// <summary>Sensibility parameter (stopping criterion for parameter fitting)</summary>
		/// <remarks>
		/// epsilon in the Bell et al. paper
		/// </remarks>
		public virtual double Sensibility { get; set; }

		/// <summary>Mean of the normal distribution used to initialize the factors</summary>
		public double InitMean { get; set; }

		/// <summary>Standard deviation of the normal distribution used to initialize the factors</summary>
		public double InitStdev { get; set; }

		/// <summary>Default constructor</summary>
		public FactorWiseMatrixFactorization() : base()
		{
			// set default values
			Shrinkage = 25;
			NumFactors = 10;
			NumIter = 10;
			Sensibility = 0.00001;
			InitStdev = 0.1;
		}

		///
		public override void Train()
		{
			// init factor matrices
			user_factors = new Matrix<float>(MaxUserID + 1, NumFactors);
			item_factors = new Matrix<float>(MaxItemID + 1, NumFactors);

			// init+train global effects model
			global_effects.Ratings = ratings;
			global_effects.Train();

			// learn model parameters
			num_learned_factors = 0;
			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		///
		public virtual void Iterate()
		{
			if (num_learned_factors >= NumFactors)
				return;

			// compute residuals
			var residuals = new float[ratings.Count];
			for (int index = 0; index < ratings.Count; index++)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];
				residuals[index] = (float) (ratings[index] - Predict(u, i));
				int n_ui = Math.Min(ratings.ByUser[u].Count, ratings.ByItem[i].Count);
				residuals[index] *= (float) (n_ui / (n_ui + Shrinkage));
			}

			// initialize new latent factors
			user_factors.ColumnInitNormal(num_learned_factors, InitMean, InitStdev);
			item_factors.ColumnInitNormal(num_learned_factors, InitMean, InitStdev);

			// compute the next factor by solving many least squares problems with one variable each
			double err     = double.MaxValue / 2;
			double err_old = double.MaxValue;
			while (err / err_old < 1 - Sensibility)
			{
				{
					var user_factors_update_numerator   = new float[MaxUserID + 1];
					var user_factors_update_denominator = new float[MaxUserID + 1];

					// compute updates in one pass over the data
					for (int index = 0; index < ratings.Count; index++)
					{
						int u = ratings.Users[index];
						int i = ratings.Items[index];

						user_factors_update_numerator[u]   += residuals[index] * item_factors[i, num_learned_factors];
						user_factors_update_denominator[u] += item_factors[i, num_learned_factors] * item_factors[i, num_learned_factors];
					}

					// update user factors
					for (int u = 0; u <= MaxUserID; u++)
						if (user_factors_update_numerator[u] != 0)
							user_factors[u, num_learned_factors] = user_factors_update_numerator[u] / user_factors_update_denominator[u];
				}

				{
					var item_factors_update_numerator   = new double[MaxItemID + 1];
					var item_factors_update_denominator = new double[MaxItemID + 1];

					// compute updates in one pass over the data
					for (int index = 0; index < ratings.Count; index++)
					{
						int u = ratings.Users[index];
						int i = ratings.Items[index];

						item_factors_update_numerator[i]   += residuals[index] * user_factors[u, num_learned_factors];
						item_factors_update_denominator[i] += user_factors[u, num_learned_factors] * user_factors[u, num_learned_factors];
					}

					// update item factors
					for (int i = 0; i <= MaxItemID; i++)
						if (item_factors_update_numerator[i] != 0)
							item_factors[i, num_learned_factors] = (float) (item_factors_update_numerator[i] / item_factors_update_denominator[i]);
				}

				err_old = err;
				err = this.ComputeFit();
			}

			num_learned_factors++;
		}

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the recommender, the global effects prediction is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override float Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1 || item_id >= item_factors.dim1)
				return global_effects.Predict(user_id, item_id);

			double result = global_effects.Predict(user_id, item_id) + DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;

			return (float) result;
		}

		///
		public override void SaveModel(string filename)
		{
			global_effects.SaveModel(filename + "-global-effects");

			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.03") )
			{
				writer.WriteLine(num_learned_factors);
				writer.WriteMatrix(user_factors);
				writer.WriteMatrix(item_factors);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			global_effects.LoadModel(filename + "-global-effects");
			if (ratings != null)
				global_effects.Ratings = ratings;

			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var num_learned_factors = int.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

				var user_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
				var item_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				if (user_factors.NumberOfColumns != item_factors.NumberOfColumns)
					throw new Exception(
						string.Format("Number of user and item factors must match: {0} != {1}",
							user_factors.NumberOfColumns, item_factors.NumberOfColumns));

				this.MaxUserID = user_factors.NumberOfRows - 1;
				this.MaxItemID = item_factors.NumberOfRows - 1;

				// assign new model
				this.num_learned_factors = num_learned_factors;
				if (this.NumFactors != user_factors.NumberOfColumns)
				{
					Console.Error.WriteLine("Set NumFactors to {0}", user_factors.NumberOfColumns);
					this.NumFactors = (uint) user_factors.NumberOfColumns;
				}
				this.user_factors = user_factors;
				this.item_factors = item_factors;
			}
		}

		///
		public float ComputeObjective()
		{
			return (float) Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings);
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} shrinkage={2} sensibility={3} num_iter={4} reg_u={5} reg_i={6}",
				this.GetType().Name, NumFactors, Shrinkage, Sensibility, NumIter, RegU, RegI);
		}
	}
}