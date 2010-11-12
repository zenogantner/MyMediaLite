// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
	/// <summary>
	/// Linear model optimized for BPR
	/// </summary>
	/// <remarks>
    /// This engine does not support online updates.
	/// </remarks>
	public class BPR_Linear : Memory, IItemAttributeAwareRecommender, IIterativeModel
	{
		/// <inheritdoc/>
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set
			{
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows);
			}
		}
		private SparseBooleanMatrix item_attributes;

		/// <inheritdoc/>
	    public int NumItemAttributes { get;	set; }

	    /// <summary>Item attribute weights</summary>
        Matrix<double> item_attribute_weight_by_user;

		/// <summary>One iteration is <see cref="iteration_length"/> * number of entries in the training matrix</summary>
		protected int iteration_length = 5;

		private System.Random random;
		/// <summary>Fast, but memory-intensive sampling</summary>
		bool fast_sampling = false;

        /// <summary>Number of iterations over the training data</summary>
		public int NumIter { get { return num_iter; } set { num_iter = value; } }
		private int num_iter = 10;

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

		/// <summary>support data structure for fast sampling</summary>
		int[][] user_pos_items;
		/// <summary>support data structure for fast sampling</summary>
		int[][] user_neg_items;

		/// <inheritdoc/>
		public override void Train()
		{
			random = util.Random.GetInstance();

			// prepare fast sampling, if necessary
			int support_data_size = ((MaxUserID + 1) * (MaxItemID + 1) * 4) / (1024 * 1024);
			Console.Error.WriteLine("BPR-LIN sds=" + support_data_size);
			if (support_data_size <= fast_sampling_memory_limit)
			{
				fast_sampling = true;

				user_pos_items = new int[MaxUserID + 1][];
				user_neg_items = new int[MaxUserID + 1][];
				for (int u = 0; u < MaxUserID + 1; u++)
				{
					List<int> pos_list = new List<int>(data_user[u]);
					user_pos_items[u] = pos_list.ToArray();
					List<int> neg_list = new List<int>();
					for (int i = 0; i < MaxItemID; i++)
						if (!data_user[u].Contains(i) && data_item[i].Count != 0)
							neg_list.Add(i);
					user_neg_items[u] = neg_list.ToArray();
				}
			}

        	item_attribute_weight_by_user = new Matrix<double>(MaxUserID + 1, NumItemAttributes);
        	MatrixUtils.InitNormal(item_attribute_weight_by_user, init_mean, init_stdev);

			for (int i = 0; i < NumIter; i++)
			{
				Iterate();
				Console.Error.WriteLine(i);
			}
		}

		/// <summary>
		/// Perform one iteration of stochastic gradient ascent over the training data.
		/// One iteration is <see cref="iteration_length"/> * number of entries in the training matrix
		/// </summary>
		public void Iterate()
		{
			int num_pos_events = data_user.NumberOfEntries;

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
				int rindex;

				rindex = random.Next (0, user_pos_items[u].Length);
				i = user_pos_items[u][rindex];

				rindex = random.Next (0, user_neg_items[u].Length);
				j = user_neg_items[u][rindex];
			}
			else
			{
				HashSet<int> user_items = data_user[u];
				i = user_items.ElementAt(random.Next (0, user_items.Count));
				do
					j = random.Next (0, MaxItemID + 1);
				while (user_items.Contains(j) || data_item[j].Count == 0); // don't sample the item if it never has been viewed (maybe unknown item!)
			}
		}

		/// <summary>Sample a user that has viewed at least one and not all items</summary>
		/// <returns>the user ID</returns>
		protected int SampleUser()
		{
			while (true)
			{
				int u = random.Next(0, MaxUserID + 1);
				HashSet<int> user_items = data_user[u];
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

		/// <summary>
		/// Modified feature update method that exploits attribute sparsity
		/// </summary>
		protected virtual void UpdateFeatures(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			HashSet<int> attr_i = item_attributes[i];
			HashSet<int> attr_j = item_attributes[j];

			// assumption: attributes are sparse
			HashSet<int> attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			HashSet<int> attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			foreach (int a in attr_i_over_j)
			{
				double w_uf = item_attribute_weight_by_user[u, a];
				double uf_update = 1 / (1 + Math.Exp(x_uij)) - regularization * w_uf;
				item_attribute_weight_by_user[u, a] = w_uf + learn_rate * uf_update;
			}
			foreach (int a in attr_j_over_i)
			{
				double w_uf = item_attribute_weight_by_user[u, a];
				double uf_update = -1 / (1 + Math.Exp(x_uij)) - regularization * w_uf;
				item_attribute_weight_by_user[u, a] = w_uf + learn_rate * uf_update;
			}
		}

		/// <inheritdoc/>
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
			HashSet<int> attributes = this.item_attributes[item_id];
			foreach (int a in attributes)
				result += item_attribute_weight_by_user[user_id, a];
            return result;
        }

		/// <inheritdoc/>
		public override void SaveModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Engine.GetWriter(filePath, this.GetType()) )
			{
				writer.WriteLine(item_attribute_weight_by_user.dim1 + " " + item_attribute_weight_by_user.dim2);
				for (int i = 0; i < item_attribute_weight_by_user.dim1; i++)
					for (int j = 0; j < item_attribute_weight_by_user.dim2; j++)
						writer.WriteLine(i + " " + j + " " + item_attribute_weight_by_user[i, j].ToString(ni));
			}
		}

		/// <inheritdoc/>
		public override void LoadModel(string filePath)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

            using ( StreamReader reader = Engine.GetReader(filePath, this.GetType()) )
			{
            	string[] numbers = reader.ReadLine().Split(' ');
				int num_users = Int32.Parse(numbers[0]);
				int dim2 = Int32.Parse(numbers[1]);

				MaxUserID = num_users - 1;
				Matrix<double> matrix = new Matrix<double>(num_users, dim2);
				int num_item_attributes = dim2;

            	while ((numbers = reader.ReadLine().Split(' ')).Length == 3)
            	{
					int i = Int32.Parse(numbers[0]);
					int j = Int32.Parse(numbers[1]);
					double v = Double.Parse(numbers[2], ni);

                	if (i >= num_users)
						throw new Exception(string.Format("Invalid user ID {0} is greater than {1}.", i, num_users - 1));
					if (j >= num_item_attributes)
						throw new Exception(string.Format("Invalid weight ID {0} is greater than {1}.", j, num_item_attributes - 1));

                	matrix[i, j] = v;
				}

				this.item_attribute_weight_by_user = matrix;
			}
		}

		/// <inheritdoc/>
		public double ComputeFit()
		{
			// TODO
			return -1;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format("BPR-Linear reg={0} num_iter={1} learn_rate={2} fast_sampling_memory_limit={3} init_mean={4} init_stdev={5}",
								  regularization, NumIter, learn_rate, fast_sampling_memory_limit, init_mean, init_stdev);
		}

	}
}

