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

namespace MyMediaLite.Eval
{
	/// <summary>Item recommendation evaluation results</summary>
	/// <remarks>
	/// This class is basically a Dictionary with a custom-made ToString() method.
	/// </remarks>
	[Serializable]
	public class ItemRecommendationEvaluationResults : EvaluationResults
	{
		/// <summary>
		/// Default for MeasuresToShow
		/// </summary>
		static public IList<string> DefaultMeasuresToShow
		{
			get { return new string[] { "AUC", "prec@5" }; }
		}

		/// <summary>default constructor</summary>
		public ItemRecommendationEvaluationResults()
		{
			Init();
		}

		///
		public ItemRecommendationEvaluationResults(IList<Dictionary<string, float>> result_list) : base(result_list)
		{
			Init();
		}

		private void Init()
		{
			MeasuresToShow = DefaultMeasuresToShow;
			IntsToShow = new string[] { "num_items", "num_lists" };
			foreach (string method in Items.Measures)
				this[method] = 0;
		}
	}
}

