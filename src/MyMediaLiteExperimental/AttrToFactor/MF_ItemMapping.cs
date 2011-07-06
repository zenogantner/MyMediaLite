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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.Util;

namespace MyMediaLite.AttrToFactor
{
	/// <summary>biased MF with item mapping learned by regularized least-squares regression</summary>
	public class MF_ItemMapping : MF_Mapping, IItemAttributeAwareRecommender
	{
		/// <summary>Who has rated what? item-wise</summary>
        protected SparseBooleanMatrix data_item;

		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set	{
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		/// <summary>The matrix storing the item attributes</summary>
		protected SparseBooleanMatrix item_attributes;

		///
	    public int NumItemAttributes { get;	set; }

		/// <summary>array to store the bias for each mapping</summary>
		protected double[] factor_bias;

		/// <summary>random number generator</summary>
		protected System.Random random;

		///
		public override void LearnAttributeToFactorMapping()
		{
			random = Util.Random.GetInstance();

			// create helper data structure
			this.data_item = new SparseBooleanMatrix();
			for (int i = 0; i < ratings.Count; i++)
				data_item[ratings.Items[i], ratings.Users[i]] = true;

			// create attribute-to-factor weight matrix
			this.attribute_to_factor = new Matrix<double>(NumItemAttributes + 1, NumFactors + 1);
			// account for regression bias term, and the item bias that we want to model

			// store the results of the different runs in the following array
			var old_attribute_to_factor = new Matrix<double>[num_init_mapping];

			Console.Error.WriteLine("Will use {0} examples ...", num_iter_mapping * MaxItemID);

			var old_rmse_per_factor = new double[num_init_mapping][];

			for (int h = 0; h < num_init_mapping; h++)
			{
				MatrixUtils.InitNormal(attribute_to_factor, InitMean, InitStdev);
				Console.Error.WriteLine("----");

				for (int i = 0; i < num_iter_mapping * MaxItemID; i++)
					IterateMapping();
				old_attribute_to_factor[h] = new Matrix<double>(attribute_to_factor);
				old_rmse_per_factor[h] = ComputeMappingFit();
			}

			var min_rmse_per_factor = new double[NumFactors + 1];
			for (int i = 0; i <= NumFactors; i++)
				min_rmse_per_factor[i] = Double.MaxValue;
			var best_factor_init       = new int[NumFactors + 1];

			// find best factor mappings:
			for (int i = 0; i < num_init_mapping; i++)
				for (int j = 0; j <= NumFactors; j++)
					if (old_rmse_per_factor[i][j] < min_rmse_per_factor[j])
					{
						min_rmse_per_factor[j] = old_rmse_per_factor[i][j];
						best_factor_init[j]    = i;
					}

			// set the best weight combinations for each factor mapping
			for (int i = 0; i <= NumFactors; i++)
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

				if (data_item[item_id].Count == 0)
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

			for (int j = 0; j < NumFactors; j++) {
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
			double bias_diff = est_factors[NumFactors] - item_bias[item_id];
			if (bias_diff > 0)
			{
				foreach (int attribute in item_attributes[item_id])
				{
					double w = attribute_to_factor[attribute, (int) NumFactors];
					double deriv = bias_diff * w + reg_mapping * w;
					MatrixUtils.Inc(attribute_to_factor, attribute, (int) NumFactors, learn_rate_mapping * -deriv);
				}
				// bias term
				double w_bias = attribute_to_factor[NumItemAttributes, (int) NumFactors];
				double deriv_bias = bias_diff * w_bias + reg_mapping * w_bias;
				MatrixUtils.Inc(attribute_to_factor, NumItemAttributes, (int) NumFactors, learn_rate_mapping * -deriv_bias);
			}
		}

		/// <summary>Compute the fit of the mapping</summary>
		/// <returns>an array of doubles containing the RMSE on the training data for each latent factor</returns>
		protected double[] ComputeMappingFit()
		{
			Console.Error.WriteLine("min = {0}, max = {1}", MinRating, MaxRating);

			double rmse    = 0;
			double penalty = 0;
			var rmse_and_penalty_per_factor = new double[NumFactors + 1];

			int num_items = 0;
			for (int i = 0; i < MaxItemID + 1; i++)
			{
				if (data_item[i].Count == 0 || item_attributes[i].Count == 0)
					continue;

				num_items++;

				double[] est_factors = MapToLatentFactorSpace(i);
				double error, reg_term;
				for (int j = 0; j < NumFactors; j++)
				{
					error    = Math.Pow(est_factors[j] - item_factors[i, j], 2);
					reg_term = reg_mapping * VectorUtils.EuclideanNorm(attribute_to_factor.GetColumn(j));
					rmse    += error;
					penalty += reg_term;
					rmse_and_penalty_per_factor[j] += error + reg_term;
				}

				// error term for item bias term
				error    = Math.Pow(est_factors[NumFactors] - item_bias[i], 2);
				reg_term = reg_mapping * Math.Pow(VectorUtils.EuclideanNorm(attribute_to_factor.GetColumn((int) NumFactors)), 2);
				rmse    += error;
				penalty += reg_term;
				rmse_and_penalty_per_factor[NumFactors] += error + reg_term;
			}

			for (int i = 0; i <= NumFactors; i++)
			{
				rmse_and_penalty_per_factor[i] = (double) rmse_and_penalty_per_factor[i] / num_items;
				Console.Error.Write("{0,0:0.####} ", rmse_and_penalty_per_factor[i]);
			}
			rmse    = (double) rmse    / ((NumFactors + 1) * num_items);
			penalty = (double) penalty / ((NumFactors + 1) * num_items);
			Console.Error.WriteLine(" > {0,0:0.####} ({1,0:0.####})", rmse, penalty);

			return rmse_and_penalty_per_factor;
		}


		/// <summary>map to latent factor space</summary>
		protected virtual double[] MapToLatentFactorSpace(int item_id)
		{
			var factor_representation = new double[NumFactors + 1];

			// regression bias
			for (int j = 0; j <= NumFactors; j++)
				factor_representation[j] = attribute_to_factor[NumItemAttributes, j];

			// estimate latent factors
			foreach (int i in item_attributes[item_id])
				for (int j = 0; j <= NumFactors; j++)
					factor_representation[j] += attribute_to_factor[i, j];

			return factor_representation;
		}

        ///
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id >= user_factors.dim1))
            {
                Console.Error.WriteLine("user is unknown: " + user_id);
				return double.MinValue;
            }

			double[] est_factors = MapToLatentFactorSpace(item_id);
			var latent_factors = new double[NumFactors];

			Array.Copy(est_factors, latent_factors, NumFactors);

            double score =
				MatrixUtils.RowScalarProduct(user_factors, user_id, latent_factors)
				+ user_bias[user_id]
				+ est_factors[NumFactors] // estimated item bias
				+ global_bias;

			return MinRating + ( 1 / (1 + Math.Exp(-score)) ) * (MaxRating - MinRating);
        }

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
			    "{0} NumFactors={1} regularization={2} num_iter={3} learn_rate={4} reg_mapping={5} num_iter_mapping={6} learn_rate_mapping={7} init_mean={8} init_stdev={9}",
				this.GetType().Name, NumFactors, Regularization, NumIter, LearnRate, reg_mapping, num_iter_mapping, learn_rate_mapping, InitMean, InitStdev
			);
		}
	}
}

