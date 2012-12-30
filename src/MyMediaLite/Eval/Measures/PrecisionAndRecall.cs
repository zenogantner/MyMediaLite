// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
using System.Linq;

namespace MyMediaLite.Eval.Measures
{
	/// <summary>Precision and recall at different positions in the list</summary>
	/// <remarks>
	///   <para>
	///     Precision and recall are classical evaluation measures from information retrieval.
	///   </para>
	///   <para>
	///     This class contains methods for computing precision and recall up to different positions
	///     in the recommendation list, and the average precision (AP).
	///   </para>
	///   <para>
	///     The mean of the AP over different users is called mean average precision (MAP)
	///   </para>
	/// </remarks>
	public static class PrecisionAndRecall
	{
		/// <summary>Compute the average precision (AP) of a list of ranked items</summary>
		/// <remarks>See p. 147 of Introduction to Information Retrieval by Manning, Raghavan, Schütze.</remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the AP for the given list</returns>
		public static double AP(IList<int> ranked_items, ICollection<int> correct_items)
		{
			int hit_count       = 0;
			double avg_prec_sum = 0;

			for (int i = 0; i < ranked_items.Count; i++)
			{
				int item_id = ranked_items[i];

				if (correct_items.Contains(item_id))
				{
					hit_count++;
					avg_prec_sum += (double) hit_count / (i + 1);
				}
			}

			if (hit_count != 0)
				return avg_prec_sum / correct_items.Count;
			else
				return 0;
		}

		/// <summary>Compute the precision at N of a list of ranked items at several N</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ns">the cutoff positions in the list</param>
		/// <returns>the precision at N for the given data at the different positions N</returns>
		public static Dictionary<int, double> PrecisionAt(
			IList<int> ranked_items,
			ICollection<int> correct_items,
			IList<int> ns)
		{
			var precision_at_n = new Dictionary<int, double>();
			foreach (int n in ns)
				precision_at_n[n] = PrecisionAt(ranked_items, correct_items, n);
			return precision_at_n;
		}

		/// <summary>Compute the precision at N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the precision at N for the given data</returns>
		public static double PrecisionAt(
			IList<int> ranked_items, ICollection<int> correct_items, int n)
		{
			return (double) HitsAt(ranked_items, correct_items, n) / n;
		}

		/// <summary>Compute the recall at N of a list of ranked items at several N</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="ns">the cutoff positions in the list</param>
		/// <returns>the recall at N for the given data at the different positions N</returns>
		public static Dictionary<int, double> RecallAt(
			IList<int> ranked_items, ICollection<int> correct_items, IList<int> ns)
		{
			var recall_at_n = new Dictionary<int, double>();
			foreach (int n in ns)
				recall_at_n[n] = RecallAt(ranked_items, correct_items, n);
			return recall_at_n;
		}

		/// <summary>Compute the recall at N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the recall at N for the given data</returns>
		public static double RecallAt(
			IList<int> ranked_items, ICollection<int> correct_items, int n)
		{
			return (double) HitsAt(ranked_items, correct_items, n) / correct_items.Count;
		}

		/// <summary>Compute the number of hits until position N of a list of ranked items</summary>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <param name="n">the cutoff position in the list</param>
		/// <returns>the hits at N for the given data</returns>
		public static int HitsAt(
			IList<int> ranked_items, ICollection<int> correct_items, int n)
		{
			if (n < 1)
				throw new ArgumentException("n must be at least 1.");

			int hit_count = 0;

			for (int i = 0; i < ranked_items.Count; i++)
			{
				int item_id = ranked_items[i];

				if (!correct_items.Contains(item_id))
					continue;

				if (i < n)
					hit_count++;
				else
					break;
			}

			return hit_count;
		}
	}
}
