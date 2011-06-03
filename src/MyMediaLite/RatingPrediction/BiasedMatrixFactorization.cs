// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.Taxonomy;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Matrix factorization engine with explicit user and item bias</summary>
	public class BiasedMatrixFactorization : MatrixFactorization
	{
		/// <summary>regularization constant for the bias terms</summary>
		public double BiasReg { get; set; }

		/// <summary>regularization constant for the user factors</summary>
		public double RegU { get; set; }

		/// <summary>regularization constant for the user factors</summary>
		public double RegI { get; set; }

		///
		public override double Regularization
		{
			set {
				base.Regularization = value;
				RegU = value;
				RegI = value;
			}
		}

		/// <summary>Use bold driver heuristics for learning rate adaption</summary>
		/// <remarks>
		/// See
		/// Rainer Gemulla, Peter J. Haas, Erik Nijkamp, Yannis Sismanis:
		/// Large-Scale Matrix Factorization with Distributed Stochastic Gradient Descent
		/// 2011
		/// </remarks>
		public bool BoldDriver { set; get; }

		/// <summary>Loss for the last iteration, used by bold driver heuristics</summary>
		double last_loss = double.NegativeInfinity;

		/// <summary>the user biases</summary>
		protected double[] user_bias;
		/// <summary>the item biases</summary>
		protected double[] item_bias;

		/// <summary>Default constructor</summary>
		public BiasedMatrixFactorization()
		{
			BiasReg = 0.0001;
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			user_bias = new double[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				user_bias[u] = 0;
			item_bias = new double[MaxItemID + 1];
			for (int i = 0; i <= MaxItemID; i++)
				item_bias[i] = 0;

			if (BoldDriver)
				last_loss = ComputeLoss();
		}

		///
		public override void Train()
		{
			InitModel();

			// compute global average
			global_bias = ratings.Average;

			for (int current_iter = 0; current_iter < NumIter; current_iter++)
				Iterate();
		}

		///
		public override void Iterate()
		{
			base.Iterate();

			if (BoldDriver)
			{
				double loss = ComputeLoss();

				if (loss > last_loss)
					LearnRate *= 0.5;
				else if (loss < last_loss)
					LearnRate *= 1.05;

				last_loss = loss;

				var ni = new NumberFormatInfo();
				ni.NumberDecimalDigits = '.';
				Console.Error.WriteLine(string.Format(ni, "loss {0} learn_rate {1} ", loss, LearnRate));
			}
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			double rating_range_size = MaxRating - MinRating;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double dot_product = user_bias[u] + item_bias[i] + MatrixUtils.RowScalarProduct(user_factors, u, item_factors, i);
				double sig_dot = 1 / (1 + Math.Exp(-dot_product));

				double p = MinRating + sig_dot * rating_range_size;
				double err = ratings[index] - p;

				double gradient_common = err * sig_dot * (1 - sig_dot) * rating_range_size;

				// adjust biases
				if (update_user)
					user_bias[u] += LearnRate * (gradient_common - BiasReg * user_bias[u]);
				if (update_item)
					item_bias[i] += LearnRate * (gradient_common - BiasReg * item_bias[i]);

				// adjust latent factors
				for (int f = 0; f < NumFactors; f++)
				{
				 	double u_f = user_factors[u, f];
					double i_f = item_factors[i, f];

					if (update_user)
					{
						double delta_u = gradient_common * i_f - RegU * u_f;
						MatrixUtils.Inc(user_factors, u, f, LearnRate * delta_u);
						// this is faster (190 vs. 260 seconds per iteration on Netflix w/ k=30) than
						//    user_factors[u, f] += learn_rate * delta_u;
					}
					if (update_item)
					{
						double delta_i = gradient_common * u_f - RegI * i_f;
						MatrixUtils.Inc(item_factors, i, f, LearnRate * delta_i);
					}
				}
			}
		}

		///
		public override double Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1 || item_id >= item_factors.dim1)
				return global_bias;

			double score = user_bias[user_id] + item_bias[item_id] + MatrixUtils.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			return MinRating + ( 1 / (1 + Math.Exp(-score)) ) * (MaxRating - MinRating);
		}

		///
		public override void SaveModel(string filename)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
			{
				writer.WriteLine(global_bias.ToString(ni));
				VectorUtils.WriteVector(writer, user_bias);
				IMatrixUtils.WriteMatrix(writer, user_factors);
				VectorUtils.WriteVector(writer, item_bias);
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

				IList<double> user_bias = VectorUtils.ReadVector(reader);
				var user_factors = (Matrix<double>) IMatrixUtils.ReadMatrix(reader, new Matrix<double>(0, 0));
				IList<double> item_bias = VectorUtils.ReadVector(reader);
				var item_factors = (Matrix<double>) IMatrixUtils.ReadMatrix(reader, new Matrix<double>(0, 0));

				if (user_factors.dim2 != item_factors.dim2)
					throw new IOException(
								  string.Format(
									  "Number of user and item factors must match: {0} != {1}",
									  user_factors.dim2, item_factors.dim2));
				if (user_bias.Count != user_factors.dim1)
					throw new IOException(
								  string.Format(
									  "Number of users must be the same for biases and factors: {0} != {1}",
									  user_bias.Count, user_factors.dim1));
				if (item_bias.Count != item_factors.dim1)
					throw new IOException(
								  string.Format(
									  "Number of items must be the same for biases and factors: {0} != {1}",
									  item_bias.Count, item_factors.dim1));

				this.MaxUserID = user_factors.dim1 - 1;
				this.MaxItemID = item_factors.dim1 - 1;

				// assign new model
				this.global_bias = bias;
				if (this.NumFactors != user_factors.dim2)
				{
					Console.Error.WriteLine("Set num_factors to {0}", user_factors.dim1);
					this.NumFactors = user_factors.dim2;
				}
				this.user_factors = user_factors;
				this.item_factors = item_factors;
				this.user_bias = new double[user_factors.dim1];
				user_bias.CopyTo(this.user_bias, 0);
				this.item_bias = new double[item_factors.dim1];
				item_bias.CopyTo(this.item_bias, 0);
			}
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);

			// create new user bias array
			double[] user_bias = new double[user_id + 1];
			Array.Copy(this.user_bias, user_bias, this.user_bias.Length);
			this.user_bias = user_bias;
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);

			// create new item bias array
			double[] item_bias = new double[item_id + 1];
			Array.Copy(this.item_bias, item_bias, this.item_bias.Length);
			this.item_bias = item_bias;
		}

		///
		public override void RetrainUser(int user_id)
		{
			user_bias[user_id] = 0;
			base.RetrainUser(user_id);
		}

		///
		public override void RetrainItem(int item_id)
		{
			item_bias[item_id] = 0;
			base.RetrainItem(item_id);
		}

		///
		public override void RemoveUser(int user_id)
		{
			base.RemoveUser(user_id);

			user_bias[user_id] = 0;
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			item_bias[item_id] = 0;
		}

		///
		public override double ComputeLoss()
		{
			double square_loss = 0;
			for (int i = 0; i < ratings.Count; i++)
			{
				int user_id = ratings.Users[i];
				int item_id = ratings.Items[i];
				square_loss += Math.Pow(Predict(user_id, item_id) - ratings[i], 2);
			}

			double complexity = 0;
			for (int u = 0; u <= MaxUserID; u++)
			{
				complexity += ratings.CountByUser[u] * RegU * Math.Pow(VectorUtils.EuclideanNorm(user_factors.GetRow(u)), 2);
				complexity += ratings.CountByUser[u] * BiasReg * Math.Pow(user_bias[u], 2);
			}
			for (int i = 0; i <= MaxItemID; i++)
			{
				complexity += ratings.CountByItem[i] * RegI * Math.Pow(VectorUtils.EuclideanNorm(item_factors.GetRow(i)), 2);
				complexity += ratings.CountByItem[i] * BiasReg * Math.Pow(item_bias[i], 2);
			}

			return square_loss + complexity;
		}

		///
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni,
								 "BiasedMatrixFactorization num_factors={0} bias_reg={1} reg_u={2} reg_i={3} learn_rate={4} num_iter={5} bold_driver={6} init_mean={7} init_stdev={8}",
								 NumFactors, BiasReg, RegU, RegI, LearnRate, NumIter, BoldDriver, InitMean, InitStdev);
		}
	}
}