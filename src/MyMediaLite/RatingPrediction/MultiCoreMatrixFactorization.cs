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
		/// <summary>the maximum number of threads to use</summary>
		/// <remarks>
		///   Determines the number of sections the users and items will be divided into.
		/// </remarks>
		public int MaxThreads { get; set; }

		/// <summary>default constructor</summary>
		public MultiCoreMatrixFactorization()
		{
			BoldDriver = true;
			MaxThreads = 100;
		}

		IList<int>[,] blocks;

		///
		public override void Train()
		{
			blocks = ratings.PartitionUsersAndItems(MaxThreads);

			// perform training
			base.Train();
		}

		///
		public override void Iterate()
		{
			// generate random sub-epoch sequence
			var subepoch_sequence = new List<int>(Enumerable.Range(0, MaxThreads));
			Utils.Shuffle(subepoch_sequence);

			foreach (int i in subepoch_sequence) // sub-epoch
				Parallel.For(0, MaxThreads, j => Iterate(blocks[j, (i + j) % MaxThreads], true, true));

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

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} learn_rate={5} num_iter={6} bold_driver={7} init_mean={8} init_stddev={9} optimize_mae={10} max_threads={11}",
				 this.GetType().Name, NumFactors, BiasReg, RegU, RegI, LearnRate, NumIter, BoldDriver, InitMean, InitStdDev, OptimizeMAE, MaxThreads);
		}
	}
}

