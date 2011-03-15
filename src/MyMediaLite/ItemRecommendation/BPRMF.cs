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
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Eval;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>
	/// Matrix factorization model for item prediction optimized using BPR-Opt;
	/// </summary>
	/// <remarks>
	/// <inproceedings>
	///   <author>Steffen Rendle</author>
	///   <author>Christoph Freudenthaler</author>
	///   <author>Zeno Gantner</author>
	///   <author>Lars Schmidt-Thieme</author>
	///   <title>BPR: Bayesian Personalized Ranking from Implicit Feedback</title>
	///   <booktitle>Proceedings of the 25th Conference on Uncertainty in Artificial Intelligence (UAI 2009)</booktitle>
	///   <location>Montreal, Canada</location>
	///   <year>2009</year>
	/// </inproceedings>
	/// </remarks>
	public class BPRMF : MF, IIterativeModel
	{
		/// <summary>Fast, but memory-intensive sampling</summary>
		protected bool fast_sampling = false;

		/// <summary>Fast sampling memory limit, in MiB</summary>
		public int FastSamplingMemoryLimit { get { return fast_sampling_memory_limit; }	set { fast_sampling_memory_limit = value; } }
		/// <summary>Fast sampling memory limit, in MiB</summary>
		protected int fast_sampling_memory_limit = 1024;

		/// <summary>Use the first item latent factor as a bias term if set to true</summary>
		public bool ItemBias { get { return item_bias; } set { item_bias = value; }	}
		/// <summary>Use the first item latent factor as a bias term if set to true</summary>
		protected bool item_bias = false;

		/// <summary>One iteration is <see cref="iteration_length"/> * number of entries in the training matrix</summary>
		public int IterationLength { get { return iteration_length; } set { iteration_length = value; }	}
		/// <summary>One iteration is <see cref="iteration_length"/> * number of entries in the training matrix</summary>
		protected int iteration_length = 5;

		/// <summary>Learning rate alpha</summary>
		public double LearnRate { get {	return learn_rate; } set { learn_rate = value; } }
		/// <summary>Learning rate alpha</summary>
		protected double learn_rate = 0.05;

		/// <summary>Regularization parameter for positive item factors</summary>
		public double RegI { get { return reg_i; } set { reg_i = value;	} }
		/// <summary>Regularization parameter for positive item factors</summary>
		protected double reg_i = 0.0025;

		/// <summary>Regularization parameter for negative item factors</summary>
		public double RegJ { get { return reg_j; } set { reg_j = value; } }
		/// <summary>Regularization parameter for negative item factors</summary>
		protected double reg_j = 0.00025;

		/// <summary>Regularization parameter for user factors</summary>
		public double RegU { get { return reg_u; } set { reg_u = value; } }
		/// <summary>Regularization parameter for user factors</summary>
		protected double reg_u = 0.0025;

		/// <summary>support data structure for fast sampling</summary>
		protected List<int[]> user_pos_items;
		/// <summary>support data structure for fast sampling</summary>
		protected List<int[]> user_neg_items;

		/// <summary>Random number generator</summary>
		protected System.Random random;

		/// <inheritdoc/>
		public override void Train()
		{
			random = Util.Random.GetInstance();
			CheckSampling();

			// if necessary, set the bias counterparts to 1
			if (item_bias)
				user_factors.SetColumnToOneValue(0, 1.0);

			base.Train();
		}

		/// <summary>Perform one iteration of stochastic gradient ascent over the training data</summary>
		/// <remarks>
		/// One iteration is <see cref="iteration_length"/> * number of entries in the training matrix
		/// </remarks>
		public override void Iterate()
		{
			int num_pos_events = Feedback.Count;

			for (int i = 0; i < num_pos_events * iteration_length; i++)
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateFactors(user_id, item_id_1, item_id_2, true, true, true);
			}
		}

		/// <summary>Sample another item, given the first one and the user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the given item</param>
		/// <param name="j">the ID of the other item</param>
		/// <returns>true if the given item was already seen by user u</returns>
		protected virtual bool SampleOtherItem(int u, int i, out int j)
		{
			HashSet<int> user_items = Feedback.UserMatrix[u];
			bool item_is_positive = user_items.Contains(i);

			if (fast_sampling)
			{
				if (item_is_positive)
				{
					int rindex = random.Next (0, user_neg_items[u].Length);
					j = user_neg_items[u][rindex];
				}
				else
				{
					int rindex = random.Next (0, user_pos_items[u].Length);
					j = user_pos_items[u][rindex];
				}
			}
			else
			{
				do
					j = random.Next (0, MaxItemID + 1);
				while (user_items.Contains(j) != item_is_positive);
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

				rindex = random.Next (0, user_pos_items[u].Length);
				i = user_pos_items[u][rindex];

				rindex = random.Next (0, user_neg_items[u].Length);
				j = user_neg_items[u][rindex];
			}
			else
			{
				HashSet<int> user_items = Feedback.UserMatrix[u];
				i = user_items.ElementAt(random.Next (0, user_items.Count));
				do
					j = random.Next (0, MaxItemID + 1);
				while (user_items.Contains(j));
			}
		}

		/// <summary>Sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		protected virtual int SampleUser()
		{
			while (true)
			{
				int u = random.Next(0, MaxUserID + 1);
				HashSet<int> user_items = Feedback.UserMatrix[u];
				if (user_items.Count == 0 || user_items.Count == MaxItemID + 1)
					continue;
				return u;
			}
		}

		/// <summary>Sample a triple for BPR learning</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		protected void SampleTriple(out int u, out int i, out int j)
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
			double x_uij = Predict(u, i) - Predict(u, j);

			int start_factor = 0;

			if (item_bias)
			{
				start_factor = 1; // leave out the first (index 0) factor later

				double w_uf = user_factors[u, 0];
				double h_if = item_factors[i, 0];
				double h_jf = item_factors[j, 0];

				if (update_i)
				{
					double if_update = w_uf / (1 + Math.Exp(x_uij)) - reg_i * h_if;
					item_factors[i, 0] = h_if + learn_rate * if_update;
				}

				if (update_j)
				{
					double jf_update = -w_uf / (1 + Math.Exp(x_uij)) - reg_j * h_jf;
					item_factors[j, 0] = h_jf + learn_rate * jf_update;
				}
			}

			for (int f = start_factor; f < num_factors; f++)
			{
				double w_uf = user_factors[u, f];
				double h_if = item_factors[i, f];
				double h_jf = item_factors[j, f];

				if (update_u)
				{
					double uf_update = (h_if - h_jf) / (1 + Math.Exp(x_uij)) - reg_u * w_uf;
					user_factors[u, f] = w_uf + learn_rate * uf_update;
				}

				if (update_i)
				{
					double if_update = w_uf / (1 + Math.Exp(x_uij)) - reg_i * h_if;
					item_factors[i, f] = h_if + learn_rate * if_update;
				}

				if (update_j)
				{
					double jf_update = -w_uf / (1 + Math.Exp(x_uij)) - reg_j * h_jf;
					item_factors[j, f] = h_jf + learn_rate * jf_update;
				}
			}
		}

		/// <inheritdoc/>
		public override void AddFeedback(int user_id, int item_id)
		{
			base.AddFeedback(user_id, item_id);

			if (fast_sampling)
				CreateFastSamplingData(user_id);

			// retrain
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		/// <inheritdoc/>
		public override void RemoveFeedback(int user_id, int item_id)
		{
			base.RemoveFeedback(user_id, item_id);

			if (fast_sampling)
				CreateFastSamplingData(user_id);

			// retrain
			RetrainUser(user_id);
			RetrainItem(item_id);
		}

		/// <inheritdoc/>
		public override void AddUser(int user_id)
		{
			if (user_id > MaxUserID)
			{
				user_factors.AddRows(user_id + 1);
				MatrixUtils.InitNormal(user_factors, InitMean, InitStdev, user_id);
			}

			base.AddUser(user_id);
		}

		/// <inheritdoc/>
		public override void AddItem(int item_id)
		{
			if (item_id > MaxItemID)
			{
				item_factors.AddRows(item_id + 1);
				MatrixUtils.InitNormal(item_factors, InitMean, InitStdev, item_id);
			}

			base.AddItem(item_id);
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			// TODO remove from fast sampling data structures
			//      (however: not needed if all feedback events have been removed properly before)

			// set item latent factors to zero
			item_factors.SetRowToOneValue(item_id, 0);
		}

		/// <summary>Retrain the latent factors of a given user</summary>
		/// <param name="user_id">the user ID</param>
		protected virtual void RetrainUser(int user_id)
		{
			MatrixUtils.InitNormal(user_factors, InitMean, InitStdev, user_id);

			HashSet<int> user_items = Feedback.UserMatrix[user_id];
			for (int i = 0; i < user_items.Count * iteration_length * NumIter; i++)
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
			MatrixUtils.InitNormal(item_factors, InitMean, InitStdev, item_id);

			int num_pos_events = Feedback.UserMatrix.NumberOfEntries;
			int num_item_iterations = num_pos_events * iteration_length * NumIter / (MaxItemID + 1);
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

		/// <summary>Compute approximate fit (AUC on training data)</summary>
		/// <returns>the fit</returns>
		public override double ComputeFit()
		{
			double sum_auc = 0;
			int num_user = 0;

			for (int user_id = 0; user_id < MaxUserID + 1; user_id++)
			{
				HashSet<int> test_items = Feedback.UserMatrix[user_id];
				if (test_items.Count == 0)
					continue;
				int[] prediction = ItemPrediction.PredictItems(this, user_id, MaxItemID);

				int num_eval_items = MaxItemID + 1;
				int num_eval_pairs = (num_eval_items - test_items.Count) * test_items.Count;

				int num_correct_pairs = 0;
				int num_pos_above = 0;
				// start with the highest weighting item...
				for (int i = 0; i < prediction.Length; i++)
				{
					int item_id = prediction[i];

					if (test_items.Contains(item_id))
						num_pos_above++;
					else
						num_correct_pairs += num_pos_above;
				}
				double user_auc = (double) num_correct_pairs / num_eval_pairs;
				sum_auc += user_auc;
				num_user++;
			}

			double auc = sum_auc / num_user;
			return auc;
		}

		private void CreateFastSamplingData(int u)
		{
			while (u >= user_pos_items.Count)
				user_pos_items.Add(null);
			while (u >= user_neg_items.Count)
				user_neg_items.Add(null);

			var pos_list = new List<int>(Feedback.UserMatrix[u]);
			user_pos_items[u] = pos_list.ToArray();
			var neg_list = new List<int>();
			for (int i = 0; i < MaxItemID; i++)
				if (! Feedback.UserMatrix[u, i])
					neg_list.Add(i);
			user_neg_items[u] = neg_list.ToArray();
		}

		/// <inheritdoc/>
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

						this.user_pos_items = new List<int[]>(MaxUserID + 1);
						this.user_neg_items = new List<int[]>(MaxUserID + 1);
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

		/// <inheritdoc/>
		public override void LoadModel(string filename)
		{
			base.LoadModel(filename);
			random = Util.Random.GetInstance();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni, "BPRMF num_factors={0} item_bias={1} reg_u={2} reg_i={3} reg_j={4} num_iter={5} learn_rate={6} fast_sampling_memory_limit={7} init_mean={8} init_stdev={9}",
								 num_factors, item_bias, reg_u, reg_i, reg_j, NumIter, learn_rate, fast_sampling_memory_limit, InitMean, InitStdev);
		}
	}
}
