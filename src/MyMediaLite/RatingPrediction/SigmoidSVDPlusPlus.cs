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
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>SVD++: Matrix factorization that also takes into account _what_ users have rated; variant that uses a sigmoid function</summary>
	/// <remarks>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Yehuda Koren:
	///         Factorization Meets the Neighborhood: a Multifaceted Collaborative Filtering Model.
	///         KDD 2008.
	///         http://research.yahoo.com/files/kdd08koren.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public class SigmoidSVDPlusPlus : SVDPlusPlus, ITransductiveRatingPredictor
	{
		/// <summary>size of the interval of valid ratings</summary>
		float rating_range_size;

		Func<double, double, float> compute_gradient_common;

		/// <summary>The optimization target</summary>
		public OptimizationTarget Loss { get; set; }

		/// <summary>Default constructor</summary>
		public SigmoidSVDPlusPlus() : base()
		{
			Regularization = 0.015f;
			LearnRate = 0.001f;
			BiasLearnRate = 0.7f;
			BiasReg = 0.33f;
		}

		///
		public override void Train()
		{
			rating_range_size = max_rating - min_rating;

			// compute global bias
			double avg = (ratings.Average - min_rating) / rating_range_size;
			global_bias = (float) Math.Log(avg / (1 - avg));

			base.Train();
		}

		void SetupLoss()
		{
			switch (Loss)
			{
				case OptimizationTarget.MAE:
					compute_gradient_common = (sig_score, err) => (float) (Math.Sign(err) * sig_score * (1 - sig_score) * rating_range_size);
					break;
				case OptimizationTarget.RMSE:
					compute_gradient_common = (sig_score, err) => (float) (err * sig_score * (1 - sig_score) * rating_range_size);
					break;
				case OptimizationTarget.LogisticLoss:
					compute_gradient_common = (sig_score, err) => (float) err;
					break;
			}
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			double score = global_bias;

			if (user_factors == null)
				PrecomputeUserFactors();

			if (user_id <= MaxUserID)
				score += user_bias[user_id];
			if (item_id <= MaxItemID)
				score += item_bias[item_id];
			if (user_id <= MaxUserID && item_id <= MaxItemID)
				score += DataType.MatrixExtensions.RowScalarProduct(user_factors, user_id, item_factors, item_id);

			double sig_score = 1 / (1 + Math.Exp(-score));
			double prediction = min_rating + sig_score * rating_range_size;

			return (float) prediction;
		}

		///
		protected override void Iterate(IList<int> rating_indices, bool update_user, bool update_item)
		{
			SetupLoss();

			user_factors = null; // delete old user factors
			float reg = Regularization; // to limit property accesses

			foreach (int index in rating_indices)
			{
				int u = ratings.Users[index];
				int i = ratings.Items[index];

				double score = global_bias + user_bias[u] + item_bias[i];
				var p_plus_y_sum_vector = y.SumOfRows(items_rated_by_user[u]);
				double norm_denominator = Math.Sqrt(items_rated_by_user[u].Length);
				for (int f = 0; f < p_plus_y_sum_vector.Count; f++)
					p_plus_y_sum_vector[f] = (float) (p_plus_y_sum_vector[f] / norm_denominator + p[u, f]);

				score += DataType.MatrixExtensions.RowScalarProduct(item_factors, i, p_plus_y_sum_vector);
				double sig_score = 1 / (1 + Math.Exp(-score));

				double prediction = min_rating + sig_score * rating_range_size;
				double err = ratings[index] - prediction;

				float user_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByUser[u])) : reg;
				float item_reg_weight = FrequencyRegularization ? (float) (reg / Math.Sqrt(ratings.CountByItem[i])) : reg;
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
					if (update_user)
					{
						double delta_u = gradient_common * i_f - user_reg_weight * p[u, f];
						p.Inc(u, f, current_learnrate * delta_u);
					}
					if (update_item)
					{
						double delta_i = gradient_common * p_plus_y_sum_vector[f] - item_reg_weight * i_f;
						item_factors.Inc(i, f, current_learnrate * delta_i);

						double common_update = normalized_gradient_common * i_f;
						foreach (int other_item_id in items_rated_by_user[u])
						{
							double delta_oi = common_update - y_reg[other_item_id] * y[other_item_id, f];
							y.Inc(other_item_id, f, current_learnrate * delta_oi);
						}
					}
				}
			}

			UpdateLearnRate();
		}

		///
		public override void LoadModel(string filename)
		{
			base.LoadModel(filename);
			rating_range_size = max_rating - min_rating;
		}

		///
		protected override float[] FoldIn(IList<Tuple<int, float>> rated_items)
		{
			SetupLoss();

			var user_p = new float[NumFactors];
			user_p.InitNormal(InitMean, InitStdDev);
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
					score += DataType.MatrixExtensions.RowScalarProduct(item_factors, item_id, y_sum_vector);
					score += DataType.MatrixExtensions.RowScalarProduct(item_factors, item_id, user_p);
					double sig_score = 1 / (1 + Math.Exp(-score));
					double prediction = min_rating + sig_score * rating_range_size;
					float err = (float) (rated_items[index].Item2 - prediction);
					float gradient_common = compute_gradient_common(sig_score, err);

					// adjust bias
					user_bias += BiasLearnRate * LearnRate * ((float) gradient_common - BiasReg * user_reg_weight * user_bias);

					// adjust factors
					for (int f = 0; f < NumFactors; f++)
					{
						float u_f = user_p[f];
						float i_f = item_factors[item_id, f];

						double delta_u = gradient_common * i_f - user_reg_weight * u_f;
						user_p[f] += (float) (LearnRate * delta_u);
					}
				}
			}

			// assign final parameter values to return vector
			var user_vector = new float[NumFactors + 1];
			user_vector[0] = user_bias;
			for (int f = 0; f < NumFactors; f++)
				user_vector[f + 1] = (float) y_sum_vector[f] + user_p[f];

			return user_vector;
		}

		/// <summary>Compute the value of the loss function that is currently being optimized</summary>
		/// <returns>the loss</returns>
		protected override double ComputeLoss()
		{
			double loss = 0;
			switch (Loss)
			{
				case OptimizationTarget.MAE:
					loss += Eval.Measures.MAE.ComputeAbsoluteErrorSum(this, ratings);
					break;
				case OptimizationTarget.RMSE:
					loss += Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings);
					break;
				case OptimizationTarget.LogisticLoss:
					loss += Eval.Measures.LogisticLoss.ComputeSum(this, ratings, min_rating, rating_range_size);
					break;
			}
			return loss;
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

