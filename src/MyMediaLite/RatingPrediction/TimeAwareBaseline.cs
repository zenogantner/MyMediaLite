// Copyright (C) 2011 Zeno Gantner
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

		double global_average;
		IList<double> user_bias;
		IList<double> item_bias;
		IList<double> alpha;
		Matrix<double> item_bias_by_time_bin;  // items in rows, bins in columns
		SparseMatrix<double> user_bias_by_day; // users in rows, days in columns
		IList<double> user_scaling;               // c_u
		SparseMatrix<double> user_scaling_by_day; // c_ut

		// hyperparameters

		/// <summary>number of iterations over the dataset to perform</summary>
		public uint NumIter { get; set; }
		/// <summary>bin size in days for modeling the time-dependent item bias</summary>
		public int BinSize { get; set; }

		/// <summary>beta parameter for modeling the drift in the user bias</summary>
		public double Beta { get; set; }

		// parameter-specific learn rates

		/// <summary>learn rate for the user bias</summary>
		public double UserBiasLearnRate { get; set; }

		/// <summary>learn rate for the item bias</summary>
		public double ItemBiasLearnRate { get; set; }

		/// <summary>learn rate for the user-wise alphas</summary>
		public double AlphaLearnRate { get; set; }
		/// <summary>learn rate for the bin-wise item bias</summary>
		public double ItemBiasByTimeBinLearnRate { get; set; }

		/// <summary>learn rate for the day-wise user bias</summary>
		public double UserBiasByDayLearnRate { get; set; }

		/// <summary>learn rate for the user-wise scaling factor</summary>
		public double UserScalingLearnRate { get; set; }

		/// <summary>learn rate for the day-wise user scaling factor</summary>
		public double UserScalingByDayLearnRate { get; set; }

		// parameter-specific regularization constants

		/// <summary>regularization for the user bias</summary>
		public double RegU { get; set; }
		/// <summary>regularization for the item bias</summary>
		public double RegI { get; set; }

		/// <summary>regularization for the user-wise alphas</summary>
		public double RegAlpha { get; set; }

		/// <summary>regularization for the bin-wise item bias</summary>
		public double RegItemBiasByTimeBin { get; set; }

		/// <summary>regularization for the day-wise user bias</summary>
		public double RegUserBiasByDay { get; set; }

		/// <summary>regularization for the user scaling factor</summary>
		public double RegUserScaling { get; set; }

		/// <summary>regularization for the day-wise user scaling factor</summary>
		public double RegUserScalingByDay { get; set; }

		// helper data structures
		IList<double> user_mean_day;

		/// <summary>default constructor</summary>
		public TimeAwareBaseline()
		{
			NumIter = 30;

			BinSize = 70;

			Beta = 0.4;

			UserBiasLearnRate = 0.003;
			ItemBiasLearnRate = 0.002;
			AlphaLearnRate = 0.00001;
			ItemBiasByTimeBinLearnRate = 0.000005;
			UserBiasByDayLearnRate = 0.0025;
			UserScalingLearnRate = 0.008;
			UserScalingByDayLearnRate = 0.002;

			RegU = 0.03;
			RegI = 0.03;
			RegAlpha = 50;
			RegItemBiasByTimeBin = 0.1;
			RegUserBiasByDay = 0.005;
			RegUserScaling = 0.01;
			RegUserScalingByDay = 0.005;
		}

		///
		public override void Train()
		{
			InitModel();

			global_average = ratings.Average;

			user_mean_day = new double[MaxUserID + 1];
			for (int i = 0; i < timed_ratings.Count; i++)
				user_mean_day[ratings.Users[i]] += (timed_ratings.LatestTime - timed_ratings.Times[i]).Days;
			for (int u = 0; u <= MaxUserID; u++)
				if (ratings.CountByUser[u] != 0)
					user_mean_day[u] /= ratings.CountByUser[u];

			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		/// <summary>Initialize the model parameters</summary>
		protected virtual void InitModel()
		{
			int number_of_days = (timed_ratings.LatestTime - timed_ratings.EarliestTime).Days;
			int number_of_bins = number_of_days / BinSize + 1;
			Console.WriteLine("{0} days, {1} bins", number_of_days, number_of_bins);

			// initialize parameters
			user_bias = new double[MaxUserID + 1];
			item_bias = new double[MaxItemID + 1];
			alpha = new double[MaxUserID + 1];
			item_bias_by_time_bin = new Matrix<double>(MaxItemID + 1, number_of_bins);
			user_bias_by_day = new SparseMatrix<double>(MaxUserID + 1, number_of_days);
			user_scaling = new double[MaxUserID + 1];
			user_scaling_by_day = new SparseMatrix<double>(MaxUserID + 1, number_of_days);
		}

		///
		public virtual void Iterate()
		{
			foreach (int index in timed_ratings.RandomIndex)
			{
				int u = timed_ratings.Users[index];
				int i = timed_ratings.Items[index];
				int day = (timed_ratings.LatestTime - timed_ratings.Times[index]).Days;
				int bin = day / BinSize;

				// compute error
				double err = timed_ratings[index] - Predict(u, i, day, bin);

				UpdateParameters(u, i, day, bin, err);
			}
		}

		/// <summary>Single SGD step: update the parameter values for one user and one item</summary>
		/// <param name='u'>the user ID</param>
		/// <param name='i'>the item ID</param>
		/// <param name='day'>the day of the rating</param>
		/// <param name='bin'>the day bin of the rating</param>
		/// <param name='err'>the current error made for this rating</param>
		protected virtual void UpdateParameters(int u, int i, int day, int bin, double err)
		{
			// update user biases
			double dev_u = Math.Sign(day - user_mean_day[u]) * Math.Pow(Math.Abs(day - user_mean_day[u]), Beta);
			alpha[u]                 += 2 * AlphaLearnRate         * (err * dev_u - RegAlpha         * alpha[u]);
			user_bias[u]             += 2 * UserBiasLearnRate      * (err         - RegU             * user_bias[u]);
			user_bias_by_day[u, day] += 2 * UserBiasByDayLearnRate * (err         - RegUserBiasByDay * user_bias_by_day[u, day]);

			// update item biases and user scalings
			double b_i  = item_bias[i];
			double b_ib = item_bias_by_time_bin[i, bin];
			double c_u  = user_scaling[u];
			double c_ud = user_scaling_by_day[u, day];
			item_bias[i]                  += 2 * ItemBiasLearnRate          * (err * (c_u + c_ud) - RegI                 * b_i);
			item_bias_by_time_bin[i, bin] += 2 * ItemBiasByTimeBinLearnRate * (err * (c_u + c_ud) - RegItemBiasByTimeBin * b_ib);
			user_scaling[u]               += 2 * UserScalingLearnRate       * (err * (b_i + b_ib) - RegUserScaling       * (c_u - 1));
			user_scaling_by_day[u, day]   += 2 * UserScalingByDayLearnRate  * (err * (b_i + b_ib) - RegUserScalingByDay  * c_ud);
		}

		///
		public override double Predict(int user_id, int item_id)
		{
			double result = global_average;
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
		protected virtual double Predict(int user_id, int item_id, int day, int bin)
		{
			double result = global_average;

			double dev_u = Math.Sign(day - user_mean_day[user_id]) * Math.Pow(Math.Abs(day - user_mean_day[user_id]), Beta);
			result += user_bias[user_id] + alpha[user_id] * dev_u + user_bias_by_day[user_id, day];
			result += (item_bias[item_id] + item_bias_by_time_bin[item_id, bin]) * (user_scaling[user_id] + user_scaling_by_day[user_id, day]);

			return result;
		}

		///
		public override double Predict(int user_id, int item_id, DateTime time)
		{
			int day = (time - timed_ratings.EarliestTime).Days;
			int bin = day / BinSize;
			
			// use latest day bin if the rating time is after the training time period
			if (bin >= item_bias_by_time_bin.NumberOfColumns)
				bin = item_bias_by_time_bin.NumberOfColumns - 1;

			double result = global_average;
			if (user_id <= MaxUserID)
			{
				double dev_u = Math.Sign(day - user_mean_day[user_id]) * Math.Pow(Math.Abs(day - user_mean_day[user_id]), Beta);
				result += user_bias[user_id] + alpha[user_id] * dev_u;
				if (day <= timed_ratings.LatestTime.Day)
					result += user_bias_by_day[user_id, day];
			}

			if (item_id <= MaxItemID && user_id > MaxUserID)
				result += item_bias[item_id] + item_bias_by_time_bin[item_id, bin];
			if (item_id <= MaxItemID && user_id <= MaxUserID && day < user_scaling_by_day.NumberOfColumns)
				result += (item_bias[item_id] + item_bias_by_time_bin[item_id, bin]) * (user_scaling[user_id] + user_scaling_by_day[user_id, day]);

			return result;
		}

		///
		public virtual double ComputeFit()
		{
			double loss =
				this.Evaluate(ratings)["RMSE"]
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

			return loss;
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

