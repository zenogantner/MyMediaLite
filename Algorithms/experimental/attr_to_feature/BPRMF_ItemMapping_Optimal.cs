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
using System.Linq;
using MyMediaLite;
using MyMediaLite.data_type;
using MyMediaLite.item_recommender;
using MyMediaLite.util;


namespace MyMediaLite.experimental.attr_to_feature
{
	public class BPRMF_ItemMapping_Optimal : BPRMF_ItemMapping
	{
		/// <inheritdoc />
		public override void LearnAttributeToFactorMapping()
		{
			this.feature_bias = new double[num_features];
			if (this.mapping_feature_bias)
				for (int i = 0; i < num_features; i++)
				{
					feature_bias[i] = MatrixUtils.ColumnAverage(item_feature, i);
					Console.Error.WriteLine("fb {0}: {1}", i, feature_bias[i]);
				}


			attribute_to_feature = new Matrix<double>(NumItemAttributes, num_features);
			MatrixUtils.InitNormal(attribute_to_feature, init_f_mean, init_f_stdev);

			for (int i = 0; i < num_iter_mapping; i++)
				iterate_mapping();

			_MapToLatentFeatureSpace = Utils.Memoize<int, double[]>(__MapToLatentFeatureSpace);
		}

		/// <inheritdoc />
		public override void iterate_mapping()
		{
			_MapToLatentFeatureSpace = __MapToLatentFeatureSpace; // make sure we don't memoize during training

			int num_pos_events = data_user.GetNumberOfEntries();

			for (int i = 0; i < num_pos_events / 250; i++) // TODO: think about using another number here ...
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateMappingFeatures(user_id, item_id_1, item_id_2);
			}
		}

		protected virtual void UpdateMappingFeatures(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			HashSet<int> attr_i = item_attributes.GetAttributes(i);
			HashSet<int> attr_j = item_attributes.GetAttributes(j);

			// assumption: attributes are sparse
			HashSet<int> attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			HashSet<int> attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			for (int f = 0; f < num_features; f++)
			{
				double w_uf = user_feature.Get(u, f);

				// update attribute-feature parameter for features which are different between the items
				foreach (int a in attr_i_over_j)
				{
					double m_af = attribute_to_feature.Get(a, f);
					double update = w_uf / (1 + Math.Exp(x_uij)) - reg_mapping * m_af;
					attribute_to_feature.Set(a, f, m_af + learn_rate_mapping * update);
				}
				foreach (int a in attr_j_over_i)
				{
					double m_af = attribute_to_feature.Get(a, f);
					double update = -w_uf / (1 + Math.Exp(x_uij)) - reg_mapping * m_af;
					attribute_to_feature.Set(a, f, m_af + learn_rate_mapping * update);
				}
			}
		}

		/// <inheritdoc />
		protected override double[] __MapToLatentFeatureSpace(int item_id)
		{
			HashSet<int> attributes = this.item_attributes.GetAttributes(item_id);
			double[] feature_representation = new double[num_features];

			if (this.mapping_feature_bias)
				for (int j = 0; j < num_features; j++)
					feature_representation[j] = feature_bias[j];

			foreach (int i in attributes)
				for (int j = 0; j < num_features; j++)
					feature_representation[j] += attribute_to_feature.Get(i, j);

			return feature_representation;
		}

		/// <inheritdoc />
		protected override double[] MapToLatentFeatureSpace(int item_id)
		{
			return _MapToLatentFeatureSpace(item_id);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("BPR-MF-ItemMapping-Optimal num_features={0}, reg_u={1}, reg_i={2}, reg_j={3}, num_iter={4}, learn_rate={5}, reg_mapping={6}, num_iter_mapping={7}, learn_rate_mapping={8}, mapping_feature_bias={9}, init_f_mean={10}, init_f_stdev={11}",
				                 num_features, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, mapping_feature_bias, init_f_mean, init_f_stdev);
		}

	}
}

