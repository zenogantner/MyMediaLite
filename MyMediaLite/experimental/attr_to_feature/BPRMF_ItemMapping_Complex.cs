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
	public class BPRMF_ItemMapping_Complex : BPRMF_ItemMapping
	{
		public int num_hidden_features = 80;
		protected Matrix<double> output_layer;


		public override void LearnAttributeToFactorMapping()
		{
			attribute_to_feature = new Matrix<double>(NumItemAttributes, num_hidden_features); // TODO: change name
			output_layer         = new Matrix<double>(num_hidden_features, num_features);

			Console.Error.WriteLine("BPR-MULTILAYER-MAP training");
			Console.Error.WriteLine("num_item_attributes=" + NumItemAttributes);
			Console.Error.WriteLine("num_hidden_features=" + num_hidden_features);

			MatrixUtils.InitNormal(attribute_to_feature, init_mean, init_stdev);
			MatrixUtils.InitNormal(output_layer, init_mean, init_stdev);

			//Console.Error.WriteLine("iteration -1 fit {0,0:0.#####} ", ComputeFit());
			for (int i = 0; i < num_iter_mapping; i++)
			{
				iterate_mapping();
				//ComputeMappingFit();
				//Console.Error.WriteLine("iteration {0} fit {1,0:0.#####} ", i, ComputeFit());
			}

			// set item_features to the mapped ones:                     // TODO: put into a separate method
			for (int item_id = 0; item_id < MaxItemID + 1; item_id++)
			{
				HashSet<int> attributes = item_attributes[item_id];

				// only map features for items where we know attributes
				if (attributes.Count == 0)
					continue;

				double[] est_features = MapToLatentFeatureSpace(item_id);

				item_feature.SetRow(item_id, est_features);
			}
		}

		public override void iterate_mapping()
		{
			Console.Error.Write(".");

			int num_pos_events = data_user.NumberOfEntries;

			for (int i = 0; i < num_pos_events / 50; i++)
			{
				int user_id, item_id_1, item_id_2;
				SampleTriple(out user_id, out item_id_1, out item_id_2);
				UpdateMappingFeatures(user_id, item_id_1, item_id_2);
			}
		}

		// TODO: ADD IN THRESHOLD FUNCTION!!
		protected virtual void UpdateMappingFeatures(int u, int i, int j)
		{
			double x_uij = Predict(u, i) - Predict(u, j);

			HashSet<int> attr_i = item_attributes[i];
			HashSet<int> attr_j = item_attributes[j];

			// assumption: attributes are sparse
			HashSet<int> attr_i_over_j = new HashSet<int>(attr_i);
			attr_i_over_j.ExceptWith(attr_j);
			HashSet<int> attr_j_over_i = new HashSet<int>(attr_j);
			attr_j_over_i.ExceptWith(attr_i);

			// update hidden layer - m1_AB
			for (int b = 0; b < num_hidden_features; b++)
			{
				foreach (int a in attr_i_over_j)
				{
					double sum = 0;
					for (int f = 0; f < num_features; f++)
					{
						double w_uf = user_feature[u, f];
						double m2_bf = output_layer[b, f];
						sum += w_uf * m2_bf;
					}
					double m1_ab = attribute_to_feature[a, b];
					double update = sum / (1 + Math.Exp(x_uij)) - reg_mapping * m1_ab;
					attribute_to_feature[a, b] = m1_ab + learn_rate_mapping * update;
				}

				foreach (int a in attr_j_over_i)
				{
					double sum = 0;
					for (int f = 0; f < num_features; f++)
					{
						double w_uf = user_feature[u, f];
						double m2_bf = output_layer[b, f];
						sum -= w_uf * m2_bf;
					}
					double m1_ab = attribute_to_feature[a, b];
					double update = sum / (1 + Math.Exp(x_uij)) - reg_mapping * m1_ab;
					attribute_to_feature[a, b] = m1_ab + learn_rate_mapping * update;
				}
			}

			// update output layer - m2_BF
			for (int b = 0; b < num_hidden_features; b++)
			{
				for (int f = 0; f < num_features; f++)
				{
					double w_uf = user_feature[u, f];

					double sum = 0;
					foreach (int a in attr_i_over_j)
					{
						double m1_ab = attribute_to_feature[a, b];
						sum += w_uf * m1_ab;
					}
					foreach (int a in attr_j_over_i)
					{
						double m1_ab = attribute_to_feature[a, b];
						sum -= w_uf * m1_ab;
					}
					double m2_bf = output_layer[b, f];
					double update = sum / (1 + Math.Exp(x_uij)) - reg_mapping * m2_bf;
					output_layer[b, f] = m2_bf + learn_rate_mapping * update;
				}
			}
		}

		protected override double[] MapToLatentFeatureSpace(int item_id)
		{
			HashSet<int> attributes = this.item_attributes[item_id];
			double[] feature_representation = new double[num_features];

			foreach (int i in attributes)
				for (int j = 0; j < num_features; j++)
					for (int k = 0; k < num_hidden_features; k++)
						feature_representation[j] += attribute_to_feature[i, k] * output_layer[k, j];
			// TODO: ADD IN THRESHOLD FUNCTION

			return feature_representation;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("BPR-MF-ItemMapping-Complex num_features={0}, reg_u={1}, reg_i={2}, reg_j={3}, num_iter={4}, learn_rate={5}, reg_mapping={6}, num_iter_mapping={7}, learn_rate_mapping={8}, num_hidden_features={9}, init_f_mean={9}, init_f_stdev={10}",
				                 num_features, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, num_hidden_features, init_mean, init_stdev);
		}

	}
}

