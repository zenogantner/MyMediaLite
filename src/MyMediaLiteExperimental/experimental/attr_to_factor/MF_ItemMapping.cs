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


namespace MyMediaLite.experimental.attr_to_factor
{
	/// <summary>biased MF with item mapping learned by regularized least-squares regression</summary>
	public class MF_ItemMapping : MF_Mapping, IItemAttributeAwareRecommender
	{
		/// <summary>Who has rated what? item-wise</summary>
        protected SparseBooleanMatrix data_item;

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
		protected double[] factor_bias;

		/// <summary>random number generator</summary>
		protected System.Random random;

		/// <inheritdoc/>
		public override void LearnAttributeToFactorMapping()
		{
			random = util.Random.GetInstance();

			// create helper data structure
			data_item = new SparseBooleanMatrix();
			foreach (RatingEvent r in Ratings.All)
				data_item[r.item_id, r.user_id] = true;

			// create attribute-to-factor weight matrix
			attribute_to_factor = new Matrix<double>(NumItemAttributes + 1, num_factors + 1);
			// account for regression bias term, and the item bias that we want to model

			// store the results of the different runs in the following array
			Matrix<double>[] old_attribute_to_factor = new Matrix<double>[num_init_mapping];

			Console.Error.WriteLine("Will use {0} examples ...", num_iter_mapping * MaxItemID);

			double[][] old_rmse_per_factor = new double[num_init_mapping][];

			for (int h = 0; h < num_init_mapping; h++)
			{
				MatrixUtils.InitNormal(attribute_to_factor, InitMean, InitStdev);
				Console.Error.WriteLine("----");

				for (int i = 0; i < num_iter_mapping * MaxItemID; i++)
					IterateMapping();
				old_attribute_to_factor[h] = new Matrix<double>(attribute_to_factor);
				old_rmse_per_factor[h] = ComputeMappingFit();
			}

			double[] min_rmse_per_factor = new double[num_factors + 1];
			for (int i = 0; i <= num_factors; i++)
				min_rmse_per_factor[i] = Double.MaxValue;
			int[] best_factor_init       = new int[num_factors + 1];

			// find best factor mappings:
			for (int i = 0; i < num_init_mapping; i++)
				for (int j = 0; j <= num_factors; j++)
					if (old_rmse_per_factor[i][j] < min_rmse_per_factor[j])
					{
						min_rmse_per_factor[j] = old_rmse_per_factor[i][j];
						best_factor_init[j]    = i;
					}

			// set the best weight combinations for each factor mapping
			for (int i = 0; i <= num_factors; i++)
			{
				Console.Error.WriteLine("Factor {0}, pick {1}", i, best_factor_init[i]);

				attribute_to_factor.SetColumn(i,
					old_attribute_to_factor[best_factor_init[i]].GetColumn(i)
				);
			}
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

		/// <summary>Perform one iteration of the mapping learning process</summary>
		public override void IterateMapping()
		{
			// stochastic gradient descent
			int item_id = SampleItem();

			double[] est_factors = MapToLatentFactorSpace(item_id);

			for (int j = 0; j < num_factors; j++) {
				// TODO do we need an absolute term here???
				double diff = est_factors[j] - item_factors[item_id, j];
				if (diff > 0)
				{
					foreach (int attribute in item_attributes[item_id])
					{
						double w = attribute_to_factor[attribute, j];
						double deriv = diff * w + reg_mapping * w;
						MatrixUtils.Inc(attribute_to_factor, attribute, j, learn_rate_mapping * -deriv);
					}
					// bias term
					double w_bias = attribute_to_factor[NumItemAttributes, j];
					double deriv_bias = diff * w_bias + reg_mapping * w_bias;
					MatrixUtils.Inc(attribute_to_factor, NumItemAttributes, j, learn_rate_mapping * -deriv_bias);
				}
			}

			// item bias part
			double bias_diff = est_factors[num_factors] - item_bias[item_id];
			if (bias_diff > 0)
			{
				foreach (int attribute in item_attributes[item_id])
				{
					double w = attribute_to_factor[attribute, num_factors];
					double deriv = bias_diff * w + reg_mapping * w;
					MatrixUtils.Inc(attribute_to_factor, attribute, num_factors, learn_rate_mapping * -deriv);
				}
				// bias term
				double w_bias = attribute_to_factor[NumItemAttributes, num_factors];
				double deriv_bias = bias_diff * w_bias + reg_mapping * w_bias;
				MatrixUtils.Inc(attribute_to_factor, NumItemAttributes, num_factors, learn_rate_mapping * -deriv_bias);
			}
		}

		/// <summary>Compute the fit of the mapping</summary>
		/// <returns>an array of doubles containing the RMSE on the training data for each latent factor</returns>
		protected double[] ComputeMappingFit()
		{
			Console.Error.WriteLine("min = {0}, max = {1}", MinRating, MaxRating);
			
			double rmse    = 0;
			double penalty = 0;
			double[] rmse_and_penalty_per_factor = new double[num_factors + 1];

			int num_items = 0;
			for (int i = 0; i < MaxItemID + 1; i++)
			{
				HashSet<int> item_users = data_item[i];
				HashSet<int> item_attrs = item_attributes[i];
				if (item_users.Count == 0 || item_attrs.Count == 0)
					continue;

				num_items++;

				double[] est_factors = MapToLatentFactorSpace(i);
				double error, reg_term;
				for (int j = 0; j < num_factors; j++)
				{
					error    = Math.Pow(est_factors[j] - item_factors[i, j], 2);
					reg_term = reg_mapping * VectorUtils.EuclideanNorm(attribute_to_factor.GetColumn(j));
					rmse    += error;
					penalty += reg_term;
					rmse_and_penalty_per_factor[j] += error + reg_term;
				}
				
				// error term for item bias term
				error    = Math.Pow(est_factors[num_factors] - item_bias[i], 2);
				reg_term = reg_mapping * Math.Pow(VectorUtils.EuclideanNorm(attribute_to_factor.GetColumn(num_factors)), 2);
				rmse    += error;
				penalty += reg_term;
				rmse_and_penalty_per_factor[num_factors] += error + reg_term;
			}

			for (int i = 0; i <= num_factors; i++)
			{
				rmse_and_penalty_per_factor[i] = (double) rmse_and_penalty_per_factor[i] / num_items;
				Console.Error.Write("{0,0:0.####} ", rmse_and_penalty_per_factor[i]);
			}
			rmse    = (double) rmse    / ((num_factors + 1) * num_items);
			penalty = (double) penalty / ((num_factors + 1) * num_items);
			Console.Error.WriteLine(" > {0,0:0.####} ({1,0:0.####})", rmse, penalty);

			return rmse_and_penalty_per_factor;
		}

		protected virtual double[] MapToLatentFactorSpace(int item_id)
		{
			double[] factor_representation = new double[num_factors + 1];

			// regression bias
			for (int j = 0; j <= num_factors; j++)
				factor_representation[j] = attribute_to_factor[NumItemAttributes, j];

			// estimate latent factors
			foreach (int i in item_attributes[item_id])
				for (int j = 0; j <= num_factors; j++)
					factor_representation[j] += attribute_to_factor[i, j];

			return factor_representation;
		}

        /// <inheritdoc/>
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id >= user_factors.dim1))
            {
                Console.Error.WriteLine("user is unknown: " + user_id);
				return double.MinValue;
            }

			double[] est_factors = MapToLatentFactorSpace(item_id);
			double[] latent_factors = new double[num_factors];

			Array.Copy(est_factors, latent_factors, num_factors);

            double score =
				MatrixUtils.RowScalarProduct(user_factors, user_id, latent_factors)
				+ user_bias[user_id]
				+ est_factors[num_factors] // estimated item bias
				+ global_bias;
			
			return MinRating + ( 1 / (1 + Math.Exp(-score)) ) * (MaxRating - MinRating);			
        }

		/// <inheritdoc/>
		public override string ToString()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(
				ni,
			    "MF-ItemMapping num_factors={0}, regularization={1}, num_iter={2}, learn_rate={3}, reg_mapping={4}, num_iter_mapping={5}, learn_rate_mapping={6}, init_mean={7}, init_stdev={8}",
				num_factors, Regularization, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, InitMean, InitStdev
			);
		}

	}
}

