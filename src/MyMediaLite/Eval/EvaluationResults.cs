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
using System.Linq;

namespace MyMediaLite.Eval
{
	/// <summary>
	/// Class for representing evaluation results
	/// </summary>
	[Serializable]
	public abstract class EvaluationResults : Dictionary<string, float>
	{
		/// <summary>
		/// List of strings representing the evaluation measures which will be shown by the ToString() method
		/// </summary>
		/// <remarks>
		/// All strings must be keys of the dictionary.
		/// </remarks>
		public IList<string> MeasuresToShow { get; set; }

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
		public string FloatingPointFormat { get; set; }

		/// <summary> Constructor</summary>
		protected EvaluationResults()
		{
			FloatingPointFormat = "0.#####";
		}

		/// <summary>Create averaged results</summary>
		/// <param name='result_list'>the list of results to average</param>
		protected EvaluationResults(IList<Dictionary<string, float>> result_list)
		{
			foreach (var key in result_list[0].Keys)
			{
				this[key] = 0;
				foreach (var r in result_list)
					this[key] += r[key];
				this[key] /= result_list.Count;
			}
		}

		/// <summary>initialize with given results</summary>
		/// <param name='results'>a dictionary containing results</param>
		protected EvaluationResults(Dictionary<string, float> results)
		{
			foreach (var key in results.Keys)
				this[key] = results[key];
		}

		/// <summary>Format item prediction results</summary>
		/// <returns>a string containing the results</returns>
		public override string ToString()
		{
			var metrics = (from m in MeasuresToShow select string.Format("{0} {1:" + FloatingPointFormat + "}", m, this[m])).ToList();
			var ints    = (from i in IntsToShow    select string.Format("{0} {1}", i, this[i])).ToList();

			string s = string.Join(" ", metrics.Concat(ints));

			if (this.ContainsKey("fit"))
				s += string.Format(CultureInfo.InvariantCulture, " fit {0:0.#####}", this["fit"]);
			return s;
		}
	}
}

