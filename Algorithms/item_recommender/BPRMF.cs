// Copyright (C) 2010 Zeno Gantner, Christoph Freudenthaler
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
using System.Linq;
using System.Text;
using MyMediaLite.data_type;
using MyMediaLite.eval;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
	/// <summary>
	/// Matrix factorization model for item prediction optimized using BPR-Opt;
	/// Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, and Lars Schmidt-Thieme:
	/// BPR: Bayesian Personalized Ranking from Implicit Feedback.
	/// in UAI 2009
	/// </summary>
	/// <author>Zeno Gantner, Christoph Freudenthaler, University of Hildesheim</author>
	public class BPRMF : MF, IterativeModel
	{
		public bool item_bias = false;

		/// <summary>Regularization parameter for user factors</summary>
		public double reg_u = 0.0025;
		/// <summary>Regularization parameter for positive item factors</summary>
		public double reg_i = 0.0025;
		/// <summary>Regularization parameter for negative item factors</summary>
		public double reg_j = 0.00025;
		/// <summary>Learning rate alpha</summary>
		public double learn_rate = 0.05;
		/// <summary>One iteration is <see cref="iteration_length"/> * number of entries in the training matrix</summary>
		protected int iteration_length = 5;

		/// <summary>Fast, but memory-intensive sampling</summary>
		protected bool fast_sampling = false;
		/// <summary>Fast sampling memory limit, in MiB</summary>
		public int fast_sampling_memory_limit = 1024;
		/// <summary>support data structure for fast sampling</summary>
		protected List<int[]> user_pos_items;
		/// <summary>support data structure for fast sampling</summary>
		protected List<int[]> user_neg_items;

		/// <summary>Random number generator</summary>
		protected System.Random random;

		/// <inheritdoc/>
		public override void Train()
		{
			random = util.Random.GetInstance();
			CheckSampling();

			base.Train();
		}

		/// <summary>
		/// Perform one iteration of stochastic gradient ascent over the training data.
		/// One iteration is <see cref="iteration_length"/> * number of entries in the training matrix
		/// </summary>
		public override void Iterate()
		{
			int num_pos_events = data_user.GetNumberOfEntries();

			user_feature.SetColumnToOneValue(0, 1.0);

			for (int i = 0; i < num_pos_events * iteration_length; i++)
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateFeatures(user_id, item_id_1, item_id_2, true, true, true);
			}
		}

		// TODO move all sampling code to a class UserItemSampler

		/// <summary>Sample another item, given the first one and the user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the given item</param>
		/// <param name="j">the ID of the other item</param>
		/// <returns>true if the given item was already seen by user u</returns>
		protected virtual bool SampleOtherItem(int u, int i, out int j)
		{
			HashSet<int> user_items = data_user.GetRow (u);
			bool item_is_positive = user_items.Contains (i);

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
					j = random.Next (0, max_item_id + 1);
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
				HashSet<int> user_items = data_user.GetRow(u);
				i = user_items.ElementAt(random.Next (0, user_items.Count));
				do
					j = random.Next (0, max_item_id + 1);
				while (user_items.Contains(j));
			}
		}

		/// <summary>Sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		protected virtual int SampleUser()
		{
			while (true)
			{
				int u = random.Next(0, max_user_id + 1);
				HashSet<int> user_items = data_user.GetRow(u);
				if (user_items.Count == 0 || user_items.Count == max_item_id + 1)
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

		/// <summary>Update features according to the stochastic gradient descent update rule</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		/// <param name="update_u">if true, update the user features</param>
		/// <param name="update_i">if true, update the features of the first item</param>
		/// <param name="update_j">if true, update the features of the second item</param>
		protected virtual void UpdateFeatures(int u, int i, int j, bool update_u, bool update_i, bool update_j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			int start_feature = 0;

			if (item_bias)
			{
				start_feature = 1;
				double w_uf = user_feature.Get(u, 0);
				double h_if = item_feature.Get(i, 0);
				double h_jf = item_feature.Get(j, 0);

				if (update_i)
				{
					double if_update = w_uf / (1 + Math.Exp(x_uij)) - reg_i * h_if;
					item_feature.Set(i, 0, h_if + learn_rate * if_update);
				}

				if (update_j)
				{
					double jf_update = -w_uf / (1 + Math.Exp(x_uij)) - reg_j * h_jf;
					item_feature.Set(j, 0, h_jf + learn_rate * jf_update);
				}
			}

			for (int f = start_feature; f < num_features; f++)
			{
				double w_uf = user_feature.Get(u, f);
				double h_if = item_feature.Get(i, f);
				double h_jf = item_feature.Get(j, f);

				if (update_u)
				{
					double uf_update = (h_if - h_jf) / (1 + Math.Exp(x_uij)) - reg_u * w_uf;
					user_feature.Set(u, f, w_uf + learn_rate * uf_update);
				}

				if (update_i)
				{
					double if_update = w_uf / (1 + Math.Exp(x_uij)) - reg_i * h_if;
					item_feature.Set(i, f, h_if + learn_rate * if_update);
				}

				if (update_j)
				{
					double jf_update = -w_uf / (1 + Math.Exp(x_uij)) - reg_j * h_jf;
					item_feature.Set(j, f, h_jf + learn_rate * jf_update);
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
			if (user_id > max_user_id)
			{
				user_feature.AddRows(user_id + 1);
				MatrixUtils.InitNormal(user_feature, init_f_mean, init_f_stdev, user_id);
			}

			base.AddUser(user_id);
		}

		/// <inheritdoc/>
		public override void AddItem(int item_id)
		{
			if (item_id > max_item_id)
			{
				item_feature.AddRows(item_id + 1);
				MatrixUtils.InitNormal(item_feature, init_f_mean, init_f_stdev, item_id);
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

			// set user features to zero
			user_feature.SetRowToOneValue(user_id, 0);
		}

		/// <inheritdoc/>
		public override void RemoveItem(int item_id)
		{
			base.RemoveItem(item_id);

			// TODO remove from fast sampling data structures
			//      (however: not needed if all feedback events have been removed properly before)

			// set item features to zero
			item_feature.SetRowToOneValue(item_id, 0);
		}

		/// <summary>Retrain the features of a given user</summary>
		/// <param name="user_id">the user ID</param>
		protected virtual void RetrainUser(int user_id)
		{
			MatrixUtils.InitNormal(user_feature, init_f_mean, init_f_stdev, user_id);

			HashSet<int> user_items = data_user.GetRow(user_id);
			for (int i = 0; i < user_items.Count * iteration_length * num_iter; i++) {
				int item_id_1, item_id_2;
				SampleItemPair(user_id, out item_id_1, out item_id_2);
				UpdateFeatures(user_id, item_id_1, item_id_2, true, false, false);
			}
		}

		/// <summary>Retrain the features of a given item</summary>
		/// <param name="item_id">the item ID</param>
		protected virtual void RetrainItem(int item_id)
		{
			MatrixUtils.InitNormal(item_feature, init_f_mean, init_f_stdev, item_id);

			int num_pos_events = data_user.GetNumberOfEntries();
			int num_item_iterations = num_pos_events * iteration_length * num_iter / (max_item_id + 1);
			for (int i = 0; i < num_item_iterations; i++) {
				// remark: the item may be updated more or less frequently than in the normal from-scratch training
				int user_id = SampleUser();
				int other_item_id;
				bool item_is_positive = SampleOtherItem(user_id, item_id, out other_item_id);

				if (item_is_positive)
					UpdateFeatures(user_id, item_id, other_item_id, false, true, false);
				else
					UpdateFeatures(user_id, other_item_id, item_id, false, false, true);
			}
		}

		/// <summary>Compute approximate fit (AUC on training data)</summary>
		/// <returns>the fit</returns>
		public override double ComputeFit()
		{
			double sum_auc = 0;
			int num_user = 0;

			for (int user_id = 0; user_id < max_user_id + 1; user_id++)
			{
				HashSet<int> test_items = data_user.GetRow (user_id);
				if (test_items.Count == 0)
					continue;
				int[] prediction = ItemPrediction.PredictItems(this, user_id, max_item_id);

				int num_eval_items = max_item_id + 1;
				int num_eval_pairs = (num_eval_items - test_items.Count) * test_items.Count;

				int num_correct_pairs = 0;
				int num_pos_above = 0;
				// start with the highest weighting item...
				for (int i = 0; i < prediction.Length; i++)
				{
					int item_id = prediction[i];

					if (test_items.Contains (item_id)) {
						num_pos_above++;
					} else {
						num_correct_pairs += num_pos_above;
					}
				}
				double user_auc = ((double)num_correct_pairs) / num_eval_pairs;
				sum_auc += user_auc;
				num_user++;
			}

			double auc = sum_auc / num_user;
			return auc;
		}

		protected void CreateFastSamplingData(int u)
		{
			while (u >= user_pos_items.Count)
				user_pos_items.Add(null);
			while (u >= user_neg_items.Count)
				user_neg_items.Add(null);

			List<int> pos_list = new List<int>(data_user.GetRow(u));
			user_pos_items[u] = pos_list.ToArray();
			List<int> neg_list = new List<int>();
			for (int i = 0; i < max_item_id; i++)
				if (!data_user.GetRow(u).Contains(i))
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
					int fast_sampling_memory_size = ((max_user_id + 1) * (max_item_id + 1) * 4) / (1024 * 1024);
					Console.Error.WriteLine("fast_sampling_memory_size=" + fast_sampling_memory_size);

					if (fast_sampling_memory_size <= fast_sampling_memory_limit)
					{
						fast_sampling = true;

						user_pos_items = new List<int[]>(max_user_id + 1);
						user_neg_items = new List<int[]>(max_user_id + 1);
						for (int u = 0; u < max_user_id + 1; u++)
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
		public override void LoadModel(string filePath)
		{
			base.LoadModel(filePath);
			random = util.Random.GetInstance();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return String.Format("BPR-MF num_features={0} item_bias={1} reg_u={2} reg_i={3} reg_j={4} num_iter={5} learn_rate={6} fast_sampling_memory_limit={7} init_f_mean={8} init_f_stdev={9}",
			                     num_features, item_bias, reg_u, reg_i, reg_j, num_iter, learn_rate, fast_sampling_memory_limit, init_f_mean, init_f_stdev);
		}
	}
}
