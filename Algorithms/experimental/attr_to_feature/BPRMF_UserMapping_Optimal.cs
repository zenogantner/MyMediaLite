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
using MyMediaLite.data_type;
using MyMediaLite.item_recommender;


namespace MyMediaLite.experimental.attr_to_feature
{
	public class BPRMF_UserMapping_Optimal : BPRMF_UserMapping
	{

		// HACK make it protected after implementation is completed
		/// <inheritdoc />
		public override void LearnAttributeToFactorMapping()
		{
			// create attribute-to-feature weight matrix
			attribute_to_feature = new Matrix<double>(NumUserAttributes, num_features);

			Console.Error.WriteLine("\nBPR-OPT-USERMAP training");
			Console.Error.WriteLine("num_user_attributes=" + NumUserAttributes);

			MatrixUtils.InitNormal(attribute_to_feature, init_f_mean, init_f_stdev);

			//Console.Error.WriteLine("iteration -1 fit {0,0:0.#####} ", ComputeFit());
			for (int i = 0; i < num_iter_mapping; i++)
			{
				//Console.Error.WriteLine("before iteration {0} fit {1,0:0.#####} ", i, ComputeFit());
				iterate_mapping();
			}
		}

		public override void iterate_mapping()
		{

			int num_pos_events = data_user.NumberOfEntries;

			for (int i = 0; i < num_pos_events / 250; i++)
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateMappingFeatures(user_id, item_id_1, item_id_2);
			}
		}

		protected virtual void UpdateMappingFeatures(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			HashSet<int> attr_u = user_attributes[u];

			for (int f = 0; f < num_features; f++)
			{
				double diff = item_feature[i, f] - item_feature[j, f];

				foreach (int a in attr_u)
				{
					double m_af = attribute_to_feature[a, f];
					double update = diff / (1 + Math.Exp(x_uij)) - reg_mapping * m_af;
					attribute_to_feature[a, f] = m_af + learn_rate_mapping * update;
				}
			}
		}

		/// <inheritdoc />
		protected override double[] MapUserToLatentFeatureSpace(HashSet<int> user_attributes)
		{
			double[] feature_representation = new double[num_features];

			foreach (int i in user_attributes)
				for (int j = 0; j < num_features; j++)
					feature_representation[j] += attribute_to_feature[i, j];

			return feature_representation;
		}

		/// <inheritdoc />		
		public override string ToString()
		{
			return String.Format("BPR-MF-UserMapping-Optimal num_features={0}, reg_u={1}, reg_i={2}, reg_j={3}, num_iter={4}, learn_rate={5}, reg_mapping={6}, num_iter_mapping={7}, learn_rate_mapping={8}, init_f_mean={9}, init_f_stdev={10}",
				                 num_features, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, init_f_mean, init_f_stdev);
		}

	}
}