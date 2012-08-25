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
using System.Linq;
using MyMediaLite;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.AttrToFactor
{
	/// <summary>item attribute to latent factor mapping, optimized for BPR loss</summary>
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
	public class BPRMF_ItemMapping_Optimal : BPRMF_ItemMapping
	{
		///
		public override void LearnAttributeToFactorMapping()
		{
			this.factor_bias = new float[num_factors];

			for (int i = 0; i < num_factors; i++)
			{
				factor_bias[i] = MatrixExtensions.ColumnAverage(item_factors, i);
				Console.Error.WriteLine("fb {0}: {1}", i, factor_bias[i]);
			}

			this.attribute_to_factor = new Matrix<float>(NumItemAttributes, num_factors);
			MatrixExtensions.InitNormal(attribute_to_factor, InitMean, InitStdDev);

			for (int i = 0; i < num_iter_mapping; i++)
				IterateMapping();

			_MapToLatentFactorSpace = Utils.Memoize<int, float[]>(__MapToLatentFactorSpace);
		}

		///
		public override void IterateMapping()
		{
			_MapToLatentFactorSpace = __MapToLatentFactorSpace; // make sure we don't memoize during training

			for (int i = 0; i < Feedback.Count / 250; i++)
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateMappingFactors(user_id, item_id_1, item_id_2);
			}
		}

		/// <summary>update the mapping factors for a given user and an item pair</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the first item ID</param>
		/// <param name="j">the second item ID</param>
		protected virtual void UpdateMappingFactors(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			var attr_i = item_attributes[i];
			var attr_j = item_attributes[j];

			// assumption: attributes are sparse
			var attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			var attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			for (int f = 0; f < num_factors; f++)
			{
				double w_uf = user_factors[u, f];

				// update attribute-factor parameter for latent factors which are different between the items
				foreach (int a in attr_i_over_j)
				{
					float m_af = attribute_to_factor[a, f];
					double update = w_uf / (1 + Math.Exp(x_uij)) - reg_mapping * m_af;
					attribute_to_factor[a, f] = (float) (m_af + learn_rate_mapping * update);
				}
				foreach (int a in attr_j_over_i)
				{
					float m_af = attribute_to_factor[a, f];
					double update = -w_uf / (1 + Math.Exp(x_uij)) - reg_mapping * m_af;
					attribute_to_factor[a, f] = (float) (m_af + learn_rate_mapping * update);
				}
			}
		}

		///
		protected override float[] __MapToLatentFactorSpace(int item_id)
		{
			var factor_representation = new float[num_factors];

			for (int j = 0; j < num_factors; j++)
				factor_representation[j] = factor_bias[j];

			foreach (int i in item_attributes[item_id])
				for (int j = 0; j < num_factors; j++)
					factor_representation[j] += attribute_to_factor[i, j];

			return factor_representation;
		}

		///
		protected override float[] MapToLatentFactorSpace(int item_id)
		{
			return _MapToLatentFactorSpace(item_id);
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} reg_u={2} reg_i={3} reg_j={4} num_iter={5} learn_rate={6} reg_mapping={7} num_iter_mapping={8} learn_rate_mapping={9} init_mean={10} init_stddev={11}",
				this.GetType().Name, num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, InitMean, InitStdDev);
		}
	}
}