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
using System.Linq;
using System.Threading.Tasks;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Matrix factorization for BPR on multiple cores</summary>
	/// <remarks>
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///     Rainer Gemulla, Peter J. Haas, Erik Nijkamp, Yannis Sismanis:
	///     Large-Scale Matrix Factorization with Distributed Stochastic Gradient Descent.
	///     KDD 2011.
	///     http://www.mpi-inf.mpg.de/~rgemulla/publications/gemulla11dsgd.pdf
	///   </description></item>
	/// </list>
	///
	/// This recommender supports incremental updates, however they are currently not performed on multiple cores.
	/// </remarks>
	public class MultiCoreBPRMF : BPRMF
	{
		// TODO support uniform user sampling
		//ISparseBooleanMatrix replacement_user_matrix;

		/// <summary>the maximum number of threads to use</summary>
		/// <remarks>
		///   Determines the number of sections the users and items will be divided into.
		/// </remarks>
		public int MaxThreads { get; set; }

		/// <summary>default constructor</summary>
		public MultiCoreBPRMF()
		{
			MaxThreads = 100;
		}

		IList<int>[,] blocks;
		IList<int>[] items_by_group;

		///
		public override void Train()
		{
			if (UniformUserSampling)
				throw new NotSupportedException("uniform_user_sampling");
			if (WithReplacement)
				throw new NotSupportedException("with_replacement");

			blocks = Feedback.PartitionUsersAndItems(MaxThreads);
			items_by_group = new IList<int>[MaxThreads];
			for (int item_group = 0; item_group < MaxThreads; item_group++)
			{
				var items_in_group = new HashSet<int>();
				for (int user_group = 0; user_group < MaxThreads; user_group++)
					foreach (int index in blocks[user_group, item_group])
						items_in_group.Add(Feedback.Items[index]);
				items_by_group[item_group] = items_in_group.ToArray();
			}

			// perform training
			base.Train();
		}

		///
		public override void Iterate()
		{
			// generate random sub-epoch sequence
			var subepoch_sequence = new List<int>(Enumerable.Range(0, MaxThreads));
			subepoch_sequence.Shuffle();

			foreach (int i in subepoch_sequence) // sub-epoch
				//Parallel.For(0, NumGroups, j => Iterate(j, (i + j) % NumGroups));
				for (int j = 0; j < MaxThreads; j++)
					Iterate(j, (i + j) % MaxThreads);

			if (BoldDriver)
			{
				double loss = ComputeLoss();

				if (loss > last_loss)
					LearnRate *= 0.5f;
				else if (loss < last_loss)
					LearnRate *= 1.05f;

				last_loss = loss;

				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loss {0} learn_rate {1} ", loss, LearnRate));
			}
		}

		/// <summary>Iterate over the examples of a specific block of the feedback matrix</summary>
		/// <param name='user_group'>the user group</param>
		/// <param name='item_group'>the item group</param>
		void Iterate(int user_group, int item_group)
		{
			int user_id, pos_item_id, neg_item_id;

			// uniform pair sampling, without replacement
			foreach (int index in blocks[user_group, item_group])
			{
				user_id = Feedback.Users[index];
				pos_item_id = Feedback.Items[index];
				neg_item_id = SampleNegativeItem(user_id, item_group);
				UpdateFactors(user_id, pos_item_id, neg_item_id, true, true, update_j);
			}
		}

		/// <summary>Sample negative item, given the the user and the item group</summary>
		/// <param name="u">the user ID</param>
		/// <param name="item_group">the group the item has to belong to</param>
		/// <returns>the ID of the negative item</returns>
		int SampleNegativeItem(int u, int item_group)
		{
			int j = -1;

			// TODO support fast_sampling
//			if (fast_sampling)
//			{
//				if (item_is_positive)
//				{
//					int rindex = random.Next(user_neg_items[u].Count);
//					j = user_neg_items[u][rindex];
//				}
//				else
//				{
//					int rindex = random.Next(user_pos_items[u].Count);
//					j = user_pos_items[u][rindex];
//				}
//			}
//			else
			{
				do {
					int random_index = random.Next(items_by_group[item_group].Count);
					j = items_by_group[item_group][random_index];
				} while (Feedback.UserMatrix[u, j] == true);
			}

			return j;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} reg_j={5} num_iter={6} learn_rate={7} uniform_user_sampling={8} with_replacement={9}, bold_driver={10} fast_sampling_memory_limit={11} update_j={12} init_mean={13} init_stddev={14} max_threads={15}",
				this.GetType().Name, num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, learn_rate, UniformUserSampling, WithReplacement, BoldDriver, fast_sampling_memory_limit, UpdateJ, InitMean, InitStdDev, MaxThreads);
		}

	}
}
