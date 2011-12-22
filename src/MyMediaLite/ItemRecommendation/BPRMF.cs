// Copyright (C) 2010 Zeno Gantner, Christoph Freudenthaler
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
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Matrix factorization model for item prediction (ranking) optimized for BPR </summary>
	/// <remarks>
	/// BPR reduces ranking to pairwise classification.
	///
	/// Literature:
	/// <list type="bullet">
    ///   <item><description>
	///     Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, Lars Schmidt-Thieme:
	///     BPR: Bayesian Personalized Ranking from Implicit Feedback.
	///     UAI 2009.
	///     http://www.ismll.uni-hildesheim.de/pub/pdfs/Rendle_et_al2009-Bayesian_Personalized_Ranking.pdf
	///   </description></item>
	/// </list>
	///
	/// Different sampling strategies are configurable by setting the UniformUserSampling and WithReplacement accordingly.
	/// To get the strategy from the original paper, set UniformUserSampling=false and WithReplacement=false.
	/// WithReplacement=true (default) gives you usually a slightly faster convergence, and UniformUserSampling=true (default)
	/// (approximately) optimizes the average AUC over all users.
	///
	/// This recommender supports incremental updates.
	/// </remarks>
	public class BPRMF : MF
	{
		/// <summary>Fast, but memory-intensive sampling</summary>
		protected bool fast_sampling = false;

		/// <summary>Item bias terms</summary>
		protected double[] item_bias;

		/// <summary>Fast sampling memory limit, in MiB</summary>
		public int FastSamplingMemoryLimit { get { return fast_sampling_memory_limit; }	set { fast_sampling_memory_limit = value; } }
		/// <summary>Fast sampling memory limit, in MiB</summary>
		protected int fast_sampling_memory_limit = 1024;

		/// <summary>Sample positive observations with (true) or without (false) replacement</summary>
		public bool WithReplacement { get; set; }

		/// <summary>Sample uniformly from users</summary>
		public bool UniformUserSampling { get; set; }

		/// <summary>Regularization parameter for the bias term</summary>
		public double BiasReg { get; set; }

		/// <summary>Learning rate alpha</summary>
		public double LearnRate { get {	return learn_rate; } set { learn_rate = value; } }
		/// <summary>Learning rate alpha</summary>
		protected double learn_rate = 0.05;

		/// <summary>Regularization parameter for user factors</summary>
		public double RegU { get { return reg_u; } set { reg_u = value; } }
		/// <summary>Regularization parameter for user factors</summary>
		protected double reg_u = 0.0025;

		/// <summary>Regularization parameter for positive item factors</summary>
		public double RegI { get { return reg_i; } set { reg_i = value;	} }
		/// <summary>Regularization parameter for positive item factors</summary>
		protected double reg_i = 0.0025;

		/// <summary>Regularization parameter for negative item factors</summary>
		public double RegJ { get { return reg_j; } set { reg_j = value; } }
		/// <summary>Regularization parameter for negative item factors</summary>
		protected double reg_j = 0.00025;

		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		public bool UpdateJ { get { return update_j; } set { update_j = value; } }
		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		protected bool update_j = true;

		/// <summary>support data structure for fast sampling</summary>
		protected IList<IList<int>> user_pos_items;
		/// <summary>support data structure for fast sampling</summary>
		protected IList<IList<int>> user_neg_items;

		/// <summary>Use bold driver heuristics for learning rate adaption</summary>
		/// <remarks>
		/// Does not work too well for BPR-MF.
		///
		/// Literature:
		/// <list type="bullet">
		///   <item><description>
		///     Rainer Gemulla, Peter J. Haas, Erik Nijkamp, Yannis Sismanis:
		///     Large-Scale Matrix Factorization with Distributed Stochastic Gradient Descent.
		///     KDD 2011.
		///     http://www.mpi-inf.mpg.de/~rgemulla/publications/gemulla11dsgd.pdf
		///   </description></item>
		/// </list>
		/// </remarks>
		public bool BoldDriver { set; get; }

		/// <summary>Loss for the last iteration, used by bold driver heuristics</summary>
		protected double last_loss = double.NegativeInfinity;

		/// <summary>array of user components of triples to use for approximate loss computation</summary>
		int[] loss_sample_u;
		/// <summary>array of positive item components of triples to use for approximate loss computation</summary>
		int[] loss_sample_i;
		/// <summary>array of negative item components of triples to use for approximate loss computation</summary>
		int[] loss_sample_j;

		/// <summary>Random number generator</summary>
		protected System.Random random;

		/// <summary>Default constructor</summary>
		public BPRMF()
		{
			UniformUserSampling = true;
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			item_bias = new double[MaxItemID + 1];
		}

		///
		public override void Train()
		{
			InitModel();

			CheckSampling();

			random = Util.Random.GetInstance();

			if (BoldDriver)
			{
				int num_sample_triples = (int) Math.Sqrt(MaxUserID) * 100;         // TODO make configurable
				Console.Error.WriteLine("loss_num_sample_triples={0}", num_sample_triples);

				// create the sample to estimate loss from
				loss_sample_u = new int[num_sample_triples];
				loss_sample_i = new int[num_sample_triples];
				loss_sample_j = new int[num_sample_triples];

				int u, i, j;
				for (int c = 0; c < num_sample_triples; c++)
				{
					SampleTriple(out u, out i, out j);
					loss_sample_u[c] = u;
					loss_sample_i[c] = i;
					loss_sample_j[c] = j;
				}

				last_loss = ComputeLoss();
			}

			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Perform one iteration of stochastic gradient ascent over the training data</summary>
		/// <remarks>
		/// One iteration is samples number of positive entries in the training matrix times
		/// </remarks>
		public override void Iterate()
		{
			int num_pos_events = Feedback.Count;

			int user_id, pos_item_id, neg_item_id;

			if (UniformUserSampling)
			{
				if (WithReplacement) // case 1: uniform user sampling, with replacement
				{
					var user_matrix = Feedback.GetUserMatrixCopy();

					for (int i = 0; i < num_pos_events; i++)
					{
						while (true) // sampling with replacement
						{
							user_id = SampleUser();
							var user_items = user_matrix[user_id];

							// reset user if already exhausted
							if (user_items.Count == 0)
								foreach (int item_id in Feedback.UserMatrix[user_id])
									user_matrix[user_id, item_id] = true;

							pos_item_id = user_items.ElementAt(random.Next(user_items.Count));
							user_matrix[user_id, pos_item_id] = false; // temporarily forget positive observation
							do
								neg_item_id = random.Next(MaxItemID + 1);
							while (Feedback.UserMatrix[user_id].Contains(neg_item_id));
							break;
						}
						UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
					}
				}
				else // case 2: uniform user sampling, without replacement
				{
					for (int i = 0; i < num_pos_events; i++)
					{
						SampleTriple(out user_id, out pos_item_id, out neg_item_id);
						UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
					}
				}
			}
			else
			{
				if (WithReplacement) // case 3: uniform pair sampling, with replacement
					for (int i = 0; i < num_pos_events; i++)
					{
						int index = random.Next(num_pos_events);
						user_id = Feedback.Users[index];
						pos_item_id = Feedback.Items[index];
						neg_item_id = -1;
						SampleOtherItem(user_id, pos_item_id, out neg_item_id);
						UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
					}
				else // case 4: uniform pair sampling, without replacement
					foreach (int index in Feedback.RandomIndex)
					{
						user_id = Feedback.Users[index];
						pos_item_id = Feedback.Items[index];
						neg_item_id = -1;
						SampleOtherItem(user_id, pos_item_id, out neg_item_id);
						UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
					}
			}

			if (BoldDriver)
			{
				double loss = ComputeLoss();

				if (loss > last_loss)
					LearnRate *= 0.5;
				else if (loss < last_loss)
					LearnRate *= 1.1;

				last_loss = loss;

				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loss {0} learn_rate {1} ", loss, LearnRate));
			}
		}

		/// <summary>Sample another item, given the first one and the user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the given item</param>
		/// <param name="j">the ID of the other item</param>
		/// <returns>true if the given item was already seen by user u</returns>
		protected virtual bool SampleOtherItem(int u, int i, out int j)
		{
			bool item_is_positive = Feedback.UserMatrix[u, i];

			if (fast_sampling)
			{
				if (item_is_positive)
				{
					int rindex = random.Next(user_neg_items[u].Count);
					j = user_neg_items[u][rindex];
				}
				else
				{
					int rindex = random.Next(user_pos_items[u].Count);
					j = user_pos_items[u][rindex];
				}
			}
			else
			{
				do
					j = random.Next(MaxItemID + 1);
				while (Feedback.UserMatrix[u, j] == item_is_positive);
			}

			return item_is_positive;
		}

		/// <summary>Sample a pair of items, given a user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		protected virtual void SampleItemPair(int u, out int i, out int j)
		{
			if (fast_sampling)
			{
				int rindex;

				rindex = random.Next(user_pos_items[u].Count);
				i = user_pos_items[u][rindex];

				rindex = random.Next(user_neg_items[u].Count);
				j = user_neg_items[u][rindex];
			}
			else
			{
				var user_items = Feedback.UserMatrix[u];
				i = user_items.ElementAt(random.Next(user_items.Count));
				do
					j = random.Next(MaxItemID + 1);
				while (user_items.Contains(j));
			}
		}

		/// <summary>Sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		protected virtual int SampleUser()
		{
			while (true)
			{
				int u = random.Next(MaxUserID + 1);
				var user_items = Feedback.UserMatrix[u];
				if (user_items.Count == 0 || user_items.Count == MaxItemID + 1)
					continue;
				return u;
			}
		}

		/// <summary>Sample a triple for BPR learning</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		protected virtual void SampleTriple(out int u, out int i, out int j)
		{
			u = SampleUser();
			SampleItemPair(u, out i, out j);
		}

		/// <summary>Update latent factors according to the stochastic gradient descent update rule</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		/// <param name="update_u">if true, update the user latent factors</param>
		/// <param name="update_i">if true, update the latent factors of the first item</param>
		/// <param name="update_j">if true, update the latent factors of the second item</param>
		protected virtual void UpdateFactors(int u, int i, int j, bool update_u, bool update_i, bool update_j)
		{
			double x_uij = item_bias[i] - item_bias[j] + MatrixExtensions.RowScalarProductWithRowDifference(user_factors, u, item_factors, i, item_factors, j);

			double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

			// adjust bias terms
			if (update_i)
			{
				double update = one_over_one_plus_ex - BiasReg * item_bias[i];
				item_bias[i] += learn_rate * update;
			}

			if (update_j)
			{
				double update = -one_over_one_plus_ex - BiasReg * item_bias[j];
				item_bias[j] += learn_rate * update;
			}

			// adjust factors
			for (int f = 0; f < num_factors; f++)
			{
				double w_uf = user_factors[u, f];
				double h_if = item_factors[i, f];
				double h_jf = item_factors[j, f];

				if (update_u)
				{
					double update = (h_if - h_jf) * one_over_one_plus_ex - reg_u * w_uf;
					user_factors[u, f] = w_uf + learn_rate * update;
				}

				if (update_i)
				{
					double update = w_uf * one_over_one_plus_ex - reg_i * h_if;
					item_factors[i, f] = h_if + learn_rate * update;
				}

				if (update_j)
				{
					double update = -w_uf  * one_over_one_plus_ex - reg_j * h_jf;
					item_factors[j, f] = h_jf + learn_rate * update;
				}
			}
		}

		///
		public override void AddFeedback(int user_id, int item_id)
		{
			base.AddFeedback(user_id, item_id);

			if (fast_sampling)
				CreateFastSamplingData(user_id);

			// retrain
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		///
		public override void RemoveFeedback(int user_id, int item_id)
		{
			base.RemoveFeedback(user_id, item_id);

			if (fast_sampling)
				CreateFastSamplingData(user_id);

			// retrain
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		///
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);

			user_factors.AddRows(user_id + 1);
			user_factors.RowInitNormal(user_id, InitMean, InitStdDev);
		}

		///
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);

			item_factors.AddRows(item_id + 1);
			item_factors.RowInitNormal(item_id, InitMean, InitStdDev);

			// create new item bias array
			var item_bias = new double[item_id + 1];
			Array.Copy(this.item_bias, item_bias, this.item_bias.Length);
			this.item_bias = item_bias;
		}

		///
		public override void RemoveUser(int user_id)
		{
			base.RemoveUser(user_id);

			if (fast_sampling)
			{
				user_pos_items[user_id] = null;
				user_neg_items[user_id] = null;
			}

			// set user latent factors to zero
			user_factors.SetRowToOneValue(user_id, 0);
		}

		///
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			if (fast_sampling)
			{
				for (int i = 0; i < user_pos_items.Count; i++)
					if (user_pos_items[i].Contains(item_id))
					{
						var new_pos_items = new int[user_pos_items.Count - 1];
						bool found = false;
						for (int j = 0; j < user_pos_items[i].Count; j++)
							if (user_pos_items[i][j] != item_id)
								new_pos_items[j - (found ? 1 : 0)] = user_pos_items[i][j];
							else
								found = true;
						user_pos_items[i] = new_pos_items;
					}

				for (int i = 0; i < user_neg_items.Count; i++)
					if (user_neg_items[i].Contains(item_id))
					{
						var new_neg_items = new int[user_neg_items.Count - 1];
						bool found = false;
						for (int j = 0; j < user_neg_items[i].Count; j++)
							if (user_neg_items[i][j] != item_id)
								new_neg_items[j - (found ? 1 : 0)] = user_neg_items[i][j];
							else
								found = true;
						user_neg_items[i] = new_neg_items;
					}
			}

			// set item latent factors to zero
			item_factors.SetRowToOneValue(item_id, 0);
		}

		/// <summary>Retrain the latent factors of a given user</summary>
		/// <param name="user_id">the user ID</param>
		protected virtual void RetrainUser(int user_id)
		{
			user_factors.RowInitNormal(user_id, InitMean, InitStdDev);

			var user_items = Feedback.UserMatrix[user_id];
			for (int i = 0; i < user_items.Count; i++)
			{
				int item_id_1, item_id_2;
				SampleItemPair(user_id, out item_id_1, out item_id_2);
				UpdateFactors(user_id, item_id_1, item_id_2, true, false, false);
			}
		}

		/// <summary>Retrain the latent factors of a given item</summary>
		/// <param name="item_id">the item ID</param>
		protected virtual void RetrainItem(int item_id)
		{
			item_factors.RowInitNormal(item_id, InitMean, InitStdDev);

			int num_pos_events = Feedback.UserMatrix.NumberOfEntries;
			int num_item_iterations = num_pos_events  / (MaxItemID + 1);
			for (int i = 0; i < num_item_iterations; i++) {
				// remark: the item may be updated more or less frequently than in the normal from-scratch training
				int user_id = SampleUser();
				int other_item_id;
				bool item_is_positive = SampleOtherItem(user_id, item_id, out other_item_id);

				if (item_is_positive)
					UpdateFactors(user_id, item_id, other_item_id, false, true, false);
				else
					UpdateFactors(user_id, other_item_id, item_id, false, false, true);
			}
		}

		///
		public override double ComputeLoss()
		{
			double ranking_loss = 0;
			for (int c = 0; c < loss_sample_u.Length; c++)
			{
				double x_uij = Predict(loss_sample_u[c], loss_sample_i[c]) - Predict(loss_sample_u[c], loss_sample_j[c]);
				ranking_loss += 1 / (1 + Math.Exp(x_uij));
			}

			double complexity = 0;
			for (int c = 0; c < loss_sample_u.Length; c++)
			{
				complexity += RegU * Math.Pow(user_factors.GetRow(loss_sample_u[c]).EuclideanNorm(), 2);
				complexity += RegI * Math.Pow(item_factors.GetRow(loss_sample_i[c]).EuclideanNorm(), 2);
				complexity += RegJ * Math.Pow(item_factors.GetRow(loss_sample_j[c]).EuclideanNorm(), 2);
				complexity += BiasReg * Math.Pow(item_bias[loss_sample_i[c]], 2);
				complexity += BiasReg * Math.Pow(item_bias[loss_sample_j[c]], 2);
			}

			return ranking_loss + 0.5 * complexity;
		}

		private void CreateFastSamplingData(int u)
		{
			while (u >= user_pos_items.Count)
				user_pos_items.Add(null);
			while (u >= user_neg_items.Count)
				user_neg_items.Add(null);

			user_pos_items[u] = Feedback.UserMatrix[u].ToArray();
			var neg_list = new List<int>();
			for (int i = 0; i < MaxItemID; i++)
				if (! Feedback.UserMatrix[u, i])
					neg_list.Add(i);
			user_neg_items[u] = neg_list.ToArray();
		}

		///
		protected void CheckSampling()
		{
			try
			{
				checked
				{
					int fast_sampling_memory_size = ((MaxUserID + 1) * (MaxItemID + 1) * 4) / (1024 * 1024);
					Console.Error.WriteLine("fast_sampling_memory_size=" + fast_sampling_memory_size);

					if (fast_sampling_memory_size <= fast_sampling_memory_limit)
					{
						fast_sampling = true;

						this.user_pos_items = new List<IList<int>>(MaxUserID + 1);
						this.user_neg_items = new List<IList<int>>(MaxUserID + 1);
						for (int u = 0; u < MaxUserID + 1; u++)
							CreateFastSamplingData(u);
					}
				}
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("fast_sampling_memory_size=TOO_MUCH");
				// do nothing - don't use fast sampling
			}
		}

		///
		public override double Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return double.MinValue;

			return item_bias[item_id] + MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);
		}

		///
		public override void SaveModel(string file)
		{
			using ( StreamWriter writer = Model.GetWriter(file, this.GetType(), "2.03") )
			{
				writer.WriteMatrix(user_factors);
				writer.WriteVector(item_bias);
				writer.WriteMatrix(item_factors);
			}
		}

		///
		public override void LoadModel(string file)
		{
			using ( StreamReader reader = Model.GetReader(file, this.GetType()) )
			{
				var user_factors = (Matrix<double>) reader.ReadMatrix(new Matrix<double>(0, 0));
				var item_bias = reader.ReadVector();
				var item_factors = (Matrix<double>) reader.ReadMatrix(new Matrix<double>(0, 0));

				if (user_factors.NumberOfColumns != item_factors.NumberOfColumns)
					throw new IOException(
						string.Format(
							"Number of user and item factors must match: {0} != {1}",
							user_factors.NumberOfColumns, item_factors.NumberOfColumns));
				if (item_bias.Count != item_factors.dim1)
					throw new IOException(
						string.Format(
							"Number of items must be the same for biases and factors: {0} != {1}",
							item_bias.Count, item_factors.dim1));

				this.MaxUserID = user_factors.NumberOfRows - 1;
				this.MaxItemID = item_factors.NumberOfRows - 1;

				// assign new model
				if (this.num_factors != user_factors.NumberOfColumns)
				{
					Console.Error.WriteLine("Set num_factors to {0}", user_factors.NumberOfColumns);
					this.num_factors = user_factors.NumberOfColumns;
				}
				this.user_factors = user_factors;
				this.item_bias    = (double[]) item_bias;
				this.item_factors = item_factors;
			}
			random = Util.Random.GetInstance();
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} reg_j={5} num_iter={6} learn_rate={7} uniform_user_sampling={8} with_replacement={9}, bold_driver={10} fast_sampling_memory_limit={11} update_j={12} init_mean={13} init_stddev={14}",
				this.GetType().Name, num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, learn_rate, UniformUserSampling, WithReplacement, BoldDriver, fast_sampling_memory_limit, UpdateJ, InitMean, InitStdDev);
		}
	}
}
