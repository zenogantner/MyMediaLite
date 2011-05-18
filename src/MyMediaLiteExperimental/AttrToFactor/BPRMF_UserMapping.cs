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

namespace MyMediaLite.AttrToFactor
{
	/// <summary>User attribute to latent factor mapping for BPR-MF, optimized for RMSE on the latent factors</summary>
	public class BPRMF_UserMapping : BPRMF_Mapping, IUserAttributeAwareRecommender
	{
		/// <inheritdoc/>
		public SparseBooleanMatrix UserAttributes
		{
			get { return this.user_attributes; }
			set	{
				this.user_attributes = value;
				this.NumUserAttributes = user_attributes.NumberOfColumns;
				this.MaxUserID = Math.Max(MaxUserID, user_attributes.NumberOfRows - 1);
			}
		}
		/// <summary>The matrix storing the user attributes</summary>
		protected SparseBooleanMatrix user_attributes;

		/// <inheritdoc/>
		public int NumUserAttributes { get; set; }

		/// <summary>
		/// Samples an user for the mapping training.
		/// Only users that are associated with at least one item, and that actually have attributes,
		/// are taken into account.
		/// </summary>
		/// <returns>the user ID</returns>
		protected int SampleUserWithAttributes()
		{
			while (true)
			{
				int user_id = random.Next(0, MaxUserID + 1);
				HashSet<int> user_items = Feedback.UserMatrix[user_id];
				HashSet<int> user_attrs = user_attributes[user_id];
				if (user_items.Count == 0 || user_attrs.Count == 0)
					continue;
				return user_id;
			}
		}

		/// <inheritdoc/>
		public override void LearnAttributeToFactorMapping()
		{
			// create attribute-to-factor weight matrix
			this.attribute_to_factor = new Matrix<double>(NumUserAttributes + 1, num_factors);
			Console.Error.WriteLine("num_user_attributes=" + NumUserAttributes);
			// store the results of the different runs in the following array
			var old_attribute_to_factor = new Matrix<double>[num_init_mapping];

			Console.Error.WriteLine("Will use {0} examples ...", num_iter_mapping * MaxUserID);

			var old_rmse_per_factor = new double[num_init_mapping][];

			for (int h = 0; h < num_init_mapping; h++)
			{
				MatrixUtils.InitNormal(attribute_to_factor, InitMean, InitStdev);
				Console.Error.WriteLine("----");

				for (int i = 0; i < num_iter_mapping * MaxUserID; i++)
				{
					if (i % 5000 == 0)
						ComputeMappingFit();
					IterateMapping();
				}
				ComputeMappingFit();

				old_attribute_to_factor[h] = new Matrix<double>(attribute_to_factor);
				old_rmse_per_factor[h] = ComputeMappingFit();
			}

			var min_rmse_per_factor = new double[num_factors];
			for (int i = 0; i < num_factors; i++)
				min_rmse_per_factor[i] = double.MaxValue;
			var best_factor_init = new int[num_factors];

			// find best factor mappings:
			for (int i = 0; i < num_init_mapping; i++)
			{
				for (int j = 0; j < num_factors; j++)
				{
					if (old_rmse_per_factor[i][j] < min_rmse_per_factor[j])
					{
						min_rmse_per_factor[j] = old_rmse_per_factor[i][j];
						best_factor_init[j]    = i;
					}
				}
			}

			// set the best weight combinations for each factor mapping
			for (int i = 0; i < num_factors; i++)
			{
				Console.Error.WriteLine("Factor {0}, pick {1}", i, best_factor_init[i]);

				attribute_to_factor.SetColumn(i,
					old_attribute_to_factor[best_factor_init[i]].GetColumn(i)
				);
			}

			Console.Error.WriteLine("----");
			ComputeMappingFit();
		}

