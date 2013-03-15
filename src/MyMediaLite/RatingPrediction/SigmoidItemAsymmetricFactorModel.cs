// Copyright (C) 2012 Zeno Gantner
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
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Asymmetric factor model</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Arkadiusz Paterek:
	///         Improving regularized singular value decomposition for collaborative filtering.
	///         KDD Cup 2007.
	///         http://arek-paterek.com/ap_kdd.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class SigmoidItemAsymmetricFactorModel : BiasedMatrixFactorization, ITransductiveRatingPredictor
	{
		int[][] items_rated_by_user;
		int[] feedback_count_by_item;
		float[] y_reg;

		/// <summary>user factors (part expressed via the rated items)</summary>
		internal Matrix<float> y;

		///
		public IDataSet AdditionalFeedback { get; set; }

		/// <summary>Default constructor</summary>
		public SigmoidItemAsymmetricFactorModel() : base()
		{
			AdditionalFeedback = new PosOnlyFeedback<SparseBooleanMatrix>(); // in case no test data is provided
			Regularization = 0.015f;
			LearnRate = 0.001f;
			BiasLearnRate = 0.7f;
			BiasReg = 0.33f;
		}

		///
		public override void Train()
		{
			MaxUserID = Math.Max(ratings.MaxUserID, AdditionalFeedback.MaxUserID);
			MaxItemID = Math.Max(ratings.MaxItemID, AdditionalFeedback.MaxItemID);
			items_rated_by_user = this.ItemsRatedByUser();
			feedback_count_by_item = this.ItemFeedbackCounts();
			y_reg = new float[MaxItemID + 1];
			for (int item_id = 0; item_id <= MaxItemID; item_id++)
				if (feedback_count_by_item[item_id] > 0)
					y_reg[item_id] = FrequencyRegularization ? (float) (RegI / Math.Sqrt(feedback_count_by_item[item_id])) : RegI;
				else
					y_reg[item_id] = 0;

			base.Train();
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_factors == null)
				PrecomputeUserFactors();
			return base.Predict(user_id, item_id);
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			SetupLoss();

			float reg_u = RegU;  // to limit property accesses
			float reg_i = RegI;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double score = global_bias + user_bias[u] + item_bias[i];
				var u_plus_y_sum_vector = y.SumOfRows(items_rated_by_user[u]);
				double norm_denominator = Math.Sqrt(items_rated_by_user[u].Length);
				for (int f = 0; f < u_plus_y_sum_vector.Count; f++)
					u_plus_y_sum_vector[f] = (float) (u_plus_y_sum_vector[f] / norm_denominator);

				score += item_factors.RowScalarProduct(i, u_plus_y_sum_vector);
				double sig_score = 1 / (1 + Math.Exp(-score));

				double prediction = min_rating + sig_score * rating_range_size;
				double err = ratings[index] - prediction;

				float user_reg_weight = FrequencyRegularization ? (float) (reg_u / Math.Sqrt(ratings.CountByUser[u])) : reg_u;
				float item_reg_weight = FrequencyRegularization ? (float) (reg_i / Math.Sqrt(ratings.CountByItem[i])) : reg_i;
				float gradient_common = compute_gradient_common(sig_score, err);

				// adjust biases
				if (update_user)
					user_bias[u] += BiasLearnRate * current_learnrate * (gradient_common - BiasReg * user_reg_weight * user_bias[u]);
				if (update_item)
					item_bias[i] += BiasLearnRate * current_learnrate * (gradient_common - BiasReg * item_reg_weight * item_bias[i]);

				// adjust factors
				double normalized_gradient_common = gradient_common / norm_denominator;
				for (int f = 0; f < NumFactors; f++)
				{
					float i_f = item_factors[i, f];

					// if necessary, compute and apply updates
					if (update_item)
					{
						double delta_i = gradient_common * u_plus_y_sum_vector[f] - item_reg_weight * i_f;
						item_factors.Inc(i, f, current_learnrate * delta_i);

						double common_update = normalized_gradient_common * i_f;
						foreach (int other_item_id in items_rated_by_user[u])
						{
							double delta_oi = common_update - y_reg[other_item_id] * y[other_item_id, f];
							y.Inc(other_item_id, f, current_learnrate * delta_oi);
						}
					}
				}
				user_factors = null; // delete old user factors
			}
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "3.00") )
			{
				writer.WriteLine(global_bias.ToString(CultureInfo.InvariantCulture));
				writer.WriteLine(min_rating.ToString(CultureInfo.InvariantCulture));
				writer.WriteLine(max_rating.ToString(CultureInfo.InvariantCulture));
				writer.WriteVector(user_bias);
				writer.WriteVector(item_bias);
				writer.WriteMatrix(y);
				writer.WriteMatrix(item_factors);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				var global_bias = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var min_rating  = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var max_rating  = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				var user_bias = reader.ReadVector();
				var item_bias = reader.ReadVector();
				var y            = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
				var item_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				if (item_bias.Count != item_factors.dim1)
					throw new IOException(
						string.Format(
							"Number of items must be the same for biases and factors: {0} != {1}",
							item_bias.Count, item_factors.dim1));

				if (y.NumberOfColumns != item_factors.NumberOfColumns)
					throw new Exception(
						string.Format("Number of user (y) and item factors must match: {0} != {1}",
							y.NumberOfColumns, item_factors.NumberOfColumns));

				this.MaxUserID = user_bias.Count - 1;
				this.MaxItemID = item_factors.NumberOfRows - 1;

				// assign new model
				this.global_bias = global_bias;
				if (this.NumFactors != item_factors.NumberOfColumns)
				{
					Console.Error.WriteLine("Set NumFactors to {0}", item_factors.NumberOfColumns);
					this.NumFactors = (uint) item_factors.NumberOfColumns;
				}
				this.user_bias = user_bias.ToArray();
				this.item_bias = item_bias.ToArray();
				this.y = y;
				this.item_factors = item_factors;
				this.min_rating = min_rating;
				this.max_rating = max_rating;
				user_factors = null; // enforce computation at first prediction

				rating_range_size = max_rating - min_rating;
			}
		}

		///
		public override float ComputeObjective()
		{
			double complexity = 0;
			if (FrequencyRegularization)
			{
				for (int u = 0; u <= ratings.MaxUserID; u++)
					if (ratings.CountByUser[u] > 0)
						complexity += (RegU / Math.Sqrt(ratings.CountByUser[u])) * BiasReg * Math.Pow(user_bias[u], 2);
				for (int i = 0; i <= MaxItemID; i++)
				{
					complexity += y_reg[i] * Math.Pow(y.GetRow(i).EuclideanNorm(), 2);
					if (i > ratings.MaxItemID || ratings.CountByItem[i] == 0)
						continue;
					complexity += (RegI / Math.Sqrt(ratings.CountByItem[i]))           * Math.Pow(item_factors.GetRow(i).EuclideanNorm(), 2);
					complexity += (RegI / Math.Sqrt(ratings.CountByItem[i])) * BiasReg * Math.Pow(item_bias[i], 2);
				}
			}
			else
			{
				for (int u = 0; u <= MaxUserID; u++)
					complexity += (RegU / ratings.CountByUser[u]) * BiasReg * Math.Pow(user_bias[u], 2);
				for (int i = 0; i <= MaxItemID; i++)
				{
					complexity += y_reg[i] * Math.Pow(y.GetRow(i).EuclideanNorm(), 2);
					if (i > ratings.MaxItemID)
						continue;
					complexity += (RegI / ratings.CountByItem[i])           * Math.Pow(item_factors.GetRow(i).EuclideanNorm(), 2);
					complexity += (RegI / ratings.CountByItem[i]) * BiasReg * Math.Pow(item_bias[i], 2);
				}
			}

			return (float) (ComputeLoss() + complexity);
		}

		///
		protected override float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			SetupLoss();

			float user_bias = 0;

			var items = (from pair in rated_items select pair.Item1).ToArray();
			float user_reg_weight = FrequencyRegularization ? (float) (Regularization / Math.Sqrt(items.Length)) : Regularization;

			// compute stuff that will not change
			var y_sum_vector = y.SumOfRows(items);
			double norm_denominator = Math.Sqrt(items.Length);
			for (int f = 0; f < y_sum_vector.Count; f++)
				y_sum_vector[f] = (float) (y_sum_vector[f] / norm_denominator);

			rated_items.Shuffle();
			for (uint it = 0; it < NumIter; it++)
			{
				for (int index = 0; index < rated_items.Count; index++)
				{
					int item_id = rated_items[index].Item1;

					double score = global_bias + user_bias + item_bias[item_id];
					score += item_factors.RowScalarProduct(item_id, y_sum_vector);
					double sig_score = 1 / (1 + Math.Exp(-score));
					double prediction = min_rating + sig_score * rating_range_size;
					float err = (float) (rated_items[index].Item2 - prediction);
					float gradient_common = compute_gradient_common(sig_score, err);

					// adjust bias
					user_bias += BiasLearnRate * LearnRate * ((float) gradient_common - BiasReg * user_reg_weight * user_bias);
				}
			}

			// assign final parameter values to return vector
			var user_vector = new float[NumFactors + 1];
			user_vector[0] = user_bias;
			for (int f = 0; f < NumFactors; f++)
				user_vector[f + 1] = (float) y_sum_vector[f];

			return user_vector;
		}

		///
		protected internal override void InitModel()
		{
			y = new Matrix<float>(MaxItemID + 1, NumFactors);
			y.InitNormal(InitMean, InitStdDev);

			// set factors to zero for items without training examples
			for (int item_id = 0; item_id < y.NumberOfRows; item_id++)
				if (item_id > ratings.MaxItemID || ratings.CountByItem[item_id] == 0)
					y.SetRowToOneValue(item_id, 0);

			base.InitModel();
		}

		/// <summary>Precompute all user factors</summary>
		protected void PrecomputeUserFactors()
		{
			if (user_factors == null)
				user_factors = new Matrix<float>(MaxUserID + 1, NumFactors);

			if (items_rated_by_user == null)
				items_rated_by_user = this.ItemsRatedByUser();

			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				PrecomputeUserFactors(user_id);
		}

		/// <summary>Precompute the factors for a given user</summary>
		/// <param name='user_id'>the ID of the user</param>
		protected void PrecomputeUserFactors(int user_id)
		{
			if (items_rated_by_user[user_id].Length == 0)
				return;

			// compute
			var factors = y.SumOfRows(items_rated_by_user[user_id]);
			double norm_denominator = Math.Sqrt(items_rated_by_user[user_id].Length);
			for (int f = 0; f < factors.Count; f++)
				factors[f] = (float) (factors[f] / norm_denominator);

			// assign
			for (int f = 0; f < factors.Count; f++)
				user_factors[user_id, f] = (float) factors[f];
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} regularization={2} bias_reg={3} frequency_regularization={4} learn_rate={5} bias_learn_rate={6} learn_rate_decay={7} num_iter={7} loss={8}",
				this.GetType().Name, NumFactors, Regularization, BiasReg, FrequencyRegularization, LearnRate, BiasLearnRate, Decay, NumIter, Loss);
		}
	}
}

