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
using System.Globalization;
using MyMediaLite;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>BPR-MF with attribute-to-factor mapping</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
    ///       <item><description>
	///         Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Steffen Rendle, Lars Schmidt-Thieme:
	///         Learning Attribute-to-Feature Mappings for Cold-Start Recommendations.
	///         ICDM 2011.
	///         http://www.ismll.uni-hildesheim.de/pub/pdfs/Gantner_et_al2010Mapping.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class BPRMF_Mapping : BPRMF, IItemAttributeAwareRecommender
	{
		// TODO
		//  - make sure we do not sample items that have no feedback when doing BPR-MF learning
		//  - integrate UserMapping
		//  - make mapping function selectable
		//  - different mapping functions for users and items
		//  - load/save
		
		/// <summary>The learn rate for training the mapping functions</summary>
		public double LearnRateMapping { get { return learn_rate_mapping; } set { learn_rate_mapping = value; } }
		/// <summary>The learn rate for training the mapping functions</summary>
		protected double learn_rate_mapping = 0.01;

		/// <summary>number of times the regression is computed (to avoid local minima)</summary>
		/// <remarks>may be ignored by the recommender</remarks>
		public int NumInitMapping {	get { return num_init_mapping; } set { num_init_mapping = value; } }
		/// <summary>number of times the regression is computed (to avoid local minima)</summary>
		/// <remarks>may be ignored by the recommender</remarks>
		protected int num_init_mapping = 5;

		/// <summary>number of iterations of the mapping training procedure</summary>
		public int NumIterMapping { get { return this.num_iter_mapping; } set { num_iter_mapping = value; } }
		/// <summary>number of iterations of the mapping training procedure</summary>
		protected int num_iter_mapping = 10;

		/// <summary>regularization constant for the mapping</summary>
		public double RegMapping { get { return this.reg_mapping; } set { reg_mapping = value; } }
		/// <summary>regularization constant for the mapping</summary>
		protected double reg_mapping = 0.1;

		/// <summary>The matrix representing the attribute-to-factor mapping</summary>
		/// <remarks>includes bias</remarks>
		protected Matrix<float> item_attribute_to_factor;

		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
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
		protected float[] item_factor_bias;
		
		///
		public override void Train()
		{
			base.Train();
			LearnItemAttributeToFactorMapping();
		}
		
		
		void LearnItemAttributeToFactorMapping()
		{
			// create attribute-to-latent factor weight matrix
			this.item_attribute_to_factor = new Matrix<float>(NumItemAttributes + 1, num_factors);
			// store the results of the different runs in the following array
			var old_attribute_to_factor = new Matrix<float>[num_init_mapping];

			Console.Error.WriteLine("Will use {0} examples ...", num_iter_mapping * MaxItemID);

			var old_rmse_per_factor = new double[num_init_mapping][];

			for (int h = 0; h < num_init_mapping; h++)
			{
				MatrixExtensions.InitNormal(item_attribute_to_factor, InitMean, InitStdDev);
				Console.Error.WriteLine("----");

				for (int i = 0; i < num_iter_mapping * MaxItemID; i++)
					UpdateItemMapping();
				old_attribute_to_factor[h] = new Matrix<float>(item_attribute_to_factor);
				old_rmse_per_factor[h] = ComputeItemMappingFit();
			}

			var min_rmse_per_factor = new double[num_factors];
			for (int i = 0; i < num_factors; i++)
				min_rmse_per_factor[i] = Double.MaxValue;
			var best_factor_init = new int[num_factors];

			// find best factor mappings
			for (int i = 0; i < num_init_mapping; i++)
				for (int j = 0; j < num_factors; j++)
					if (old_rmse_per_factor[i][j] < min_rmse_per_factor[j])
					{
						min_rmse_per_factor[j] = old_rmse_per_factor[i][j];
						best_factor_init[j] = i;
					}

			// set the best weight combinations for each factor mapping
			for (int i = 0; i < num_factors; i++)
			{
				Console.Error.WriteLine("Factor {0}, pick {1}", i, best_factor_init[i]);

				item_attribute_to_factor.SetColumn(i,
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
			while (true) // sampling with replacement
			{
				int item_id = random.Next(MaxItemID + 1);
				var item_users = Feedback.ItemMatrix.GetEntriesByRow(item_id);
				if (item_users.Count == 0)
					continue;
				return item_id;
			}
		}

		void UpdateItemMapping()
		{
			int item_id = SampleItem();

			float[] est_factors = MapToItemLatentFactorSpace(item_id);

			for (int j = 0; j < num_factors; j++) {
				// TODO do we need an absolute term here???
				double diff = est_factors[j] - item_factors[item_id, j];
				if (diff > 0)
				{
					foreach (int attribute in item_attributes[item_id])
					{
						double w = item_attribute_to_factor[attribute, j];
						double deriv = diff * w + reg_mapping * w;
						MatrixExtensions.Inc(item_attribute_to_factor, attribute, j, learn_rate_mapping * -deriv);
					}
					// bias term
					double w_bias = item_attribute_to_factor[NumItemAttributes, j];
					double deriv_bias = diff * w_bias + reg_mapping * w_bias;
					MatrixExtensions.Inc(item_attribute_to_factor, NumItemAttributes, j, learn_rate_mapping * -deriv_bias);
				}
			}
		}

		/// <summary>Compute the fit of the item mapping</summary>
		/// <returns>
		/// an array of doubles containing the RMSE on the training data for each latent factor
		/// </returns>
		protected double[] ComputeItemMappingFit()
		{
			double rmse    = 0;
			double penalty = 0;
			var rmse_and_penalty_per_factor = new double[num_factors];

			int num_items = 0;
			for (int item_id = 0; item_id <= MaxItemID; item_id++)
			{
				if (item_id > Feedback.MaxItemID)
					continue;
				var item_users = Feedback.ItemMatrix.GetEntriesByRow(item_id);
				var item_attrs = item_attributes.GetEntriesByRow(item_id);
				if (item_attrs.Count == 0) // TODO why ignore users w/o attributes?
					continue;

				num_items++;

				float[] est_factors = MapToItemLatentFactorSpace(item_id);
				for (int f = 0; f < num_factors; f++)
				{
					double error    = Math.Pow(est_factors[f] - item_factors[item_id, f], 2);
					double reg_term = reg_mapping * VectorExtensions.EuclideanNorm(item_attribute_to_factor.GetColumn(f));
					rmse    += error;
					penalty += reg_term;
					rmse_and_penalty_per_factor[f] += error + reg_term;
				}
			}

			for (int i = 0; i < num_factors; i++)
			{
				rmse_and_penalty_per_factor[i] = (double) rmse_and_penalty_per_factor[i] / num_items;
				Console.Error.Write("{0:0.####} ", rmse_and_penalty_per_factor[i]);
			}
			rmse    = (double) rmse    / (num_factors * num_items);
			penalty = (double) penalty / (num_factors * num_items);
			Console.Error.WriteLine(" > {0:0.####} ({1:0.####})", rmse, penalty);

			return rmse_and_penalty_per_factor;
		}

		/// <summary>map to latent factor space (method to be called)</summary>
		protected virtual float[] MapToItemLatentFactorSpace(int item_id)
		{
			var factor_representation = new float[num_factors];
			for (int j = 0; j < num_factors; j++)
				// bias
				factor_representation[j] = item_attribute_to_factor[NumItemAttributes, j];

			foreach (int i in item_attributes[item_id])
				for (int j = 0; j < num_factors; j++)
					factor_representation[j] += item_attribute_to_factor[i, j];

			return factor_representation;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if ((user_id < 0) || (user_id >= user_factors.dim1))
			{
				Console.Error.WriteLine("user is unknown: " + user_id);
				return float.MinValue;
			}

			float[] est_factors = MapToItemLatentFactorSpace(item_id);
			return MatrixExtensions.RowScalarProduct(user_factors, user_id, est_factors);
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

