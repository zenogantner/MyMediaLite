// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;
using MyMediaLite.IO;

// TODO finish implementation

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Constrained probabilistic matrix factorization</summary>
	/// <remarks>
	/// Training use batch gradient descent.
	///
	/// Literature:
	/// <list type="bullet">
    ///   <item><description>
	///     Ruslan Salakhutdinov, Andriy Mnih:
	///     Probabilistic Matrix Factorization.
	///     NIPS 2008.
	///     http://www.mit.edu/~rsalakhu/papers/nips07_pmf.pdf
	///   </description></item>
	/// </list>
	///
	/// This recommender does NOT (yet) support incremental updates.
	/// </remarks>
	public class ConstrainedProbabilisticMatrixFactorization : MatrixFactorization
	{
		/// <summary>regularization constant for the user factors</summary>
		public float RegY { get; set; }

		/// <summary>regularization constant for the item factors</summary>
		public float RegV { get; set; }

		/// <summary>regularization constant for the latent similarity constraint matrix</summary>
		public float RegW { get; set; }

		///
		public override float Regularization
		{
			set {
				base.Regularization = value;
				RegY = value;
				RegV = value;
				RegW = value;
			}
		}

		/// <summary>Use bold driver heuristics for learning rate adaption</summary>
		/// <remarks>
		/// See
		/// Rainer Gemulla, Peter J. Haas, Erik Nijkamp, Yannis Sismanis:
		/// Large-Scale Matrix Factorization with Distributed Stochastic Gradient Descent,
		/// 2011
		/// </remarks>
		public bool BoldDriver { set; get; }

		/// <summary>Loss for the last iteration, used by bold driver heuristics</summary>
		//double last_loss = double.NegativeInfinity;

		Matrix<float> similarity_constraint_matrix;

		/// <summary>Default constructor</summary>
		public ConstrainedProbabilisticMatrixFactorization()
		{
			Regularization = 0.004f;
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			similarity_constraint_matrix = new Matrix<float>(NumFactors, MaxItemID + 1);

			//if (BoldDriver)
			//	last_loss = ComputeLoss();
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

			/*
			if (BoldDriver)
			{
				double loss = ComputeLoss();

				if (loss > last_loss)
					LearnRate *= 0.5;
				else if (loss < last_loss)
					LearnRate *= 1.05;

				last_loss = loss;

				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loss {0} learn_rate {1} ", loss, LearnRate));
			}
			*/
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			IterateRMSE(rating_indices, update_user, update_item);
		}

		void IterateRMSE(IList<int> rating_indices, bool update_user, bool update_item)
		{
			// TODO
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id >= user_factors.dim1 || item_id >= item_factors.dim1)
				return global_bias;

			// TODO
			double score = MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			return (float) (MinRating + ( 1 / (1 + Math.Exp(-score)) ) * (MaxRating - MinRating));
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.04") )
			{
				writer.WriteLine(global_bias.ToString(CultureInfo.InvariantCulture));
				IMatrixExtensions.WriteMatrix(writer, user_factors);
				IMatrixExtensions.WriteMatrix(writer, item_factors);
				IMatrixExtensions.WriteMatrix(writer, similarity_constraint_matrix);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var bias = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

				var user_factors                 = (Matrix<float>) IMatrixExtensions.ReadMatrix(reader, new Matrix<float>(0, 0));
				var item_factors                 = (Matrix<float>) IMatrixExtensions.ReadMatrix(reader, new Matrix<float>(0, 0));
				var similarity_constraint_matrix = (Matrix<float>) IMatrixExtensions.ReadMatrix(reader, new Matrix<float>(0, 0));

				if (user_factors.dim2 != item_factors.dim2)
					throw new IOException(
								string.Format(
									"Number of user and item factors must match: {0} != {1}",
									user_factors.dim2, item_factors.dim2));
				if (similarity_constraint_matrix.dim2 != item_factors.dim1)
					throw new IOException(
								  string.Format(
									  "Number of items must match: {0} != {1}",
									  similarity_constraint_matrix.dim2, item_factors.dim1));
				if (similarity_constraint_matrix.dim1 != item_factors.dim2)
					throw new IOException(
								  string.Format(
									  "Number of factors must match: {0} != {1}",
									  similarity_constraint_matrix.dim1, item_factors.dim2));

				this.MaxUserID = user_factors.dim1 - 1;
				this.MaxItemID = item_factors.dim1 - 1;

				// assign new model
				this.global_bias = bias;
				if (this.NumFactors != user_factors.dim2)
				{
					Console.Error.WriteLine("Set num_factors to {0}", user_factors.dim2);
					this.NumFactors = (uint) user_factors.dim2;
				}
				this.user_factors                 = user_factors;
				this.item_factors                 = item_factors;
				this.similarity_constraint_matrix = similarity_constraint_matrix;
			}
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);

			// TODO enhance similarity matrix
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			// TODO enhance similarity matrix
		}

		///
		public override double ComputeLoss()
		{
			// TODO
			return -1;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} reg_y={2} reg_v={3} reg_w={4} learn_rate={5} num_iter={6} bold_driver={7} init_mean={8} init_stddev={9}",
				this.GetType().Name, NumFactors, RegY, RegV, RegW, LearnRate, NumIter, BoldDriver, InitMean, InitStdDev);
		}
	}
}