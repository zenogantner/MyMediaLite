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

namespace MyMediaLite.Eval.Measures
{
		/// <summary>The reciprocal rank of a list of ranked items</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Mean_reciprocal_rank
		/// </remarks>
	public static class ReciprocalRank
	{
		/// <summary>Compute the reciprocal rank of a list of ranked items</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Mean_reciprocal_rank
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the mean reciprocal rank for the given data</returns>
		public static double Compute(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items = null)
		{
			if (ignore_items == null)
				ignore_items = new HashSet<int>();

			int pos = 0;

			foreach (int item_id in ranked_items)
			{
				if (ignore_items.Contains(item_id))
					continue;

				if (correct_items.Contains(ranked_items[pos++]))
					return (double) 1 / (pos);
			}

			return 0;
		}
	}
}
