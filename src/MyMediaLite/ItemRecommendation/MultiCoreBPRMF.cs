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

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Matrix factorization for BPR on multiple cores</summary>
	/// <remarks>
	/// This recommender supports incremental updates, however they are currently not performed on multiple cores.
	/// </remarks>
	public class MultiCoreBPRMF : BPRMF
	{
		/// <summary>the maximum number of threads to use</summary>
		/// <remarks>
		///   Determines the number of sections the users and items will be divided into.
		/// </remarks>
		public int MaxThreads { get; set; }

		IList<IList<int>> index_blocks;

		/// <summary>default constructor</summary>
		public MultiCoreBPRMF()
		{
			WithReplacement = false;
			UniformUserSampling = false;
			MaxThreads = 100;
		}

		///
		public override void Train()
		{
			index_blocks = Feedback.PartitionIndices(MaxThreads);

			// perform training
			base.Train();
		}

		///
		protected override void IterateWithoutReplacementUniformPair()
		{
			Parallel.ForEach(
				index_blocks,
				indices => IterateWithoutReplacementUniformPair(indices));
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} reg_j={5} num_iter={6} learn_rate={7} uniform_user_sampling={8} with_replacement={9} update_j={10} max_threads={11}",
				this.GetType().Name, num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, learn_rate, UniformUserSampling, WithReplacement, UpdateJ, MaxThreads);
		}
	}
}
