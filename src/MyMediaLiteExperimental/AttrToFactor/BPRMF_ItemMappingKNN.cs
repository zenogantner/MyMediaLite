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
using System.Diagnostics;
using System.Globalization;
using MyMediaLite;
using MyMediaLite.Correlation;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.Util;

namespace MyMediaLite.AttrToFactor
{
	/// <summary>BPR-MF with item mapping learned by kNN</summary>
	public class BPRMF_ItemMappingKNN : BPRMF_ItemMapping
	{
		/// <summary>Number of neighbors to be used for mapping</summary>
		public uint K { get { return k; } set { k = value; } }
		uint k = uint.MaxValue;

		CorrelationMatrix item_correlation;

		///
		public override void LearnAttributeToFactorMapping()
		{
			BinaryCosine cosine_correlation = new BinaryCosine(MaxItemID + 1);
			Console.Error.WriteLine("training with max_item_id={0}", MaxItemID);
			cosine_correlation.ComputeCorrelations(item_attributes);
			this.item_correlation = cosine_correlation;
			_MapToLatentFactorSpace = Utils.Memoize<int, double[]>(__MapToLatentFactorSpace);
		}

		/// <summary>map to latent factor space (actual function)</summary>
		protected override double[] __MapToLatentFactorSpace(int item_id)
		{
			var est_factors = new double[num_factors];

			IList<int> relevant_items = item_correlation.GetPositivelyCorrelatedEntities(item_id);

			double weight_sum = 0;
			uint neighbors =  k;
			foreach (int item_id2 in relevant_items)
			{
				if (item_id2 >= item_factors.dim1) // check whether item is in training data
					continue;
				if (Feedback.ItemMatrix[item_id2].Count == 0)
					continue;

				double weight = item_correlation[item_id, item_id2];
				weight_sum += weight;
				for (int f = 0; f < num_factors; f++)
					est_factors[f] += weight * item_factors[item_id2, f];

				if (--neighbors == 0)
					break;
			}

			for (int f = 0; f < num_factors; f++)
				est_factors[f] /= weight_sum;

			return est_factors;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"BPRMF_ItemMappingKNN num_factors={0} reg_u={1} reg_i={2} reg_j={3} num_iter={4} learn_rate={5} k={6} init_mean={7} init_stdev={8}",
				num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, k == uint.MaxValue ? "inf" : k.ToString(), InitMean, InitStdev
			);
		}

	}
}

