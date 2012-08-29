// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using MyMediaLite;
using MyMediaLite.Correlation;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.AttrToFactor
{
	/// <summary>BPR-MF with item mapping learned by kNN</summary>
	/// <remarks>
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///     Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Steffen Rendle, Lars Schmidt-Thieme:
	///     Learning Attribute-to-Feature Mappings for Cold-Start Recommendations.
	///     ICDM 2011.
	///     http://www.ismll.uni-hildesheim.de/pub/pdfs/Gantner_et_al2010Mapping.pdf
	///   </description></item>
	/// </list>
	///
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class BPRMF_ItemMappingKNN : BPRMF_ItemMapping
	{
		/// <summary>Number of neighbors to be used for mapping</summary>
		public uint K { get { return k; } set { k = value; } }
		uint k = uint.MaxValue;

		SymmetricCorrelationMatrix item_correlation;

		///
		public override void LearnAttributeToFactorMapping()
		{
			BinaryCosine cosine_correlation = new BinaryCosine(MaxItemID + 1);
			Console.Error.WriteLine("training with max_item_id={0}", MaxItemID);
			cosine_correlation.ComputeCorrelations(item_attributes);
			this.item_correlation = cosine_correlation;
			_MapToLatentFactorSpace = Utils.Memoize<int, float[]>(__MapToLatentFactorSpace);
		}

		/// <summary>map to latent factor space (actual function)</summary>
		protected override float[] __MapToLatentFactorSpace(int item_id)
		{
			var est_factors = new float[num_factors];

			IList<int> relevant_items = item_correlation.GetPositivelyCorrelatedEntities(item_id);

			float weight_sum = 0;
			uint neighbors =  k;
			foreach (int item_id2 in relevant_items)
			{
				if (item_id2 >= item_factors.dim1) // check whether item is in training data
					continue;
				if (Feedback.ItemMatrix[item_id2].Count == 0)
					continue;

				float weight = item_correlation[item_id, item_id2];
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
				"{0} num_factors={1} reg_u={2} reg_i={3} reg_j={4} num_iter={5} learn_rate={6} k={7} init_mean={8} init_stddev={9}",
				this.GetType().Name, num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, k == uint.MaxValue ? "inf" : k.ToString(), InitMean, InitStdDev
			);
		}

	}
}

