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
	/// <summary>
	///   Asymmetric factor model which represents items in terms of the users that rated them,
	///   and users in terms of the items they rated
	/// </summary>
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
	public class SigmoidCombinedAsymmetricFactorModel : BiasedMatrixFactorization, ITransductiveRatingPredictor
	{
		int[][] users_who_rated_the_item;
		int[][] items_rated_by_user;
		int[] feedback_count_by_user;
		int[] feedback_count_by_item;
		float[] x_reg;
		float[] y_reg;

		/// <summary>item factors (part expressed via the users who rated them)</summary>
		internal Matrix<float> x;
		/// <summary>user factors (part expressed via the rated items)</summary>
		internal Matrix<float> y;

		///
		public IDataSet AdditionalFeedback { get; set; }

		/// <summary>Default constructor</summary>
		public SigmoidCombinedAsymmetricFactorModel() : base()
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
			users_who_rated_the_item = this.UsersWhoRated();
			items_rated_by_user = this.ItemsRatedByUser();
			feedback_count_by_user = this.UserFeedbackCounts();
			feedback_count_by_item = this.ItemFeedbackCounts();
			x_reg = new float[MaxUserID + 1];
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				if (feedback_count_by_user[user_id] > 0)
					x_reg[user_id] = FrequencyRegularization ? (float) (RegU / Math.Sqrt(feedback_count_by_user[user_id])) : RegU;
				else
					x_reg[user_id] = 0;
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
			if (item_factors == null)
				PrecomputeItemFactors();
			return base.Predict(user_id, item_id);
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			user_factors = null; // delete old user factors
			item_factors = null; // delete old item factors

			SetupLoss();

			float reg_u = RegU;  // to limit property accesses
			float reg_i = RegI;

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double score = global_bias + user_bias[u] + item_bias[i];

				var y_sum = y.SumOfRows(items_rated_by_user[u]);
				double u_norm_denominator = Math.Sqrt(items_rated_by_user[u].Length);
				for (int f = 0; f < y_sum.Count; f++)
					y_sum[f] = (float) (y_sum[f] / u_norm_denominator);

				var x_sum = x.SumOfRows(users_who_rated_the_item[i]);
				double i_norm_denominator = Math.Sqrt(users_who_rated_the_item[i].Length);
				for (int f = 0; f < x_sum.Count; f++)
					x_sum[f] = (float) (x_sum[f] / i_norm_denominator);

				score += DataType.VectorExtensions.ScalarProduct(y_sum, x_sum);
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
				double u_normalized_gradient_common = gradient_common / u_norm_denominator;
				double i_normalized_gradient_common = gradient_common / i_norm_denominator;
				for (int f = 0; f < NumFactors; f++)
				{
					float u_f = y_sum[f];
					float i_f = x_sum[f];

					// if necessary, compute and apply updates
					if (update_user)
					{
						double common_update = i_normalized_gradient_common * u_f;
						foreach (int other_user_id in users_who_rated_the_item[i])
						{
							double delta_ou = common_update - x_reg[other_user_id] * x[other_user_id, f];
							x.Inc(other_user_id, f, current_learnrate * delta_ou);
						}
					}

					// if necessary, compute and apply updates
					if (update_item)
					{
						double common_update = u_normalized_gradient_common * i_f;
						foreach (int other_item_id in items_rated_by_user[u])
						{
							double delta_oi = common_update - y_reg[other_item_id] * y[other_item_id, f];
							y.Inc(other_item_id, f, current_learnrate * delta_oi);
						}
					}
				}
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
				writer.WriteMatrix(x);
				writer.WriteMatrix(y);
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
				var y            = (Matrix<float>) reader.ReadMatrix(new Matrix<float>(0, 0));

				this.MaxUserID = user_bias.Count - 1;
				this.MaxItemID = item_bias.Count - 1;

				// assign new model
				this.global_bias = global_bias;
				if (this.NumFactors != x.NumberOfColumns)
				{
					Console.Error.WriteLine("Set NumFactors to {0}", x.NumberOfColumns);
					this.NumFactors = (uint) x.NumberOfColumns;
				}
				this.user_bias = user_bias.ToArray();
				this.item_bias = item_bias.ToArray();
				this.x = x;
				this.y = y;
				this.min_rating = min_rating;
				this.max_rating = max_rating;

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
					complexity += x_reg[u] * Math.Pow(x.GetRow(u).EuclideanNorm(), 2);
				for (int u = 0; u <= ratings.MaxUserID; u++)
					if (ratings.CountByUser[u] > 0)
						complexity += (RegU * Math.Sqrt(ratings.CountByUser[u])) * BiasReg * Math.Pow(user_bias[u], 2);
				for (int i = 0; i <= MaxItemID; i++)
					complexity += y_reg[i] * Math.Pow(y.GetRow(i).EuclideanNorm(), 2);
				for (int i = 0; i <= ratings.MaxItemID; i++)
					if (ratings.CountByItem[i] > 0)
						complexity += (RegI * Math.Sqrt(ratings.CountByItem[i])) * BiasReg * Math.Pow(item_bias[i], 2);
			}
			else
			{
				for (int u = 0; u <= MaxUserID; u++)
					complexity += feedback_count_by_user[u] * RegU * Math.Pow(x.GetRow(u).EuclideanNorm(), 2);
				for (int u = 0; u <= ratings.MaxUserID; u++)
					complexity += ratings.CountByUser[u] * RegU * BiasReg * Math.Pow(user_bias[u], 2);
				for (int i = 0; i <= MaxItemID; i++)
					complexity += feedback_count_by_item[i] * RegI * Math.Pow(y.GetRow(i).EuclideanNorm(), 2);
				for (int i = 0; i <= ratings.MaxItemID; i++)
					complexity += ratings.CountByItem[i] * RegI * BiasReg * Math.Pow(item_bias[i], 2);
			}

			return (float) (ComputeLoss() + complexity);
		}

		///
		protected override float Predict(float[] user_vector, int item_id)
		{
			if (item_factors == null)
				PrecomputeItemFactors();
			return base.Predict(user_vector, item_id);
		}

		///
		protected override float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			var items_rated_by_user = (from t in rated_items select t.Item1).ToArray();

			var factors = y.SumOfRows(items_rated_by_user).ToArray();
			double norm_denominator = Math.Sqrt(items_rated_by_user.Length);
			for (int f = 0; f < factors.Length; f++)
				factors[f] = (float) (factors[f] / norm_denominator);

			var user_vector = new float[NumFactors + 1];
			user_vector[FOLD_IN_BIAS_INDEX] = 0;
			Array.Copy(factors, 0, user_vector, FOLD_IN_FACTORS_START, NumFactors);

			return user_vector;
		}

		///
		protected internal override void InitModel()
		{
			base.InitModel();

			x = new Matrix<float>(MaxUserID + 1, NumFactors);
			x.InitNormal(InitMean, InitStdDev);
			// set factors to zero for users without training examples
			for (int user_id = 0; user_id < x.NumberOfRows; user_id++)
				if (user_id > ratings.MaxUserID || ratings.CountByUser[user_id] == 0)
					x.SetRowToOneValue(user_id, 0);

			y = new Matrix<float>(MaxItemID + 1, NumFactors);
			y.InitNormal(InitMean, InitStdDev);
			// set factors to zero for items without training examples
			for (int item_id = 0; item_id < y.NumberOfRows; item_id++)
				if (item_id > ratings.MaxItemID || ratings.CountByItem[item_id] == 0)
					y.SetRowToOneValue(item_id, 0);
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

		/// <summary>Precompute all item factors</summary>
		protected void PrecomputeItemFactors()
		{
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

