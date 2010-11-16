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
using System.Globalization;
using MyMediaLite;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.item_recommender;
using MyMediaLite.util;


namespace MyMediaLite.experimental.attr_to_feature
{
	/// <summary>BPR-MF with item mapping learned by regularized least-squares regression</summary>
	public class BPRMF_ItemMapping : BPRMF_Mapping, IItemAttributeAwareRecommender
	{
		/// <inheritdoc/>
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set
			{
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		/// <summary>The matrix storing the item attributes</summary>
		protected SparseBooleanMatrix item_attributes;

		/// <inheritdoc/>
	    public int NumItemAttributes { get;	set; }

		/// <summary>array to store the bias for each mapping</summary>
		protected double[] feature_bias;

		/// <inheritdoc/>
		public override void LearnAttributeToFactorMapping()
		{
			// create attribute-to-feature weight matrix
			attribute_to_feature = new Matrix<double>(NumItemAttributes + 1, num_factors);
			// store the results of the different runs in the following array
			Matrix<double>[] old_attribute_to_feature = new Matrix<double>[num_init_mapping];

			Console.Error.WriteLine("Will use {0} examples ...", num_iter_mapping * MaxItemID);

			double[][] old_rmse_per_feature = new double[num_init_mapping][];

			for (int h = 0; h < num_init_mapping; h++)
			{
				MatrixUtils.InitNormal(attribute_to_feature, init_mean, init_stdev);
				Console.Error.WriteLine("----");

				for (int i = 0; i < num_iter_mapping * MaxItemID; i++)
					IterateMapping();
				old_attribute_to_feature[h] = new Matrix<double>(attribute_to_feature);
				old_rmse_per_feature[h] = ComputeMappingFit();
			}

			double[] min_rmse_per_feature = new double[num_factors];
			for (int i = 0; i < num_factors; i++)
				min_rmse_per_feature[i] = Double.MaxValue;
			int[] best_feature_init       = new int[num_factors];

			// find best feature mappings
			for (int i = 0; i < num_init_mapping; i++)
				for (int j = 0; j < num_factors; j++)
					if (old_rmse_per_feature[i][j] < min_rmse_per_feature[j])
					{
						min_rmse_per_feature[j] = old_rmse_per_feature[i][j];
						best_feature_init[j]   = i;
					}

			// set the best weight combinations for each feature mapping
			for (int i = 0; i < num_factors; i++)
			{
				Console.Error.WriteLine("Feature {0}, pick {1}", i, best_feature_init[i]);

				attribute_to_feature.SetColumn(i,
					old_attribute_to_feature[best_feature_init[i]].GetColumn(i)
				);
			}

			_MapToLatentFeatureSpace = Utils.Memoize<int, double[]>(__MapToLatentFeatureSpace);
		}

		/// <summary>
		/// Samples an item for the mapping training.
		/// Only items that are associated with at least one user are taken into account.
		/// </summary>
		/// <returns>the item ID</returns>
		protected int SampleItem()
		{
			while (true)
			{
				int item_id = random.Next(0, MaxItemID + 1);
				HashSet<int> item_users = data_item[item_id];
				if (item_users.Count == 0)
					continue;
				return item_id;
			}
		}

		/// <summary>
		/// Perform one iteration of the mapping learning process
		/// </summary>
		public override void IterateMapping()
		{
			_MapToLatentFeatureSpace = __MapToLatentFeatureSpace; // make sure we don't memoize during training

			// stochastic gradient descent
			int item_id = SampleItem();

			double[] est_features = MapToLatentFeatureSpace(item_id);

			for (int j = 0; j < num_factors; j++) {
				// TODO do we need an absolute term here???
				double diff = est_features[j] - item_factors[item_id, j];
				if (diff > 0)
				{
					foreach (int attribute in item_attributes[item_id])
					{
						double w = attribute_to_feature[attribute, j];
						double deriv = diff * w + reg_mapping * w;
						MatrixUtils.Inc(attribute_to_feature, attribute, j, learn_rate_mapping * -deriv);
					}
					// bias term
					double w_bias = attribute_to_feature[NumItemAttributes, j];
					double deriv_bias = diff * w_bias + reg_mapping * w_bias;
					MatrixUtils.Inc(attribute_to_feature, NumItemAttributes, j, learn_rate_mapping * -deriv_bias);
				}
			}
		}

		/// <summary>
		/// Compute the fit of the mapping
		/// </summary>
		/// <returns>
		/// an array of doubles containing the RMSE on the training data for each latent factor
		/// </returns>
		protected double[] ComputeMappingFit()
		{
			double rmse    = 0;
			double penalty = 0;
			double[] rmse_and_penalty_per_feature = new double[num_factors];

			int num_items = 0;
			for (int i = 0; i < MaxItemID + 1; i++)
			{
				HashSet<int> item_users = data_item[i];
				HashSet<int> item_attrs = item_attributes[i];
				if (item_users.Count == 0 || item_attrs.Count == 0)
					continue;

				num_items++;

				double[] est_features = MapToLatentFeatureSpace(i);
				for (int j = 0; j < num_factors; j++)
				{
					double error    = Math.Pow(est_features[j] - item_factors[i, j], 2);
					double reg_term = reg_mapping * VectorUtils.EuclideanNorm(attribute_to_feature.GetColumn(j));
					rmse    += error;
					penalty += reg_term;
					rmse_and_penalty_per_feature[j] += error + reg_term;
				}
			}

			for (int i = 0; i < num_factors; i++)
			{
				rmse_and_penalty_per_feature[i] = (double) rmse_and_penalty_per_feature[i] / num_items;
				Console.Error.Write("{0,0:0.####} ", rmse_and_penalty_per_feature[i]);
			}
			rmse    = (double) rmse    / (num_factors * num_items);
			penalty = (double) penalty / (num_factors * num_items);
			Console.Error.WriteLine(" > {0,0:0.####} ({1,0:0.####})", rmse, penalty);

			return rmse_and_penalty_per_feature;
		}

		protected Func<int, double[]> _MapToLatentFeatureSpace;

		protected virtual double[] MapToLatentFeatureSpace(int item_id)
		{
			return _MapToLatentFeatureSpace(item_id);
		}


		protected virtual double[] __MapToLatentFeatureSpace(int item_id)
		{
			HashSet<int> item_attributes = this.item_attributes[item_id];

			double[] feature_representation = new double[num_factors];
			for (int j = 0; j < num_factors; j++)
				// bias
				feature_representation[j] = attribute_to_feature[NumItemAttributes, j];

			foreach (int i in item_attributes)
				for (int j = 0; j < num_factors; j++)
					feature_representation[j] += attribute_to_feature[i, j];

			return feature_representation;
		}

        /// <inheritdoc/>
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id >= user_factors.dim1))
            {
                Console.Error.WriteLine("user is unknown: " + user_id);
				return double.MinValue;
            }

			double[] est_features = MapToLatentFeatureSpace(item_id);
            return MatrixUtils.RowScalarProduct(user_factors, user_id, est_features);
        }

		/// <inheritdoc/>
		public override string ToString()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(
				ni,
			    "BPR-MF-ItemMapping num_features={0}, reg_u={1}, reg_i={2}, reg_j={3}, num_iter={4}, learn_rate={5}, reg_mapping={6}, num_iter_mapping={7}, learn_rate_mapping={8}, init_mean={9}, init_stdev={10}",
				num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, init_mean, init_stdev
			);
		}

	}
}

