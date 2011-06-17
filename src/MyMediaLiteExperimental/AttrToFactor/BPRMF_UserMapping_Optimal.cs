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
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;

namespace MyMediaLite.AttrToFactor
{
	/// <summary>User attribute to latent factor mapping for BPR-MF, optimized for BPR loss</summary>
	public class BPRMF_UserMapping_Optimal : BPRMF_UserMapping
	{
		///
		public override void LearnAttributeToFactorMapping()
		{
			// create attribute-to-factor weight matrix
			attribute_to_factor = new Matrix<double>(NumUserAttributes, num_factors);

			Console.Error.WriteLine("\nBPR-OPT-USERMAP training");
			Console.Error.WriteLine("num_user_attributes=" + NumUserAttributes);

			MatrixUtils.RowInitNormal(attribute_to_factor, InitMean, InitStdev);

			for (int i = 0; i < num_iter_mapping; i++)
				IterateMapping();
		}

		///
		public override void IterateMapping()
		{

			int num_pos_events = Feedback.Count;

			for (int i = 0; i < num_pos_events / 250; i++)
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

			for (int f = 0; f < num_factors; f++)
			{
				double diff = item_factors[i, f] - item_factors[j, f];

				foreach (int a in user_attributes[u])
				{
					double m_af = attribute_to_factor[a, f];
					double update = diff / (1 + Math.Exp(x_uij)) - reg_mapping * m_af;
					attribute_to_factor[a, f] = m_af + learn_rate_mapping * update;
				}
			}
		}

		///
		protected override double[] MapUserToLatentFactorSpace(ICollection<int> user_attributes)
		{
			var factor_representation = new double[num_factors];

			foreach (int i in user_attributes)
				for (int j = 0; j < num_factors; j++)
					factor_representation[j] += attribute_to_factor[i, j];

			return factor_representation;
		}

		///
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(
				ni,
			    "BPRMF_UserMapping_Optimal num_factors={0} reg_u={1} reg_i={2} reg_j={3} num_iter={4} learn_rate={5} reg_mapping={6} num_iter_mapping={7} learn_rate_mapping={8} init_mean={9} init_stdev={10}",
				num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, InitMean, InitStdev
			);
		}

	}
}