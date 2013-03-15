// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Zeno Gantner, Steffen Rendle, Christoph Freudenthaler
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
using System.Runtime.CompilerServices;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

[assembly: InternalsVisibleTo("Tests")]

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Simple matrix factorization class, learning is performed by stochastic gradient descent (SGD)</summary>
	/// <remarks>
	///   <para>
	///     Factorizing the observed rating values using a factor matrix for users and one for items.
	///   </para>
	///
	///   <para>
	///     NaN values in the model occur if values become too large or too small to be represented by the type float.
	///     If you encounter such problems, there are three ways to fix them:
	///     (1) (preferred) Use BiasedMatrixFactorization, which is more stable.
	///     (2) Change the range of rating values (1 to 5 works generally well with the default settings).
	///     (3) Decrease the learn_rate.
	///   </para>
	///
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class MatrixFactorization : IncrementalRatingPredictor, IIterativeModel, IFoldInRatingPredictor
	{
		/// <summary>Matrix containing the latent user factors</summary>
		protected internal Matrix<float> user_factors;

		/// <summary>Matrix containing the latent item factors</summary>
		protected internal Matrix<float> item_factors;

		/// <summary>The bias (global average)</summary>
		protected float global_bias;

		/// <summary>Mean of the normal distribution used to initialize the factors</summary>
		public double InitMean { get; set; }

		/// <summary>Standard deviation of the normal distribution used to initialize the factors</summary>
		public double InitStdDev { get; set; }

		/// <summary>Number of latent factors</summary>
		public uint NumFactors { get; set; }

		/// <summary>Learn rate (update step size)</summary>
		public float LearnRate { get; set; }

		/// <summary>Multiplicative learn rate decay</summary>
		/// <remarks>Applied after each epoch (= pass over the whole dataset)</remarks>
		public float Decay { get; set; }

		/// <summary>Regularization parameter</summary>
		public virtual float Regularization { get; set; }

		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get; set; }

		/// <summary>The learn rate used for the current epoch</summary>
		protected internal float current_learnrate;

		/// <summary>Default constructor</summary>
		public MatrixFactorization() : base()
		{
			// set default values
			Regularization = 0.015f;
			LearnRate = 0.01f;
			Decay = 1.0f;
			NumIter = 30;
			InitStdDev = 0.1;
			NumFactors = 10;
		}

		/// <summary>Initialize the model data structure</summary>
		protected internal virtual void InitModel()
		{
			// init factor matrices
			user_factors = new Matrix<float>(MaxUserID + 1, NumFactors);
			item_factors = new Matrix<float>(MaxItemID + 1, NumFactors);
			user_factors.InitNormal(InitMean, InitStdDev);
			item_factors.InitNormal(InitMean, InitStdDev);

			// set factors to zero for users and items without training examples
			for (int u = 0; u < ratings.CountByUser.Count; u++)
				if (ratings.CountByUser[u] == 0)
					user_factors.SetRowToOneValue(u, 0);
			for (int i = 0; i < ratings.CountByItem.Count; i++)
				if (ratings.CountByItem[i] == 0)
					item_factors.SetRowToOneValue(i, 0);

			current_learnrate = LearnRate;
		}

		///
		public override void Train()
		{
			InitModel();

			// learn model parameters
			global_bias = ratings.Average;
			LearnFactors(ratings.RandomIndex, true, true);
		}

		/// <summary>Updates <see cref="current_learnrate"/> after each epoch</summary>
		protected virtual void UpdateLearnRate()
		{
			current_learnrate *= Decay;
		}

		///
		public virtual void Iterate()
		{
			Iterate(ratings.RandomIndex, true, true);
		}

		/// <summary>Updates the latent factors on a user</summary>
		/// <param name="user_id">the user ID</param>
		public virtual void RetrainUser(int user_id)
		{
			if (UpdateUsers)
			{
				user_factors.RowInitNormal(user_id, InitMean, InitStdDev);
				LearnFactors(ratings.ByUser[user_id], true, false);
			}
		}

		/// <summary>Updates the latent factors of an item</summary>
		/// <param name="item_id">the item ID</param>
		public virtual void RetrainItem(int item_id)
		{
			if (UpdateItems)
			{
				item_factors.RowInitNormal(item_id, InitMean, InitStdDev);
				LearnFactors(ratings.ByItem[item_id], false, true);
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

				float err = ratings[index] - Predict(u, i, false);

				// adjust factors
				for (int f = 0; f < NumFactors; f++)
				{
					float u_f = user_factors[u, f];
					float i_f = item_factors[i, f];

					// if necessary, compute and apply updates
					if (update_user)
					{
						double delta_u = err * i_f - Regularization * u_f;
						user_factors.Inc(u, f, current_learnrate * delta_u);
					}
					if (update_item)
					{
						double delta_i = err * u_f - Regularization * i_f;
						item_factors.Inc(i, f, current_learnrate * delta_i);
					}
				}
			}

			UpdateLearnRate();
		}

		private void LearnFactors(IList<int> rating_indices, bool update_user, bool update_item)
		{
			for (uint current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate(rating_indices, update_user, update_item);
		}

		///
		protected float Predict(int user_id, int item_id, bool bound)
		{
			float result = global_bias + DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			if (bound)
			{
				if (result > MaxRating)
					return MaxRating;
				if (result < MinRating)
					return MinRating;
			}
			return result;
		}

		/// <summary>Predict rating for a fold-in user and an item</summary>
		/// <param name='user_vector'>a float vector representing the user</param>
		/// <param name='item_id'>the item ID</param>
		/// <returns>the predicted rating</returns>
		protected virtual float Predict(float[] user_vector, int item_id)
		{
			return Predict(user_vector, item_id, true);
		}

		///
		float Predict(float[] user_vector, int item_id, bool bound)
		{
			float result = global_bias + DataType.MatrixExtensions.RowScalarProduct(item_factors, item_id, user_vector);

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
		public override float Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1)
				return global_bias;
			if (item_id >= item_factors.dim1)
				return global_bias;

			return Predict(user_id, item_id, true);
		}

		///
		public override void AddRatings(IRatings ratings)
		{
			base.AddRatings(ratings);
			foreach (int user_id in ratings.AllUsers)
				RetrainUser(user_id);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void UpdateRatings(IRatings ratings)
		{
			base.UpdateRatings(ratings);
			foreach (int user_id in ratings.AllUsers)
				RetrainUser(user_id);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void RemoveRatings(IDataSet ratings)
		{
			base.RemoveRatings(ratings);
			foreach (int user_id in ratings.AllUsers)
				RetrainUser(user_id);
			foreach (int item_id in ratings.AllItems)
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

		/// <summary>Compute parameters (latent factors) for a user represented by ratings</summary>
		/// <returns>a vector of latent factors</returns>
		/// <param name='rated_items'>a list of (item ID, rating value) pairs</param>
		protected virtual float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			var user_vector = new float[NumFactors];
			user_vector.InitNormal(InitMean, InitStdDev);
			rated_items.Shuffle();
			double lr = LearnRate;
			for (uint it = 0; it < NumIter; it++)
			{
				for (int index = 0; index < rated_items.Count; index++)
				{
					int item_id = rated_items[index].Item1;
					float err = rated_items[index].Item2 - Predict(user_vector, item_id, false);

					// adjust factors
					for (int f = 0; f < NumFactors; f++)
					{
						float u_f = user_vector[f];
						float i_f = item_factors[item_id, f];

						double delta_u = err * i_f - Regularization * u_f;
						user_vector[f] += (float) (lr * delta_u);
					}
				}
				lr *= Decay;
			}
			return user_vector;
		}

		///
		public IList<Tuple<int, float>> ScoreItems(IList<Tuple<int, float>> rated_items, IList<int> candidate_items)
		{
			var user_vector = FoldIn(rated_items);

			// score the items
			var result = new Tuple<int, float>[candidate_items.Count];
			for (int i = 0; i < candidate_items.Count; i++)
			{
				int item_id = candidate_items[i];
				result[i] = Tuple.Create(item_id, Predict(user_vector, item_id));
			}
			return result;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.99") )
			{
				writer.WriteLine(global_bias.ToString(CultureInfo.InvariantCulture));
				writer.WriteMatrix(user_factors);
				writer.WriteMatrix(item_factors);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var bias = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

				var user_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
				var item_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

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
					Console.Error.WriteLine("Set NumFactors to {0}", user_factors.NumberOfColumns);
					this.NumFactors = (uint) user_factors.NumberOfColumns;
				}
				this.user_factors = user_factors;
				this.item_factors = item_factors;
			}
		}

		/// <summary>Compute the regularized loss</summary>
		/// <returns>the regularized loss</returns>
		public virtual float ComputeObjective()
		{
			double objective = Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings);

			for (int u = 0; u <= MaxUserID; u++)
				objective += ratings.CountByUser[u] * Regularization * Math.Pow(user_factors.GetRow(u).EuclideanNorm(), 2);

			for (int i = 0; i <= MaxItemID; i++)
				objective += ratings.CountByItem[i] * Regularization * Math.Pow(item_factors.GetRow(i).EuclideanNorm(), 2);

			return (float) objective;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} regularization={2} learn_rate={3} learn_rate_decay={4} num_iter={5}",
				this.GetType().Name, NumFactors, Regularization, LearnRate, Decay, NumIter);
		}
	}
}