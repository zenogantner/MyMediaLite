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

using System;
using System.Collections.Generic;

namespace MyMediaLite.Eval.Measures
{
	/// <summary>Normalized discounted cumulative gain (NDCG) of a list of ranked items</summary>
	/// <remarks>
	/// See http://recsyswiki.com/wiki/Discounted_Cumulative_Gain
	/// </remarks>
	public static class NDCG
	{
		/// <summary>Compute the normalized discounted cumulative gain (NDCG) of a list of ranked items</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Discounted_Cumulative_Gain
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ignore_items">a collection of item IDs which should be ignored for the evaluation</param>
		/// <returns>the NDCG for the given data</returns>
		public static double Compute(IList<int> ranked_items, ICollection<int> correct_items, ICollection<int> ignore_items = null)
		{
			if (ignore_items == null)
				ignore_items = new HashSet<int>();

			double dcg   = 0;
			double idcg  = ComputeIDCG(correct_items.Count);
			int left_out = 0;

			for (int i = 0; i < ranked_items.Count; i++)
			{
				int item_id = ranked_items[i];
				if (ignore_items.Contains(item_id))
				{
					left_out++;
					continue;
				}

				if (!correct_items.Contains(item_id))
					continue;

				// compute NDCG part
				int rank = i + 1 - left_out;
				dcg += 1 / Math.Log(rank + 1, 2);
			}

			return dcg / idcg;
		}

		/// <summary>Computes the ideal DCG given the number of positive items.</summary>
		/// <remarks>
		/// See http://recsyswiki.com/wiki/Discounted_Cumulative_Gain
		/// </remarks>
		/// <returns>the ideal DCG</returns>
		/// <param name='n'>the number of positive items</param>
		static double ComputeIDCG(int n)
		{
			double idcg = 0;
			for (int i = 0; i < n; i++)
				idcg += 1 / Math.Log(i + 2, 2);
			return idcg;
		}
	}
}
