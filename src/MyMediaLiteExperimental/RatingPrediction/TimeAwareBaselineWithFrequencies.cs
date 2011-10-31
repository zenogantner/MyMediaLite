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
using System.Globalization;
using System.Linq;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Time-aware bias model with frequencies</summary>
	/// <remarks>
	/// Model described in equation (11) of BellKor Grand Prize documentation for the Netflix Prize (see below).
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
	public class TimeAwareBaselineWithFrequencies : TimeAwareBaseline
	{
		// additional parameters
		SparseMatrix<double> item_bias_at_frequency;

		// additional hyper-parameters

		/// <summary>logarithmic base for the frequency counts</summary>
		public double FrequencyLogBase { get; set; }

		/// <summary>regularization constant for b_{i, f_{ui}}</summary>
		public double RegItemBiasAtFrequency { get; set; }

		/// <summary>learn rate for b_{i, f_{ui}}</summary>
		public double ItemBiasAtFrequencyLearnRate { get; set; }

		// additional helper data structures
		SparseMatrix<int> log_frequency_by_day { get; set; }

		/// <summary>Default constructor</summary>
		public TimeAwareBaselineWithFrequencies()
		{
			NumIter = 40;

			FrequencyLogBase = 6.76;

			BinSize = 70;

			Beta = 0.4;

			UserBiasLearnRate = 0.00267;
			ItemBiasLearnRate = 0.000488;
			AlphaLearnRate = 0.00000311;
			ItemBiasByTimeBinLearnRate = 0.00000115;
			UserBiasByDayLearnRate = 0.000257;
			UserScalingLearnRate = 0.00564;
			UserScalingByDayLearnRate = 0.00103;
			ItemBiasAtFrequencyLearnRate = 0.00236;

			RegU = 0.0255;
			RegI = 0.0255;
			RegAlpha = 3.95;
			RegItemBiasByTimeBin = 0.0929;
			RegUserBiasByDay = 0.00231;
			RegUserScaling = 0.0476;
			RegUserScalingByDay = 0.019;
			RegItemBiasAtFrequency = 0.000000011;
		}

		///
		public override void Train()
		{
			int number_of_days = (timed_ratings.LatestTime - timed_ratings.EarliestTime).Days;

			// compute log rating frequencies
			log_frequency_by_day = new SparseMatrix<int>(MaxUserID + 1, number_of_days);
			// first count the frequencies ...
			for (int i = 0; i < timed_ratings.Count; i++)
			{
				int day = (timed_ratings.LatestTime - timed_ratings.Times[i]).Days;
				log_frequency_by_day[timed_ratings.Users[i], day]++;
			}
			// ... then apply (rounded) logarithm
			foreach (var index_pair in log_frequency_by_day.NonEmptyEntryIDs)
				log_frequency_by_day[index_pair.First, index_pair.Second]
					= (int) Math.Ceiling(Math.Log(log_frequency_by_day[index_pair.First, index_pair.Second], FrequencyLogBase));

			base.Train();
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			item_bias_at_frequency = new SparseMatrix<double>(MaxItemID + 1, SparseMatrixUtils.Max(log_frequency_by_day));
		}
		
		///
		protected override void UpdateParameters(int u, int i, int day, int bin, double err)
		{
			base.UpdateParameters(u, i, day, bin, err);

			// update additional bias
			int f = log_frequency_by_day[u, day];
			double b_i_f_ui  = item_bias_at_frequency[i, f];
			item_bias_at_frequency[i, f] += 2 * ItemBiasAtFrequencyLearnRate  * (err * b_i_f_ui - RegItemBiasAtFrequency * b_i_f_ui);
		}

		///
		protected override double Predict (int user_id, int item_id, int day, int bin)
		{
			double result = base.Predict (user_id, item_id, day, bin);
			result += item_bias_at_frequency[item_id, log_frequency_by_day[user_id, day]];

			return result;
		}

		///
		public override double Predict(int user_id, int item_id, DateTime time)
		{
			double result = base.Predict(user_id, item_id, time);
			int day = (timed_ratings.LatestTime - time).Days;
			result += item_bias_at_frequency[item_id, log_frequency_by_day[user_id, day]];

			return result;
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
				+ "reg_user_scaling={16} reg_user_scaling_by_day={17} "
				+ "frequency_log_base={18} item_bias_at_frequency_learn_rate={19} reg_item_bias_at_frequency={20}",
				this.GetType().Name,
				NumIter, BinSize, Beta, UserBiasLearnRate, ItemBiasLearnRate, AlphaLearnRate,
				ItemBiasByTimeBinLearnRate, UserBiasByDayLearnRate, UserScalingLearnRate, UserScalingByDayLearnRate,
				RegU, RegI, RegAlpha, RegItemBiasByTimeBin, RegUserBiasByDay,
				RegUserScaling, RegUserScalingByDay, FrequencyLogBase, ItemBiasAtFrequencyLearnRate, RegItemBiasAtFrequency);
		}
	}
}

