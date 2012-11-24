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
using System.Linq;

namespace MyMediaLite.Eval
{
	/// <summary>Item recommendation evaluation results</summary>
	/// <remarks>
	/// This class is basically a Dictionary with a custom-made ToString() method.
	/// </remarks>
	[Serializable]
	public class ItemRecommendationEvaluationResults : Dictionary<string, float>
	{
		/// <summary>
		/// List of strings representing the metrics which will be shown by the ToString() method
		/// </summary>
		/// <remarks>
		/// All strings must be keys of the dictionary.
		/// </remarks>
		public IList<string> MetricsToShow { get; set; }

		/// <summary>
		/// List of strings representing the integer values (like number of users) which will be shown by the ToString() method
		/// </summary>
		/// <remarks>
		/// All strings must be keys of the dictionary.
		/// </remarks>
		public IList<string> IntsToShow { get; set; }

		/// <summary>
		/// The format string used to display floating point numbers
		/// </summary>
		public string MetricFormatString { get; set; }

		/// <summary>default constructor</summary>
		public ItemRecommendationEvaluationResults()
		{
			MetricsToShow = new string[] { "AUC", "prec@5", "prec@10", "MAP", "recall@5", "recall@10", "NDCG", "MRR" };
			IntsToShow    = new string[] { "num_users", "num_items", "num_lists" };
			MetricFormatString = "0.#####";

			foreach (string method in Items.Measures)
				this[method] = 0;
		}

		/// <summary>Create averaged results</summary>
		/// <param name='result_list'>the list of results to average</param>
		public ItemRecommendationEvaluationResults(IList<Dictionary<string, float>> result_list)
		{
			foreach (var key in result_list[0].Keys)
			{
				this[key] = 0;
				foreach (var r in result_list)
					this[key] += r[key];
				this[key] /= result_list.Count;
			}
		}

		/// <summary>Format item prediction results</summary>
		/// <returns>a string containing the results</returns>
		public override string ToString()
		{
			var metrics = (from m in MetricsToShow select string.Format("{0} {1:" + MetricFormatString + "}", m, this[m])).ToList();
			var ints    = (from i in IntsToShow    select string.Format("{0} {1}", i, this[i])).ToList();

			string s = string.Join(" ", metrics.Concat(ints));

			if (this.ContainsKey("fit"))
				s += string.Format(CultureInfo.InvariantCulture, " fit {0:0.#####}", this["fit"]);
			return s;
		}
	}
}

