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
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Weigthed BPR-MF with frequency-adjusted sampling</summary>
	/// <remarks>
	/// Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Lars Schmidt-Thieme:
	/// Bayesian Personalized Ranking for Non-Uniformly Sampled Items.
	/// KDD Cup Workshop 2011
	/// </remarks>
	public class WeightedBPRMF : BPRMF
	{
		// TODO offer this data structure from Feedback
		/// <summary>array of user IDs of positive user-item pairs</summary>
		protected int[] users;
		/// <summary>array of item IDs of positive user-item pairs</summary>
		protected int[] items;

		/// <summary>Default constructor</summary>
		public WeightedBPRMF() { }

		///
		public override void Train()
		{
			// de-activate until supported
			WithReplacement = false;
			// de-activate until false is supported
			UniformUserSampling = true;

			// prepare helper data structures for training
			users = new int[Feedback.Count];
			items = new int[Feedback.Count];

			int index = 0;
			foreach (int user_id in Feedback.UserMatrix.NonEmptyRowIDs)
				foreach (int item_id in Feedback.UserMatrix[user_id])
				{
					users[index] = user_id;
					items[index] = item_id;

					index++;
				}

			// suppress using user_neg_items in BPRMF
			FastSamplingMemoryLimit = 0;

			base.Train();
		}

		///
		protected override void SampleTriple(out int u, out int i, out int j)
		{
			// sample user from positive user-item pairs
			int index = random.Next(items.Length - 1);
			u = users[index];
			i = items[index];

			// sample negative item
			do
				j = items[random.Next(items.Length - 1)];
			while (Feedback.UserMatrix[u, j]);
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} reg_j={5} num_iter={6} learn_rate={7}",
				this.GetType().Name, num_factors, BiasReg, reg_u, reg_i, reg_j, NumIter, learn_rate);
		}
	}
}

