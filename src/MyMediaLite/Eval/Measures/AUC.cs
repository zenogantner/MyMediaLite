// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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

using System.Collections.Generic;
using System.Linq;

/*! \namespace MyMediaLite.Eval.Measures
*   \brief This namespace contains different evaluation measures.
*/
namespace MyMediaLite.Eval.Measures
{
	/// <summary>Area under the ROC curve (AUC) of a list of ranked items</summary>
	/// <remarks>
	/// See http://recsyswiki.com/wiki/Area_Under_the_ROC_Curve
	/// </remarks>
	public static class AUC
	{
		/// <summary>Compute the area under the ROC curve (AUC) of a list of ranked items</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Area_Under_the_ROC_Curve
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the AUC for the given data</returns>
		public static double Compute(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items = null)
		{
			if (ignore_items == null)
				ignore_items = new HashSet<int>();

			int num_eval_items = ranked_items.Count - ignore_items.Intersect(ranked_items).Count();
			int num_eval_pairs = (num_eval_items - correct_items.Count) * correct_items.Count;

			int num_correct_pairs = 0;
			int hit_count         = 0;

			foreach (int item_id in ranked_items)
			{
				if (ignore_items.Contains(item_id))
					continue;

				if (!correct_items.Contains(item_id))
					num_correct_pairs += hit_count;
				else
					hit_count++;
			}

			return ((double) num_correct_pairs) / num_eval_pairs;
		}
	}
}
