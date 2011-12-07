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
using System.Threading.Tasks;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Matrix factorization for rating prediction on multiple cores</summary>
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
	public class MultiCoreMatrixFactorization : BiasedMatrixFactorization
	{
 		/// <summary>Gets or sets the number of groups.</summary>
		/// <value>The number of groups (for rows and columns of the rating matrix, each)</value>
		public int NumGroups { get; set; }
		
		/// <summary>default constructor</summary>
		public MultiCoreMatrixFactorization()
		{
			BoldDriver = true;
			NumGroups = 100;
		}

		IList<int>[,] blocks;

		///
		public override void Train()
		{
			// divide rating matrix into blocks
			var user_permutation = new List<int>(Enumerable.Range(0, MaxUserID + 1));
			var item_permutation = new List<int>(Enumerable.Range(0, MaxItemID + 1));
			Utils.Shuffle(user_permutation);
			Utils.Shuffle(item_permutation);

			blocks = new IList<int>[NumGroups, NumGroups];
			for (int i = 0; i < NumGroups; i++)
				for (int j = 0; j < NumGroups; j++)
					blocks[i, j] = new List<int>();

			for (int index = 0; index < ratings.Count; index++)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				blocks[user_permutation[u] % NumGroups, item_permutation[i] % NumGroups].Add(index);
			}

			// randomize index sequences inside the blocks
			for (int i = 0; i < NumGroups; i++)
				for (int j = 0; j < NumGroups; j++)
					Utils.Shuffle(blocks[i, j]);

			// perform training
			base.Train();
		}

		///
		public override void Iterate()
		{
			// generate random sub-epoch sequence
			var subepoch_sequence = new List<int>(Enumerable.Range(0, NumGroups));
			Utils.Shuffle(subepoch_sequence);

			foreach (int i in subepoch_sequence) // sub-epoch
				Parallel.For(0, NumGroups, j => Iterate(blocks[j, (i + j) % NumGroups], true, true));

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
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} learn_rate={5} num_iter={6} bold_driver={7} init_mean={8} init_stddev={9} optimize_mae={10} num_groups={11}",
				 this.GetType().Name, NumFactors, BiasReg, RegU, RegI, LearnRate, NumIter, BoldDriver, InitMean, InitStdDev, OptimizeMAE, NumGroups);
		}
	}
}

