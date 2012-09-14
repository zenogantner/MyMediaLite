// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Latent-feature log linear model</summary>
	/// <remarks>
	///   <para>
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Aditya Krishna Menon, Charles Elkan:
	///         A log-linear model with latent features for dyadic prediction.
	///         ICDM 2010.
	///         http://cseweb.ucsd.edu/~akmenon/LFL-ICDM10.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class LatentFeatureLogLinearModel : RatingPredictor, IIterativeModel
	{
		// TODO
		//  - load/save
		//  - ComputeObjective()
		//  - RMSE/MAE optimization
		//  - incremental updates
		//  - fold-in
		//  - centered: instead if  rating look at rating - global_bias

		/// <summary>Mean of the normal distribution used to initialize the factors</summary>
		public double InitMean { get; set; }

		/// <summary>Standard deviation of the normal distribution used to initialize the factors</summary>
		public double InitStdDev { get; set; }

		/// <summary>Number of latent factors</summary>
		public uint NumFactors { get; set;}

		/// <summary>Learn rate</summary>
		public float LearnRate { get; set; }

		/// <summary>Number of iterations over the training data</summary>
		public uint NumIter { get; set; }

		/// <summary>Regularization based on rating frequency</summary>
		/// <description>
		/// Regularization proportional to the inverse of the square root of the number of ratings associated with the user or item.
		/// As described in the paper by Menon and Elkan.
		/// </description>
		public bool FrequencyRegularization { get; set; }

		/// <summary>The optimization target</summary>
		public OptimizationTarget Loss { get; set; }

		/// <summary>Learn rate factor for the bias terms</summary>
		public float BiasLearnRate { get; set; }

		/// <summary>regularization factor for the bias terms</summary>
		public float BiasReg { get; set; }

		/// <summary>regularization constant for the user factors</summary>
		public float RegU { get; set; }

		/// <summary>regularization constant for the item factors</summary>
		public float RegI { get; set; }

		// base category does not need biases and latent factors
		// all further categories are represented by the following structure
		IList<Matrix<float>> user_factors;
		IList<Matrix<float>> item_factors;
		Matrix<float> user_biases;
		Matrix<float> item_biases;

		// base category is last one
		IList<float> label_id_to_value;
		Dictionary<float, int> value_to_label_id;

		/// <summary>Default constructor</summary>
		public LatentFeatureLogLinearModel()
		{
			BiasReg = 0.01f;
			BiasLearnRate = 1.0f;
			RegU = 0.015f;
			RegI = 0.015f;
			LearnRate = 0.01f;
			NumIter = 30;
			InitStdDev = 0.1;
			NumFactors = 10;
		}

		///
		public override void Train()
		{
			InitModel();

			for (int it = 0; it <= NumIter; it++)
				Iterate();
		}

		///
		public void Iterate()
		{
			Iterate(ratings.RandomIndex, true, true);
		}

		/*
		void SetupLoss()
		{
			switch (Loss)
			{
				case OptimizationTarget.MAE:
					throw new NotSupportedException();
					break;
				case OptimizationTarget.RMSE:
					compute_gradient_common = (sig_score, err) => (float) (err * sig_score * (1 - sig_score) * rating_range_size);
					break;
				case OptimizationTarget.LogisticLoss:
					compute_gradient_common = (sig_score, err) => (float) err;
					break;
			}
		}
		*/

		void InitModel()
		{
			// determine base class
			var label_frequency = new Dictionary<float, int>();
			for (int i = 0; i < ratings.Count; i++)
			{
				float label = ratings[i];
				if (label_frequency.ContainsKey(label))
					label_frequency[label]++;
				else
					label_frequency.Add(label, 1);
			}
			var label_id_to_value = label_frequency.Keys.ToList();
			label_id_to_value.Sort(
				delegate(float x1, float x2) {
					return label_frequency[x1].CompareTo(label_frequency[x2]);
				});

			//  assign label ID <-> value mappings
			this.label_id_to_value = label_id_to_value;
			value_to_label_id = new Dictionary<float, int>();
			for (int label = 0; label < label_id_to_value.Count; label++)
				value_to_label_id[label_id_to_value[label]] = label;

			// init model parameters
			int num_labels = label_id_to_value.Count;
			user_biases = new Matrix<float>(num_labels - 1, MaxUserID + 1);
			item_biases = new Matrix<float>(num_labels - 1, MaxItemID + 1);
			user_factors = new Matrix<float>[num_labels - 1];
			item_factors = new Matrix<float>[num_labels - 1];
			for (int label = 0; label < num_labels - 1; label++)
			{
				user_factors[label] = new Matrix<float>(MaxUserID + 1, NumFactors);
				item_factors[label] = new Matrix<float>(MaxItemID + 1, NumFactors);
				user_factors[label].InitNormal(InitMean, InitStdDev);
				item_factors[label].InitNormal(InitMean, InitStdDev);
			}
		}

		///
		void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			//SetupLoss();

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];
				int correct_label = value_to_label_id[ratings[index]];

				float user_reg_weight = FrequencyRegularization ? (float) (RegU / Math.Sqrt(ratings.CountByUser[u])) : RegU;
				float item_reg_weight = FrequencyRegularization ? (float) (RegI / Math.Sqrt(ratings.CountByItem[i])) : RegI;

				for (int l = 0; l < label_id_to_value.Count - 1; l++)
				{
					double dot_product = user_biases[l, u] + item_biases[l, i] + MatrixExtensions.RowScalarProduct(user_factors[l], u, item_factors[l], i);
					double label_percentage = 1 / (1 + Math.Exp(-dot_product));

					float gradient_common = (float) -label_percentage;
					if (l == correct_label)
						gradient_common += 1;

					// adjust biases
					if (update_user)
						user_biases.Inc(l, u, BiasLearnRate * LearnRate * (gradient_common - BiasReg * user_reg_weight * user_biases[l, u]));
					if (update_item)
						item_biases.Inc(l, i, BiasLearnRate * LearnRate * (gradient_common - BiasReg * item_reg_weight * item_biases[l, i]));

					// adjust latent factors
					for (int f = 0; f < NumFactors; f++)
					{
						double u_f = user_factors[l][u, f];
						double i_f = item_factors[l][i, f];

						if (update_user)
						{
							double delta_u = gradient_common * i_f - user_reg_weight * u_f;
							user_factors[l].Inc(u, f, LearnRate * delta_u);
						}
						if (update_item)
						{
							double delta_i = gradient_common * u_f - item_reg_weight * i_f;
							item_factors[l].Inc(i, f, LearnRate * delta_i);
						}
					}
				}
			}
		}

		///
		public float ComputeObjective()
		{
			return -1;
		}

		IList<float> PredictPercentages(int user_id, int item_id)
		{
			var percentages = new float[label_id_to_value.Count];
			float percentage_sum = 0;

			for (int label = 0; label < percentages.Length - 1; label++)
			{
				double score = 0;
				if (user_id <= MaxUserID)
					score += user_biases[label, user_id];
				if (item_id <= MaxItemID)
					score += item_biases[label, item_id];
				if (user_id <= MaxUserID && item_id <= MaxItemID)
					score += MatrixExtensions.RowScalarProduct(user_factors[label], user_id, item_factors[label], item_id);
				
				float p = (float) ( 1 / (1 + Math.Exp(-score)) );
				percentages[label] = p;
				percentage_sum += p;
			}
			percentages[percentages.Length - 1] = 1 - percentage_sum;

			return percentages;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			var percentages = PredictPercentages(user_id, item_id);
			double prediction = 0;
			for (int l = 0; l < label_id_to_value.Count; l++)
				prediction += label_id_to_value[l] * percentages[l];

			// TODO maybe cap values

			return (float) prediction;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} bias_reg={2} reg_u={3} reg_i={4} frequency_regularization={5} learn_rate={6} bias_learn_rate={7} num_iter={8} loss={9}",
				this.GetType().Name, NumFactors, BiasReg, RegU, RegI, FrequencyRegularization,LearnRate, BiasLearnRate, NumIter, Loss);
		}
	}
}