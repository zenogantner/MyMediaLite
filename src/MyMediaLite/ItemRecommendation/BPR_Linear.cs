// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Linear model optimized for BPR</summary>
	/// <remarks>
	/// This recommender does not support online updates.
	/// </remarks>
	public class BPR_Linear : ItemRecommender, IItemAttributeAwareRecommender, IIterativeModel
	{
		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set	{
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private SparseBooleanMatrix item_attributes;

		///
		public int NumItemAttributes { get;	set; }

		// Item attribute weights
		private Matrix<double> item_attribute_weight_by_user;

		/// <summary>One iteration is <see cref="iteration_length"/> * number of entries in the training matrix</summary>
		protected int iteration_length = 5;

		private System.Random random;
		// Fast, but memory-intensive sampling
		private bool fast_sampling = false;

		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get { return num_iter; } set { num_iter = value; } }
		private uint num_iter = 10;

		/// <summary>Fast sampling memory limit, in MiB</summary>
		public int FastSamplingMemoryLimit { get { return fast_sampling_memory_limit; } set { fast_sampling_memory_limit = value; }	}
		int fast_sampling_memory_limit = 1024;

 		/// <summary>mean of the Gaussian distribution used to initialize the features</summary>
		public double InitMean { get { return init_mean; } set { init_mean = value; } }
		double init_mean = 0;

		/// <summary>standard deviation of the normal distribution used to initialize the features</summary>
		public double InitStdev { get { return init_stdev; } set { init_stdev = value; } }
		double init_stdev = 0.1;

		/// <summary>Learning rate alpha</summary>
		public double LearnRate { get { return learn_rate; } set { learn_rate = value; } }
		double learn_rate = 0.05;

		/// <summary>Regularization parameter</summary>
		public double Regularization { get { return regularization; }	set { regularization = value; } }
		double regularization = 0.015;

		// support data structure for fast sampling
		private IList<int>[] user_pos_items;
		// support data structure for fast sampling
		private IList<int>[] user_neg_items;

		///
		public override void Train()
		{
			random = Util.Random.GetInstance();

			// prepare fast sampling, if necessary
			int support_data_size = ((MaxUserID + 1) * (MaxItemID + 1) * 4) / (1024 * 1024);
			Console.Error.WriteLine("sds=" + support_data_size);
			if (support_data_size <= fast_sampling_memory_limit)
			{
				fast_sampling = true;

				this.user_pos_items = new int[MaxUserID + 1][];
				this.user_neg_items = new int[MaxUserID + 1][];
				for (int u = 0; u < MaxUserID + 1; u++)
				{
					var pos_list = new List<int>(Feedback.UserMatrix[u]);
					user_pos_items[u] = pos_list.ToArray();
					var neg_list = new List<int>();
					for (int i = 0; i < MaxItemID; i++)
						if (!Feedback.UserMatrix[u].Contains(i) && Feedback.ItemMatrix[i].Count != 0) // TODO we can spare the item matrix, thus use less memory
							neg_list.Add(i);
					user_neg_items[u] = neg_list.ToArray();
				}
			}

			this.item_attribute_weight_by_user = new Matrix<double>(MaxUserID + 1, NumItemAttributes);
			MatrixUtils.InitNormal(item_attribute_weight_by_user, init_mean, init_stdev);

			for (uint i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>
		/// Perform one iteration of stochastic gradient ascent over the training data.
		/// One iteration is <see cref="iteration_length"/> * number of entries in the training matrix
		/// </summary>
		public void Iterate()
		{
			int num_pos_events = Feedback.Count;

			for (int i = 0; i < num_pos_events * iteration_length; i++)
			{
				if (i % 1000000 == 999999)
					Console.Error.Write(".");
				if (i % 100000000 == 99999999)
					Console.Error.WriteLine();

				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);

				UpdateFeatures(user_id, item_id_1, item_id_2);
			}
		}

		/// <summary>Sample a pair of items, given a user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the first item</param>
		/// <param name="j">the ID of the second item</param>
		protected  void SampleItemPair(int u, out int i, out int j)
		{
			if (fast_sampling)
			{
				i = user_pos_items[u][random.Next(0, user_pos_items[u].Count)];
				j = user_neg_items[u][random.Next (0, user_neg_items[u].Count)];
			}
			else
			{
				var user_items = Feedback.UserMatrix[u];
				i = user_items.ElementAt(random.Next (0, user_items.Count));
				do
					j = random.Next (0, MaxItemID + 1);
				while (Feedback.UserMatrix[u, j] || Feedback.ItemMatrix[j].Count == 0); // don't sample the item if it never has been viewed (maybe unknown item!)
				// TODO think about saving the property accesses here
			}
		}

		/// <summary>Sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		protected int SampleUser()
		{
			while (true)
			{
				int u = random.Next(0, MaxUserID + 1);
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
		protected void SampleTriple(out int u, out int i, out int j)
		{
			u = SampleUser();
			SampleItemPair(u, out i, out j);
		}

		/// <summary>Modified feature update method that exploits attribute sparsity</summary>
		protected virtual void UpdateFeatures(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			ICollection<int> attr_i = item_attributes[i];
			ICollection<int> attr_j = item_attributes[j];

			// assumption: attributes are sparse
			var attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			var attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			double one_over_one_plus_ex = 1 / (1 + Math.Exp(x_uij));

			foreach (int a in attr_i_over_j)
			{
				double w_uf = item_attribute_weight_by_user[u, a];
				double uf_update = one_over_one_plus_ex - regularization * w_uf;
				item_attribute_weight_by_user[u, a] = w_uf + learn_rate * uf_update;
			}
			foreach (int a in attr_j_over_i)
			{
				double w_uf = item_attribute_weight_by_user[u, a];
				double uf_update = -one_over_one_plus_ex - regularization * w_uf;
				item_attribute_weight_by_user[u, a] = w_uf + learn_rate * uf_update;
			}
		}

		///
		public override double Predict(int user_id, int item_id)
		{
			if ((user_id < 0) || (user_id >= item_attribute_weight_by_user.dim1))
			{
				Console.Error.WriteLine("user is unknown: " + user_id);
				return 0;
			}
			if ((item_id < 0) || (item_id > MaxItemID))
			{
				Console.Error.WriteLine("item is unknown: " + item_id);
				return 0;
			}

			double result = 0;
			foreach (int a in item_attributes[item_id])
				result += item_attribute_weight_by_user[user_id, a];
			return result;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
				IMatrixUtils.WriteMatrix(writer, item_attribute_weight_by_user);
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
				this.item_attribute_weight_by_user = (Matrix<double>) IMatrixUtils.ReadMatrix(reader, new Matrix<double>(0, 0));
		}

		///
		public double ComputeFit()
		{
			// TODO
			return -1;
		}

		///
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni,
								 "BPR_Linear reg={0} num_iter={1} learn_rate={2} fast_sampling_memory_limit={3} init_mean={4} init_stdev={5}",
								 Regularization, NumIter, LearnRate, FastSamplingMemoryLimit, InitMean, InitStdev);
		}

	}
}

