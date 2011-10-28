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
using MyMediaLite.DataType;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Time-aware bias model</summary>
	/// <remarks>
	/// Model described in equation (12) of BellKor Grand Prize documentation for the Netflix Prize.
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
		IList<double> user_scaling; // c_u
		SparseMatrix<double> user_scaling_by_day;// c_ut

		// hyperparameters
		public uint NumIter { get; set; }
		public int BinSize { get; set; }
		public double Beta { get; set; }

		// parameter-specific learn rates
		public double UserBiasLearnRate { get; set; }
		public double ItemBiasLearnRate { get; set; }
		public double AlphaLearnRate { get; set; }
		public double ItemBiasByTimeBinLearnRate { get; set; }
		public double UserBiasByDayLearnRate { get; set; }
		public double UserScalingLearnRate { get; set; }
		public double UserScalingByDayLearnRate { get; set; }

		// parameter-specific regularization constants
		public double RegU { get; set; }
		public double RegI { get; set; }
		public double RegAlpha { get; set; }
		public double RegItemBiasByTimeBin { get; set; }
		public double RegUserBiasByDay { get; set; }
		public double RegUserScaling { get; set; }
		public double RegUserScalingByDay { get; set; }

		// helper data structures
		IList<double> user_mean_day;

		public TimeAwareBaseline()
		{
			NumIter = 30;

			BinSize = 70;

			Beta = 0.4;

			UserBiasLearnRate = 3000;
			ItemBiasLearnRate = 2000;
			AlphaLearnRate = 10;
			ItemBiasByTimeBinLearnRate = 5;
			UserBiasByDayLearnRate = 2500;
			UserScalingLearnRate = 8000;
			UserScalingByDayLearnRate = 2000;

			RegU = 300;
			RegI = 300;
			RegAlpha = 500000;
			RegItemBiasByTimeBin = 1000;
			RegUserBiasByDay = 50;
			RegUserScaling = 100;
			RegUserScalingByDay = 50;
		}

		public override void Train()
		{
			InitModel();

			global_average = ratings.Average;

			user_mean_day = new double[MaxUserID + 1];
			for (int i = 0; i < timed_ratings.Count; i++)
				user_mean_day[ratings.Users[i]] += (timed_ratings.LatestTime - timed_ratings.Times[i]).Days;
			for (int u = 0; u <= MaxUserID; u++)
				user_mean_day[u] /= ratings.CountByUser[u];

			for (int i = 0; i < NumIter; i++)
				Iterate();
		}

		protected virtual void InitModel()
		{
			Console.WriteLine(timed_ratings.EarliestTime);
			Console.WriteLine(timed_ratings.LatestTime);
			int number_of_days = (timed_ratings.LatestTime - timed_ratings.EarliestTime).Days;
			int number_of_bins = number_of_days / BinSize;
			Console.WriteLine("{0} days, {1} bins", number_of_days, number_of_bins);

			// initialize parameters
			user_bias = new double[Ratings.MaxUserID + 1];
			item_bias = new double[Ratings.MaxItemID + 1];
			alpha = new double[Ratings.MaxUserID + 1];
			item_bias_by_time_bin = new Matrix<double>(Ratings.MaxItemID + 1, number_of_bins);
			user_bias_by_day = new SparseMatrix<double>(Ratings.MaxUserID + 1, number_of_days);
			user_scaling = new double[Ratings.MaxUserID + 1];
			user_scaling_by_day = new SparseMatrix<double>(Ratings.MaxUserID + 1, number_of_days);
		}

		public virtual void Iterate()
		{
			foreach (int index in timed_ratings.RandomIndex)
			{
				int u = timed_ratings.Users[index];
				int i = timed_ratings.Items[index];
				int day = (timed_ratings.LatestTime - timed_ratings.Times[index]).Days;
				int bin = day / BinSize;

				// compute error
				double err = timed_ratings[index] - Predict(u, i, timed_ratings.Times[index]);

				// update user biases
				double dev_u = Math.Sign(day - user_mean_day[u]) * Math.Pow(Math.Abs(day - user_mean_day[u]), Beta);
				alpha[u]                 += 0.5 * AlphaLearnRate         * (err * dev_u - RegAlpha         * alpha[u]);
				user_bias[u]             += 0.5 * UserBiasLearnRate      * (err         - RegU             * user_bias[u]);
				user_bias_by_day[u, day] += 0.5 * UserBiasByDayLearnRate * (err         - RegUserBiasByDay * user_bias_by_day[u, day]);

				// update item biases and user scalings
				double b_i  = item_bias[i];
				double b_ib = item_bias_by_time_bin[i, bin];
				double c_u  = user_scaling[u];
				double c_ud = user_scaling_by_day[u, day];
				item_bias[i]                  += 0.5 * ItemBiasLearnRate          * (err * (c_u + c_ud) - RegI                 * b_i);
				item_bias_by_time_bin[i, bin] += 0.5 * ItemBiasByTimeBinLearnRate * (err * (c_u + c_ud) - RegItemBiasByTimeBin * b_ib);
				user_scaling[u]               += 0.5 * UserScalingLearnRate       * (err * (b_i + b_ib) - RegUserScaling       * c_u);
				user_scaling_by_day[u, day]   += 0.5 * UserScalingByDayLearnRate  * (err * (b_i + b_ib) - RegUserScalingByDay  * c_ud);
			}
		}

		public override double Predict(int user_id, int item_id)
		{
			double result = global_average;
			if (user_id <= MaxUserID)
				result += user_bias[user_id];
			if (item_id <= MaxItemID)
				result += item_bias[item_id];

			return result;
		}

		public override double Predict(int user_id, int item_id, DateTime time)
		{
			int day = (timed_ratings.LatestTime - time).Days;
			int bin = day / BinSize;

			double result = global_average;
			if (user_id <= MaxUserID)
			{
				double dev_u = Math.Sign(day - user_mean_day[user_id]) * Math.Pow(Math.Abs(day - user_mean_day[user_id]), Beta);
				result += user_bias[user_id] + alpha[user_id] * dev_u + user_bias_by_day[user_id, day];
			}

			if (item_id <= MaxItemID && user_id > MaxUserID)
				result += item_bias[item_id] + item_bias_by_time_bin[item_id, bin];
			if (item_id <= MaxItemID && user_id <= MaxUserID)
				result += item_bias[item_id] + item_bias_by_time_bin[item_id, bin];// * (user_scaling[user_id] + user_scaling_by_day[user_id, day]);

			return result;
		}

		public double ComputeFit()
		{
			return -1;
		}
	}
}

