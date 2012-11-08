// Copyright (C) 2012 Lucas Drumond
// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
	/// <summary>Sparse Linear Methods (SLIM) for item prediction (ranking) optimized for BPR-Opt optimization criterion </summary>
	/// <remarks>
	/// This implementation differs from the algorithm in the original SLIM paper since the model here is optimized for BPR-Opt
	/// instead of the elastic net loss. The optmization algorithm used is the Sotchastic Gradient Ascent.
	///
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///     Steffen Rendle, Christoph Freudenthaler, Zeno Gantner, Lars Schmidt-Thieme:
	///     BPR: Bayesian Personalized Ranking from Implicit Feedback.
	///     UAI 2009.
	///     http://www.ismll.uni-hildesheim.de/pub/pdfs/Rendle_et_al2009-Bayesian_Personalized_Ranking.pdf
	///   </description></item>
	///   <item><description>
	///     X. Ning, G. Karypis: Slim: Sparse linear methods for top-n recommender systems.
	///    ICDM 2011.
	///    http://glaros.dtc.umn.edu/gkhome/fetch/papers/SLIM2011icdm.pdf
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
	public class BPRSLIM : SLIM
	{
		/// <summary>Fast, but memory-intensive sampling</summary>
		protected bool fast_sampling = false;

		/// <summary>Fast sampling memory limit, in MiB</summary>
		public int FastSamplingMemoryLimit { get { return fast_sampling_memory_limit; }	set { fast_sampling_memory_limit = value; } }
		/// <summary>Fast sampling memory limit, in MiB</summary>
		protected int fast_sampling_memory_limit = 1024;

		/// <summary>Sample positive observations with (true) or without (false) replacement</summary>
		public bool WithReplacement { get; set; }

		/// <summary>Sample uniformly from users</summary>
		public bool UniformUserSampling { get; set; }

		/// <summary>Learning rate alpha</summary>
		public double LearnRate { get {	return learn_rate; } set { learn_rate = value; } }
		/// <summary>Learning rate alpha</summary>
		protected double learn_rate = 0.05;

		/// <summary>Regularization parameter for positive item weights</summary>
		public double RegI { get { return reg_i; } set { reg_i = value;	} }
		/// <summary>Regularization parameter for positive item weights</summary>
		protected double reg_i = 0.0025;

		/// <summary>Regularization parameter for negative item weights</summary>
		public double RegJ { get { return reg_j; } set { reg_j = value; } }
		/// <summary>Regularization parameter for negative item weights</summary>
		protected double reg_j = 0.00025;

		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		public bool UpdateJ { get { return update_j; } set { update_j = value; } }
		/// <summary>If set (default), update factors for negative sampled items during learning</summary>
		protected bool update_j = true;

		/// <summary>support data structure for fast sampling</summary>
		protected IList<IList<int>> user_pos_items;
		/// <summary>support data structure for fast sampling</summary>
		protected IList<IList<int>> user_neg_items;

		/// <summary>Random number generator</summary>
		protected System.Random random;

		/// <summary>Default constructor</summary>
		public BPRSLIM()
		{
			UniformUserSampling = true;
		}

		///
		protected override void InitModel()
		{
			base.InitModel();
		}

		///
		public override void Train()
		{
			InitModel();

			CheckSampling();

			random = MyMediaLite.Random.GetInstance();

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
				while (Feedback.UserMatrix[u, j] != item_is_positive);
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
			double x_uij = PredictWithDifference(u, i, j);

			double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

			// adjust factors
			var user_items = Feedback.UserMatrix.GetEntriesByRow(u);

			foreach (int f in user_items)
			{
				double w_if = item_weights[i, f];
				double w_jf = item_weights[j, f];

				if (update_i && i != f)
				{
					double update = one_over_one_plus_ex - reg_i * w_if;
					item_weights[i, f] = (float) (w_if + learn_rate * update);
				}

				if (update_j && j != f)
				{
					double update = - one_over_one_plus_ex - reg_j * w_jf;
					item_weights[j, f] = (float) (w_jf + learn_rate * update);
				}
			}
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

			if (fast_sampling)
			{
				user_pos_items[user_id] = null;
				user_neg_items[user_id] = null;
			}
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
			item_weights.SetRowToOneValue(item_id, 0);
		}


		/// <summary>Retrain the latent factors of a given item</summary>
		/// <param name="item_id">the item ID</param>
		protected virtual void RetrainItem(int item_id)
		{
			item_weights.RowInitNormal(item_id, InitMean, InitStdDev);

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

		/// <summary>Compute the fit (AUC on training data)</summary>
		/// <returns>the fit</returns>
		public override float ComputeObjective()
		{
			return 0;
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
		public double PredictWithDifference(int user_id, int pos_item_id, int neg_item_id)
		{
			if (user_id > MaxUserID || pos_item_id > MaxItemID || neg_item_id > MaxItemID)
				return double.MinValue;

			var user_items = Feedback.UserMatrix.GetEntriesByRow(user_id);
			double prediction = 0;

			for (int k = 0; k < user_items.Count; k++)
			{
				int f = user_items.ElementAt(k);
				prediction += item_weights[pos_item_id, f] - item_weights[neg_item_id, f];
			}
			return prediction;
		}

		///
		public override void SaveModel(string file)
		{
			using ( StreamWriter writer = Model.GetWriter(file, this.GetType(), "2.99") )
			{
				writer.WriteMatrix(item_weights);
			}
		}

		///
		public override void LoadModel(string file)
		{
			using ( StreamReader reader = Model.GetReader(file, this.GetType()) )
			{
				var item_weights = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				this.MaxItemID = item_weights.NumberOfRows - 1;

				this.item_weights = item_weights;
			}
			random = MyMediaLite.Random.GetInstance();
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} reg_i={1} reg_j={2} num_iter={3} learn_rate={4} uniform_user_sampling={5} with_replacement={6} fast_sampling_memory_limit={7} update_j={8}",
				this.GetType().Name, reg_i, reg_j, NumIter, learn_rate, UniformUserSampling, WithReplacement, fast_sampling_memory_limit, UpdateJ);
		}
	}
}