		/// <inheritdoc/>
		public override void IterateMapping()
		{
			// stochastic gradient descent
			int user_id = SampleUserWithAttributes();

			HashSet<int> attributes = user_attributes[user_id];
			double[] est_factors = MapUserToLatentFactorSpace(attributes);

			for (int j = 0; j < num_factors; j++)
			{
				// TODO do we need an absolute term here???
				double diff = est_factors[j] - user_factors[user_id, j];
				if (diff > 0)
				{
					foreach (int attribute in attributes)
					{
						double w = attribute_to_factor[attribute, j];
						double deriv = diff * w + reg_mapping * w;
						MatrixUtils.Inc(attribute_to_factor, attribute, j, learn_rate_mapping * -deriv);
					}
					// bias term
					double w_bias = attribute_to_factor[NumUserAttributes, j];
					double deriv_bias = diff * w_bias + reg_mapping * w_bias;
					MatrixUtils.Inc(attribute_to_factor, NumUserAttributes, j, learn_rate_mapping * -deriv_bias);
				}
			}
		}

		/// <inheritdoc/>
		protected double[] ComputeMappingFit()
		{
			double rmse    = 0;
			double penalty = 0;
			double[] rmse_and_penalty_per_factor = new double[num_factors];

			int num_users = 0;
			for (int i = 0; i <= MaxUserID; i++)
			{
				HashSet<int> user_items = Feedback.UserMatrix[i];
				HashSet<int> user_attrs = user_attributes[i];
				if (user_items.Count == 0 || user_attrs.Count == 0)
					continue;

				num_users++;

				HashSet<int> attributes = user_attributes[i];
				double[] est_factors = MapUserToLatentFactorSpace(attributes);
				for (int j = 0; j < num_factors; j++)
				{
					double error    = Math.Pow(est_factors[j] - user_factors[i, j], 2);
					double reg_term = reg_mapping * VectorUtils.EuclideanNorm(attribute_to_factor.GetColumn(j));
					rmse    += error;
					penalty += reg_term;
					rmse_and_penalty_per_factor[j] += error + reg_term;
				}
			}

			for (int i = 0; i < num_factors; i++)
			{
				rmse_and_penalty_per_factor[i] = (double) rmse_and_penalty_per_factor[i] / num_users;
				Console.Error.Write("{0,0:0.####} ", rmse_and_penalty_per_factor[i]);
			}
			rmse    = (double) rmse    / (num_factors * num_users);
			penalty = (double) penalty / (num_factors * num_users);
			Console.Error.WriteLine(" > {0,0:0.####} ({1,0:0.####})", rmse, penalty);

			return rmse_and_penalty_per_factor;
		}

		/// <summary>map from user attributes to latent factor space</summary>
		protected virtual double[] MapUserToLatentFactorSpace(HashSet<int> user_attributes)
		{
			double[] factor_representation = new double[num_factors];
			for (int j = 0; j < num_factors; j++)
				// bias
				factor_representation[j] = attribute_to_factor[NumUserAttributes, j];

			foreach (int i in user_attributes)
				for (int j = 0; j < num_factors; j++)
					factor_representation[j] += attribute_to_factor[i, j];

			return factor_representation;
		}

        /// <inheritdoc/>
        public override double Predict(int user_id, int item_id)
        {
			HashSet<int> attributes = user_attributes[user_id];
			double[] est_factors = MapUserToLatentFactorSpace(attributes);

            double result = 0;
            for (int f = 0; f < num_factors; f++)
                result += item_factors[item_id, f] * est_factors[f];
            return result;
        }

		/// <inheritdoc/>
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(
				ni,
				"BPRMF_UserMapping num_factors={0} reg_u={1} reg_i={2} reg_j={3} num_iter={4} learn_rate={5} reg_mapping={6} num_iter_mapping={7} learn_rate_mapping={8} init_mean={9} init_stdev={10}",
				num_factors, reg_u, reg_i, reg_j, NumIter, learn_rate, reg_mapping, num_iter_mapping, learn_rate_mapping, InitMean, InitStdev
			);
		}

	}
}

