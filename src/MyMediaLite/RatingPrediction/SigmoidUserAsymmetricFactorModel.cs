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
	/// <summary>Asymmetric factor model which represents items in terms of the users that rated them</summary>
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
	public class SigmoidUserAsymmetricFactorModel : BiasedMatrixFactorization, ITransductiveRatingPredictor
	{
		int[][] users_who_rated_the_item;
		int[] feedback_count_by_user;
		float[] x_reg;

		/// <summary>item factors (part expressed via the users who rated them)</summary>
		internal Matrix<float> x;

		///
		public IDataSet AdditionalFeedback { get; set; }

		/// <summary>Default constructor</summary>
		public SigmoidUserAsymmetricFactorModel() : base()
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
			MaxUserID = Math.Max(MaxUserID, AdditionalFeedback.MaxUserID);
			MaxItemID = Math.Max(MaxItemID, AdditionalFeedback.MaxItemID);
			users_who_rated_the_item = this.UsersWhoRated();
			feedback_count_by_user = this.UserFeedbackCounts();
			x_reg = new float[MaxUserID + 1];
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				if (feedback_count_by_user[user_id] > 0)
					x_reg[user_id] = FrequencyRegularization ? (float) (RegU / Math.Sqrt(feedback_count_by_user[user_id])) : RegU;
				else
					x_reg[user_id] = 0;

			base.Train();
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (item_factors == null)
				PrecomputeItemFactors();
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
				var x_sum = x.SumOfRows(users_who_rated_the_item[i]);
				double norm_denominator = Math.Sqrt(users_who_rated_the_item[i].Length);
				for (int f = 0; f < x_sum.Count; f++)
					x_sum[f] = (float) (x_sum[f] / norm_denominator);

				score += user_factors.RowScalarProduct(u, x_sum);
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
					float u_f = user_factors[u, f];

					// if necessary, compute and apply updates
					if (update_user)
					{
						double delta_u = gradient_common * x_sum[f] - user_reg_weight * u_f;
						user_factors.Inc(u, f, current_learnrate * delta_u);

						double common_update = normalized_gradient_common * u_f;
						foreach (int other_user_id in users_who_rated_the_item[i])
						{
							double delta_ou = common_update - x_reg[other_user_id] * x[other_user_id, f];
							x.Inc(other_user_id, f, current_learnrate * delta_ou);
						}
					}
				}
			}
			item_factors = null; // delete old item factors
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
				writer.WriteMatrix(x);
				writer.WriteMatrix(user_factors);
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
				var x            = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));
				var user_factors = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				if (user_bias.Count != user_factors.dim1)
					throw new IOException(
						string.Format(
							"Number of users must be the same for biases and factors: {0} != {1}",
							user_bias.Count, user_factors.dim1));

				if (x.NumberOfColumns != user_factors.NumberOfColumns)
					throw new Exception(
						string.Format("Number of item (x) and user factors must match: {0} != {1}",
							x.NumberOfColumns, user_factors.NumberOfColumns));

				this.MaxUserID = user_factors.NumberOfRows - 1;

				// assign new model
				this.global_bias = global_bias;
				if (this.NumFactors != user_factors.NumberOfColumns)
				{
					Console.Error.WriteLine("Set NumFactors to {0}", user_factors.NumberOfColumns);
					this.NumFactors = (uint) user_factors.NumberOfColumns;
				}
				this.user_bias = user_bias.ToArray();
				this.item_bias = item_bias.ToArray();
				this.x = x;
				this.user_factors = user_factors;
				this.min_rating = min_rating;
				this.max_rating = max_rating;
				item_factors = null; // enfore computation at first prediction

				rating_range_size = max_rating - min_rating;
			}
		}

		///
		public override float ComputeObjective()
		{
			double complexity = 0;
			if (FrequencyRegularization)
			{
				for (int u = 0; u <= MaxUserID; u++)
				{
					complexity += x_reg[u] * Math.Pow(x.GetRow(u).EuclideanNorm(), 2);
					if (u > ratings.MaxUserID || ratings.CountByUser[u] == 0)
						continue;

					complexity += (RegU / Math.Sqrt(ratings.CountByUser[u]))           * Math.Pow(user_factors.GetRow(u).EuclideanNorm(), 2);
					complexity += (RegU / Math.Sqrt(ratings.CountByUser[u])) * BiasReg * Math.Pow(user_bias[u], 2);
				}
				for (int i = 0; i <= ratings.MaxItemID; i++)
					if (ratings.CountByItem[i] > 0)
						complexity += (RegI / Math.Sqrt(ratings.CountByItem[i])) * BiasReg * Math.Pow(item_bias[i], 2);
			}
			else
			{
				for (int u = 0; u <= MaxUserID; u++)
				{
					complexity += feedback_count_by_user[u] * RegU * Math.Pow(x.GetRow(u).EuclideanNorm(), 2);
					if (u > ratings.MaxUserID)
						continue;

					complexity += ratings.CountByUser[u] * RegU           * Math.Pow(user_factors.GetRow(u).EuclideanNorm(), 2);
					complexity += ratings.CountByUser[u] * RegU * BiasReg * Math.Pow(user_bias[u], 2);
				}
				for (int i = 0; i <= MaxItemID; i++)
					complexity += ratings.CountByItem[i] * RegI * BiasReg * Math.Pow(item_bias[i], 2);
			}

			return (float) (ComputeLoss() + complexity);
		}

		///
		protected override float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			throw new NotImplementedException();
		}

		///
		protected internal override void InitModel()
		{
			x = new Matrix<float>(MaxUserID + 1, NumFactors);
			x.InitNormal(InitMean, InitStdDev);

			// set factors to zero for users without training examples
			for (int user_id = 0; user_id < x.NumberOfRows; user_id++)
				if (user_id > ratings.MaxUserID || ratings.CountByUser[user_id] == 0)
					x.SetRowToOneValue(user_id, 0);

			base.InitModel();
		}

		/// <summary>Precompute all item factors</summary>
		protected void PrecomputeItemFactors()
		{
			MaxItemID = Math.Max(Ratings.MaxItemID, AdditionalFeedback.MaxItemID);

			if (item_factors == null)
				item_factors = new Matrix<float>(MaxItemID + 1, NumFactors);

			if (users_who_rated_the_item == null)
				users_who_rated_the_item = this.UsersWhoRated();

			for (int item_id = 0; item_id <= MaxItemID; item_id++)
				PrecomputeItemFactors(item_id);
		}

		/// <summary>Precompute the factors for a given item</summary>
		/// <param name='item_id'>the ID of the item</param>
		protected void PrecomputeItemFactors(int item_id)
		{
			if (users_who_rated_the_item[item_id].Length == 0)
				return;

			// compute
			var factors = x.SumOfRows(users_who_rated_the_item[item_id]);
			double norm_denominator = Math.Sqrt(users_who_rated_the_item[item_id].Length);
			for (int f = 0; f < factors.Count; f++)
				factors[f] = (float) (factors[f] / norm_denominator);

			// assign
			for (int f = 0; f < factors.Count; f++)
				item_factors[item_id, f] = (float) factors[f];
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_factors={1} regularization={2} bias_reg={3} frequency_regularization={4} learn_rate={5} bias_learn_rate={6} learn_rate_decay={7} num_iter={8} loss={9}",
				this.GetType().Name, NumFactors, Regularization, BiasReg, FrequencyRegularization, LearnRate, BiasLearnRate, Decay, NumIter, Loss);
		}
	}
}

