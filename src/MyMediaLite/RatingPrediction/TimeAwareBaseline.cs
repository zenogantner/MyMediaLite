// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.Eval;
using MyMediaLite.DataType;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Time-aware bias model</summary>
	/// <remarks>
	/// Model described in equation (10) of BellKor Grand Prize documentation for the Netflix Prize (see below).
	/// The optimization problem is described in equation (12).
	///
	/// The default hyper-parameter values are set to the ones shown in the report.
	/// For datasets other than Netflix, you may want to find better parameters.
	///
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///   Yehuda Koren: The BellKor Solution to the Netflix Grand Prize
	///   </description></item>
	/// </list>
	///
	/// This recommender does currently NOT support incremental updates.
	/// </remarks>
	public class TimeAwareBaseline : TimeAwareRatingPredictor, IIterativeModel
	{
		// parameters

		float global_average;
		IList<float> user_bias;
		IList<float> item_bias;
		IList<float> alpha;
		Matrix<float> item_bias_by_time_bin;  // items in rows, bins in columns
		SparseMatrix<float> user_bias_by_day; // users in rows, days in columns
		IList<float> user_scaling;               // c_u
		SparseMatrix<float> user_scaling_by_day; // c_ut

		// hyperparameters

		/// <summary>number of iterations over the dataset to perform</summary>
		public uint NumIter { get; set; }
		/// <summary>bin size in days for modeling the time-dependent item bias</summary>
		public int BinSize { get; set; }

		/// <summary>beta parameter for modeling the drift in the user bias</summary>
		public float Beta { get; set; }

		// parameter-specific learn rates

		/// <summary>learn rate for the user bias</summary>
		public float UserBiasLearnRate { get; set; }

		/// <summary>learn rate for the item bias</summary>
		public float ItemBiasLearnRate { get; set; }

		/// <summary>learn rate for the user-wise alphas</summary>
		public float AlphaLearnRate { get; set; }
		/// <summary>learn rate for the bin-wise item bias</summary>
		public float ItemBiasByTimeBinLearnRate { get; set; }

		/// <summary>learn rate for the day-wise user bias</summary>
		public float UserBiasByDayLearnRate { get; set; }

		/// <summary>learn rate for the user-wise scaling factor</summary>
		public float UserScalingLearnRate { get; set; }

		/// <summary>learn rate for the day-wise user scaling factor</summary>
		public float UserScalingByDayLearnRate { get; set; }

		// parameter-specific regularization constants

		/// <summary>regularization for the user bias</summary>
		public float RegU { get; set; }
		/// <summary>regularization for the item bias</summary>
		public float RegI { get; set; }

		/// <summary>regularization for the user-wise alphas</summary>
		public float RegAlpha { get; set; }

		/// <summary>regularization for the bin-wise item bias</summary>
		public float RegItemBiasByTimeBin { get; set; }

		/// <summary>regularization for the day-wise user bias</summary>
		public float RegUserBiasByDay { get; set; }

		/// <summary>regularization for the user scaling factor</summary>
		public float RegUserScaling { get; set; }

		/// <summary>regularization for the day-wise user scaling factor</summary>
		public float RegUserScalingByDay { get; set; }

		// helper data structures
		IList<float> user_mean_day;
		/// <summary>last day in the training data, counting from the first day</summary>
		protected int latest_relative_day;

		/// <summary>default constructor</summary>
		public TimeAwareBaseline()
		{
			NumIter = 30;

			BinSize = 70;

			Beta = 0.4f;

			UserBiasLearnRate = 0.003f;
			ItemBiasLearnRate = 0.002f;
			AlphaLearnRate = 0.00001f;
			ItemBiasByTimeBinLearnRate = 0.000005f;
			UserBiasByDayLearnRate = 0.0025f;
			UserScalingLearnRate = 0.008f;
			UserScalingByDayLearnRate = 0.002f;

			RegU = 0.03f;
			RegI = 0.03f;
			RegAlpha = 50;
			RegItemBiasByTimeBin = 0.1f;
			RegUserBiasByDay = 0.005f;
			RegUserScaling = 0.01f;
			RegUserScalingByDay = 0.005f;
		}

		///
		public override void Train()
		{
			InitModel();

			global_average = ratings.Average;
			latest_relative_day = RelativeDay(timed_ratings.LatestTime);

			// compute mean day of rating by user
			user_mean_day = new float[MaxUserID + 1];
			for (int i = 0; i < timed_ratings.Count; i++)
				user_mean_day[ratings.Users[i]] += RelativeDay(timed_ratings.Times[i]);
			for (int u = 0; u <= MaxUserID; u++)
				if (ratings.CountByUser[u] != 0)
					user_mean_day[u] /= ratings.CountByUser[u];
				else // no ratings yet?
					user_mean_day[u] = RelativeDay(timed_ratings.LatestTime); // set to latest day

			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Given a DateTime object, return the day relative to the first rating day in the dataset</summary>
		/// <returns>the day relative to the first rating day in the dataset</returns>
		/// <param name='datetime'>the date/time of the rating event</param>
		protected int RelativeDay(DateTime datetime)
		{
			return (datetime - timed_ratings.EarliestTime).Days;
		}

		/// <summary>Initialize the model parameters</summary>
		protected virtual void InitModel()
		{
			int number_of_days = (timed_ratings.LatestTime - timed_ratings.EarliestTime).Days;
			int number_of_bins = number_of_days / BinSize + 1;
			Console.WriteLine("{0} days, {1} bins", number_of_days, number_of_bins);

			// initialize parameters
			user_bias = new float[MaxUserID + 1];
			item_bias = new float[MaxItemID + 1];
			alpha = new float[MaxUserID + 1];
			item_bias_by_time_bin = new Matrix<float>(MaxItemID + 1, number_of_bins);
			user_bias_by_day = new SparseMatrix<float>(MaxUserID + 1, number_of_days);
			user_scaling = new float[MaxUserID + 1];
			user_scaling.Init(1f);
			user_scaling_by_day = new SparseMatrix<float>(MaxUserID + 1, number_of_days);
		}

		///
		public virtual void Iterate()
		{
			foreach (int index in timed_ratings.RandomIndex)
			{
				int u = timed_ratings.Users[index];
				int i = timed_ratings.Items[index];
				int day = RelativeDay(timed_ratings.Times[index]);
				int bin = day / BinSize;

				// compute error
				float err = timed_ratings[index] - Predict(u, i, day, bin);

				UpdateParameters(u, i, day, bin, err);
			}
		}

		/// <summary>Single stochastic gradient descent step: update the parameter values for one user and one item</summary>
		/// <param name='u'>the user ID</param>
		/// <param name='i'>the item ID</param>
		/// <param name='day'>the day of the rating</param>
		/// <param name='bin'>the day bin of the rating</param>
		/// <param name='err'>the current error made for this rating</param>
		protected virtual void UpdateParameters(int u, int i, int day, int bin, float err)
		{
			// update user biases
			double dev_u = Math.Sign(day - user_mean_day[u]) * Math.Pow(Math.Abs(day - user_mean_day[u]), Beta);
			alpha[u]                 += (float) (AlphaLearnRate         * (err * dev_u - RegAlpha         * alpha[u]));
			user_bias[u]             += (float) (UserBiasLearnRate      * (err         - RegU             * user_bias[u]));
			user_bias_by_day[u, day] += (float) (UserBiasByDayLearnRate * (err         - RegUserBiasByDay * user_bias_by_day[u, day]));

			// update item biases and user scalings
			float b_i  = item_bias[i];
			float b_ib = item_bias_by_time_bin[i, bin];
			float c_u  = user_scaling[u];
			float c_ud = user_scaling_by_day[u, day];
			item_bias[i]                  += (float) (ItemBiasLearnRate          * (err * (c_u + c_ud) - RegI                 * b_i));
			item_bias_by_time_bin[i, bin] += (float) (ItemBiasByTimeBinLearnRate * (err * (c_u + c_ud) - RegItemBiasByTimeBin * b_ib));
			user_scaling[u]               += (float) (UserScalingLearnRate       * (err * (b_i + b_ib) - RegUserScaling       * (c_u - 1)));
			user_scaling_by_day[u, day]   += (float) (UserScalingByDayLearnRate  * (err * (b_i + b_ib) - RegUserScalingByDay  * c_ud));
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			float result = global_average;
			if (user_id <= MaxUserID)
				result += user_bias[user_id];
			if (item_id <= MaxItemID)
				result += item_bias[item_id];

			return result;
		}

		/// <summary>Predict the specified user_id, item_id, day and bin</summary>
		/// <remarks>
		/// Assumes user and item IDs are valid.
		/// </remarks>
		/// <param name='user_id'>the user ID</param>
		/// <param name='item_id'>the item ID</param>
		/// <param name='day'>the day of the rating</param>
		/// <param name='bin'>the day bin of the rating</param>
		protected virtual float Predict(int user_id, int item_id, int day, int bin)
		{
			double result = global_average;

			double dev_u = Math.Sign(day - user_mean_day[user_id]) * Math.Pow(Math.Abs(day - user_mean_day[user_id]), Beta);
			result += user_bias[user_id] + alpha[user_id] * dev_u + user_bias_by_day[user_id, day];
			result += (item_bias[item_id] + item_bias_by_time_bin[item_id, bin]) * (user_scaling[user_id] + user_scaling_by_day[user_id, day]);

			return (float) result;
		}

		///
		public override float Predict(int user_id, int item_id, DateTime time)
		{
			int day = RelativeDay(time);
			int bin = day / BinSize;

			// use latest day bin if the rating time is after the training time period
			if (bin >= item_bias_by_time_bin.NumberOfColumns)
				bin = item_bias_by_time_bin.NumberOfColumns - 1;

			double result = global_average;
			double scaling = 1;
			if (user_id <= MaxUserID)
			{
				double dev_u = Math.Sign(day - user_mean_day[user_id]) * Math.Pow(Math.Abs(day - user_mean_day[user_id]), Beta);
				result += user_bias[user_id] + alpha[user_id] * dev_u;
				if (day <= latest_relative_day)
					result += user_bias_by_day[user_id, day];

				scaling = user_scaling[user_id];
				if (day < user_scaling_by_day.NumberOfColumns)
					scaling += user_scaling_by_day[user_id, day];
			}

			if (item_id <= MaxItemID)
				result += (item_bias[item_id] + item_bias_by_time_bin[item_id, bin]) * scaling;

			return (float) result;
		}

		///
		public virtual float ComputeObjective()
		{
			double loss =
				Eval.Measures.RMSE.ComputeSquaredErrorSum(this, ratings)
				+ RegU                 * Math.Pow(user_bias.EuclideanNorm(),             2)
				+ RegI                 * Math.Pow(item_bias.EuclideanNorm(),             2)
 				+ RegAlpha             * Math.Pow(alpha.EuclideanNorm(),                 2)
				+ RegUserBiasByDay     * Math.Pow(user_bias_by_day.FrobeniusNorm(),      2)
				+ RegItemBiasByTimeBin * Math.Pow(item_bias_by_time_bin.FrobeniusNorm(), 2)
				+ RegUserScalingByDay  * Math.Pow(user_scaling_by_day.FrobeniusNorm(),   2);

			double user_scaling_reg_term = 0;
			foreach (var e in user_scaling)
				user_scaling_reg_term += Math.Pow(1 - e, 2);
			user_scaling_reg_term *= RegUserScaling;
			loss += user_scaling_reg_term;

			return (float) loss;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} num_iter={1} bin_size={2} beta={3} user_bias_learn_rate={4} item_bias_learn_rate={5} "
					+ "alpha_learn_rate={6} item_bias_by_time_bin_learn_rate={7} user_bias_by_day_learn_rate={8} "
					+ "user_scaling_learn_rate={9} user_scaling_by_day_learn_rate={10} "
					+ "reg_u={11} reg_i={12} reg_alpha={13} reg_item_bias_by_time_bin={14} reg_user_bias_by_day={15} "
					+ "reg_user_scaling={16} reg_user_scaling_by_day={17}",
				this.GetType().Name,
				NumIter, BinSize, Beta, UserBiasLearnRate, ItemBiasLearnRate, AlphaLearnRate,
				ItemBiasByTimeBinLearnRate, UserBiasByDayLearnRate, UserScalingLearnRate, UserScalingByDayLearnRate,
				RegU, RegI, RegAlpha, RegItemBiasByTimeBin, RegUserBiasByDay,
				RegUserScaling, RegUserScalingByDay);
		}
	}
}

