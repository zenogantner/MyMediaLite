// Copyright (C) 2012 Lucas Drumond
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
using System.Threading;
using System.Threading.Tasks;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Sparse Linear Methods (SLIM) for item prediction (ranking) optimized for the elastic net loss  </summary>
	/// <remarks>
	///   <para>
	///     The model is learned using a coordinate descent algorithm with soft thresholding
	///     (Friedman et al. 2010).
	///   </para>
	///   <para>
	///     Literature:
	///   <list type="bullet">
	///     <item><description>
	///       X. Ning, G. Karypis: Slim: Sparse linear methods for top-n recommender systems.
	///      ICDM 2011.
	///      http://glaros.dtc.umn.edu/gkhome/fetch/papers/SLIM2011icdm.pdf
	///     </description></item>
	///     <item><description>
	///       J. Friedman, T. Hastie, R. Tibshirani: Regularization Paths for Generalized Linear Models via Coordinate Descent.
	///      Journal of Statistical Software 2010.
	///      http://www.jstatsoft.org/v33/i01/paper
	///     </description></item>
	///    </list>
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class LeastSquareSLIM : SLIM
	{
		/// <summary>Regularization parameter for the L1 regularization term (lambda in the original paper)</summary>
		public double RegL1 { get { return reg_l1; } set { reg_l1 = value; } }
		/// <summary>Regularization parameter for the L1 regularization term (lambda in the original paper)</summary>
		protected double reg_l1 = 0.01;

		/// <summary>Regularization parameter for the L2 regularization term (beta/2 in the original paper)</summary>
		public double RegL2 { get { return reg_l2; } set { reg_l2 = value; } }
		/// <summary>Regularization parameter for the L2 regularization term (beta/2 in the original paper)</summary>
		protected double reg_l2 = 0.001;

		/// <summary>How many neighbors to use in the kNN feature selection</summary>
		public uint K { get { return neighbors; } set { neighbors = value; } }
		/// <summary>How many neighbors to use in the kNN feature selection</summary>
		protected uint neighbors = 50;

		/// <summary>Default constructor</summary>
		public LeastSquareSLIM() : base() { }

		///
		protected override void InitModel()
		{
			base.InitModel();

			if (K > 0)
			{
				itemKNN = new ItemKNN();
				itemKNN.K = K;
				itemKNN.Correlation = MyMediaLite.Correlation.BinaryCorrelationType.Cosine;
				itemKNN.Feedback = this.Feedback;
			}
		}

		///
		public override void Train()
		{
			InitModel();

			if (K > 0)
				itemKNN.Train();

			// Since the item parameter vectors are learned independently from one another,
			// their learning can be done in parallel
			Parallel.For(0, MaxItemID + 1, item_id => {
				Train(item_id);
			});
		}

		/// <summary>Learns the set of parameters for a given item</summary>
		public void Train(int item_id)
		{
			for (int iter = 0; iter < NumIter; iter++)
				Iterate(item_id);
		}

		IList<int> GetMostSimilarItems(int item_id)
		{
			return itemKNN.GetMostSimilarItems(item_id, itemKNN.K);
		}

		/// <summary>Perform one iteration of coordinate descent for a given set of item parameters over the training data</summary>
		public void Iterate(int item_id)
		{
			if (K > 0)
			{
				foreach (int neighbor in GetMostSimilarItems(item_id))
					if (neighbor != item_id)
						UpdateParameters(item_id, neighbor);
			}
			else
			{
				for (int feat = 0; feat <= MaxItemID; feat++)
					if (feat != item_id)
						UpdateParameters(item_id, feat);
			}
		}

		/// <summary>
		/// Iterate this instance.
		/// </summary>
		public override void Iterate()
		{
			Parallel.For(0, MaxItemID + 1, item_id => {
				Iterate(item_id);
			});
		}

		/// <summary>Update item parameters according to the coordinate descent update rule</summary>
		/// <param name="item_id">the ID of the first item</param>
		/// <param name="other_item_id">the ID of the second item</param>
		protected virtual void UpdateParameters(int item_id, int other_item_id)
		{
			var item_users = Feedback.UserMatrix.GetEntriesByColumn(other_item_id);

			double gradient_sum = 0;

			foreach (int u in item_users)
			{
				if (Feedback.UserMatrix[u].Contains(item_id))
					gradient_sum += 1;

				gradient_sum -= Predict(u, item_id, other_item_id);
			}

			double gradient = gradient_sum / ((double) MaxUserID + 1.0);

			if (reg_l1 < Math.Abs(gradient))
			{
				if (gradient > 0)
				{
					double update = (gradient - reg_l1) / (1.0 + reg_l2);
					item_weights[item_id, other_item_id] = (float) update;
				}
				else
				{
					double update = (gradient + reg_l1) / (1.0 + reg_l2);
					item_weights[item_id, other_item_id] = (float) update;
				}
			}
			else
			{
				item_weights[item_id, other_item_id] = 0;
			}
		}

		/// <summary>
		/// Predict the specified user_id, item_id without taking exclude_item_id
		/// into consideration. This is needed for the coordinate descent update rule (equation 5 from
		/// Friedman et al. (2010)).
		/// </summary>
		/// <param name='user_id'>
		/// User_id.
		/// </param>
		/// <param name='item_id'>
		/// Item_id.
		/// </param>
		/// <param name='exclude_item_id'>
		/// Current item ID which shouldn't .
		/// </param>
		public float Predict(int user_id, int item_id, int exclude_item_id)
		{
			var user_items = Feedback.UserMatrix.GetEntriesByRow(user_id);
			float prediction = 0;

			if (K > 0)
			{
				foreach (int neighbor in GetMostSimilarItems(item_id))
				{
					if (Feedback.ItemMatrix[neighbor, user_id] && neighbor != exclude_item_id)
						prediction += item_weights[item_id, neighbor];
				}
			}
			else
			{
				foreach (int other_item_id in user_items)
					if (other_item_id != exclude_item_id)
						prediction += item_weights[item_id, other_item_id];
			}

			return prediction;
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);

			item_weights.AddRows(item_id + 1);
			item_weights.RowInitNormal(item_id, InitMean, InitStdDev);
		}

		///
		public override void RemoveUser(int user_id)
		{
			base.RemoveUser(user_id);
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			// set item latent factors to zero
			item_weights.SetRowToOneValue(item_id, 0);
		}


		/// <summary>Retrain the latent factors of a given item</summary>
		/// <param name="item_id">the item ID</param>
		protected virtual void RetrainItem(int item_id)
		{
			item_weights.RowInitNormal(item_id, InitMean, InitStdDev);

			Train(item_id);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id >= item_weights.dim1)
				return float.MinValue;

			var user_items = Feedback.UserMatrix.GetEntriesByRow(user_id);
			float prediction = 0;

			if (K > 0)
			{
				foreach (int neighbor in itemKNN.GetMostSimilarItems(item_id))
				{
					if (Feedback.ItemMatrix[neighbor, user_id])
						prediction += item_weights[item_id, neighbor];
				}
			}
			else
			{
				foreach (int item in user_items)
					prediction += item_weights[item_id, item];
			}

			return prediction;
		}

		/// <summary>Compute the regularized loss (regularized squared error on training data)</summary>
		/// <returns>the objective</returns>
		public override float ComputeObjective()
		{
			return 0;
		}

		///
		public override void SaveModel(string filename)
		{
			if (K > 0)
				itemKNN.SaveModel(filename + "-knn");

			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "3.05") )
			{
				writer.WriteMatrix(item_weights);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			if (K > 0)
			{
				itemKNN = new ItemKNN();
				itemKNN.LoadModel(filename + "-knn");
			}

			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var item_weights = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				this.MaxItemID = item_weights.NumberOfRows - 1;
				this.item_weights = item_weights;
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} reg_l1={1} reg_l2={2} num_iter={3} K={4}",
				this.GetType().Name, reg_l1, reg_l2, NumIter, K);
		}
	}
}
