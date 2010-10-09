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
using System.Diagnostics;
using MyMediaLite;
using MyMediaLite.correlation;
using MyMediaLite.item_recommender;
using MyMediaLite.util;


namespace MyMediaLite.experimental.attr_to_feature
{
	// TODO: store model (for debugging)

	public class BPRMF_ItemMapping_kNN : BPRMF_ItemMapping
	{
		public uint k = uint.MaxValue;

		protected CorrelationMatrix item_correlation;


		// HACK: make it protected after implementation is completed
		public override void LearnAttributeToFactorMapping()
		{
			Cosine cosine_correlation = new Cosine(MaxItemID + 1);
			cosine_correlation.ComputeCorrelations(item_attributes);
			this.item_correlation = cosine_correlation;
			_MapToLatentFeatureSpace = Utils.Memoize<int, double[]>(__MapToLatentFeatureSpace);
		}

		protected override double[] __MapToLatentFeatureSpace(int item_id)
		{
			double[] item_features = new double[num_features];

			IList<int> relevant_items = item_correlation.GetPositivelyCorrelatedEntities(item_id);

			double weight_sum = 0;
			uint neighbors =  k;
			foreach (int item_id2 in relevant_items)
			{
				if (item_id2 >= item_feature.dim1) // check whether item is in training data
					continue;
				if (data_item[item_id2].Count == 0)
					continue;

				double weight = item_correlation.Get(item_id, item_id2);
				weight_sum += weight;
				for (int f = 0; f < num_features; f++)
					item_features[f] += weight * item_feature[item_id2, f];

				if (--neighbors == 0)
					break;
			}

			for (int f = 0; f < num_features; f++)
				item_features[f] /= weight_sum;

			return item_features;

		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return String.Format("BPR-MF-ItemMapping-kNN num_features={0}, reg_u={1}, reg_i={2}, reg_j={3}, num_iter={4}, learn_rate={5}, k={6}, init_f_mean={7}, init_f_stdev={8}",
				                 num_features, reg_u, reg_i, reg_j, NumIter, learn_rate, k == UInt32.MaxValue ? "inf" : k.ToString(), init_f_mean, init_f_stdev);
		}

	}
}

