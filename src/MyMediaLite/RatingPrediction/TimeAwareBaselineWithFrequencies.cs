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
		SparseMatrix<float> item_bias_at_frequency;

		// additional hyper-parameters

		/// <summary>logarithmic base for the frequency counts</summary>
		public float FrequencyLogBase { get; set; }

		/// <summary>regularization constant for b_{i, f_{ui}}</summary>
		public float RegItemBiasAtFrequency { get; set; }

		/// <summary>learn rate for b_{i, f_{ui}}</summary>
		public float ItemBiasAtFrequencyLearnRate { get; set; }

		// additional helper data structures
		SparseMatrix<int> log_frequency_by_day { get; set; }

		/// <summary>Default constructor</summary>
		public TimeAwareBaselineWithFrequencies()
		{
			NumIter = 40;
			FrequencyLogBase = 6.76f;
			BinSize = 70;
			Beta = 0.4f;

			UserBiasLearnRate = 0.00267f;
			ItemBiasLearnRate = 0.000488f;
			AlphaLearnRate = 0.00000311f;
			ItemBiasByTimeBinLearnRate = 0.000115f;
			UserBiasByDayLearnRate = 0.000257f;
			UserScalingLearnRate = 0.00564f;
			UserScalingByDayLearnRate = 0.00103f;
			ItemBiasAtFrequencyLearnRate = 0.00236f;

			RegU = 0.0255f;
			RegI = 0.0255f;
			RegAlpha = 3.95f;
			RegItemBiasByTimeBin = 0.0929f;
			RegUserBiasByDay = 0.00231f;
			RegUserScaling = 0.0476f;
			RegUserScalingByDay = 0.019f;
			RegItemBiasAtFrequency = 0.000000011f;
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
				int day = RelativeDay(timed_ratings.Times[i]);
				log_frequency_by_day[timed_ratings.Users[i], day]++;
			}
			// ... then apply (rounded) logarithm
			foreach (var index_pair in log_frequency_by_day.NonEmptyEntryIDs)
				log_frequency_by_day[index_pair.Item1, index_pair.Item2]
					= (int) Math.Ceiling(Math.Log(log_frequency_by_day[index_pair.Item1, index_pair.Item2], FrequencyLogBase));

			base.Train();
		}

		///
		protected override void InitModel()
		{
			base.InitModel();

			item_bias_at_frequency = new SparseMatrix<float>(MaxItemID + 1, log_frequency_by_day.Max());
		}

		///
		protected override void UpdateParameters(int u, int i, int day, int bin, float err)
		{
			base.UpdateParameters(u, i, day, bin, err);

			// update additional bias
			int f = log_frequency_by_day[u, day];
			float b_i_f_ui  = item_bias_at_frequency[i, f];
			item_bias_at_frequency[i, f] += (float) (ItemBiasAtFrequencyLearnRate * (err * b_i_f_ui - RegItemBiasAtFrequency * b_i_f_ui));
		}

		///
		protected override float Predict(int user_id, int item_id, int day, int bin)
		{
			float result = base.Predict(user_id, item_id, day, bin);
			if (day <= latest_relative_day)
				result += item_bias_at_frequency[item_id, log_frequency_by_day[user_id, day]];

			return result;
		}

		///
		public override float Predict(int user_id, int item_id, DateTime time)
		{
			float result = base.Predict(user_id, item_id, time);
			int day = RelativeDay(time);
			if (day <= latest_relative_day)
				result += item_bias_at_frequency[item_id, log_frequency_by_day[user_id, day]];

			return result;
		}

		///
		public override float ComputeObjective()
		{
			return (float) (base.ComputeObjective() + RegItemBiasAtFrequency * Math.Pow(item_bias_at_frequency.FrobeniusNorm(), 2));
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

