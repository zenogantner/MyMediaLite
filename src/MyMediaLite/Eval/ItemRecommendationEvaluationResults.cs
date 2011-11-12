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

namespace MyMediaLite.Eval
{
	/// <summary>Item recommendation evaluation results</summary>
	/// <remarks>
	/// This class is basically a Dictionary with a custom-made ToString() method.
	/// </remarks>
	public class ItemRecommendationEvaluationResults : Dictionary<string, double>
	{
		/// <summary>default constructor</summary>
		public ItemRecommendationEvaluationResults()
		{
			foreach (string method in Items.Measures)
				this[method] = 0;
		}

		/// <summary>Format item prediction results</summary>
		/// <returns>a string containing the results</returns>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture, "AUC {0:0.#####} prec@5 {1:0.#####} prec@10 {2:0.#####} MAP {3:0.#####} recall@5 {4:0.#####} recall@10 {5:0.#####} NDCG {6:0.#####} MRR {7:0.#####} num_users {8} num_items {9} num_lists {10}",
				this["AUC"], this["prec@5"], this["prec@10"], this["MAP"], this["recall@5"], this["recall@10"], this["NDCG"], this["MRR"], this["num_users"], this["num_items"], this["num_lists"]
			);
		}
	}
}

