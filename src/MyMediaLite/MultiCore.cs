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
//
using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Data;

namespace MyMediaLite
{
	/// <summary>Utility routines for multi-core algorithms</summary>
	public static class MultiCore
	{
		/// <summary>Partition dataset user- and item-wise for parallel processing</summary>
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
		/// </remarks>
		/// <returns>a two-dimensional array of index lists, each entry corresponds to one block entry</returns>
		/// <param name='dataset'>a feedback dataset</param>
		/// <param name='num_groups'>the number of groups both users and items are partitioned into</param>
		public static IList<int>[,] PartitionUsersAndItems(this IDataSet dataset, int num_groups)
		{
			num_groups = Math.Min(num_groups, dataset.MaxUserID + 1);
			num_groups = Math.Min(num_groups, dataset.MaxItemID + 1);

			// divide rating matrix into blocks
			var user_permutation = new List<int>(Enumerable.Range(0, dataset.MaxUserID + 1));
			var item_permutation = new List<int>(Enumerable.Range(0, dataset.MaxItemID + 1));
			user_permutation.Shuffle();
			item_permutation.Shuffle();

			var blocks = new IList<int>[num_groups, num_groups];
			for (int i = 0; i < num_groups; i++)
				for (int j = 0; j < num_groups; j++)
					blocks[i, j] = new List<int>();

			for (int index = 0; index < dataset.Count; index++)
			{
				int u = dataset.Users[index];
				int i = dataset.Items[index];

				blocks[user_permutation[u] % num_groups, item_permutation[i] % num_groups].Add(index);
			}

			// randomize index sequences inside the blocks
			for (int i = 0; i < num_groups; i++)
				for (int j = 0; j < num_groups; j++)
					blocks[i, j].Shuffle();

			return blocks;
		}

		/// <summary>Partition the indices of a dataset into groups</summary>
		/// <returns>the grouped indices</returns>
		/// <param name='dataset'>a dataset</param>
		/// <param name='num_groups'>the number of groups</param>
		public static IList<IList<int>> PartitionIndices(this IDataSet dataset, int num_groups)
		{
			num_groups = Math.Min(num_groups, dataset.Count);
			var indices = dataset.RandomIndex;

			var groups = new IList<int>[num_groups];
			for (int i = 0; i < num_groups; i++)
				groups[i] = new List<int>();

			for (int index = 0; index < dataset.Count; index++)
				groups[index % num_groups].Add(indices[index]);

			return groups;
		}
	}
}

