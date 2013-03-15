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
using System.Collections.Generic;

namespace MyMediaLite.Eval.Measures
{
	/// <summary>The reciprocal rank of a list of ranked items</summary>
	/// <remarks>
	///   <para>
	///     See http://en.wikipedia.org/wiki/Mean_reciprocal_rank
	///   </para>
	///
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         E.M. Voorhees "Proceedings of the 8th Text Retrieval Conference". TREC-8 Question Answering Track Report. 1999.
	///         http://gate.ac.uk/sale/dd/related-work/qa/TREC+1999+TREC-8+QA+Report.pdf
	///       </description></item>
	///     </list>
	///   </para>
	/// </remarks>
	public static class ReciprocalRank
	{
		/// <summary>Compute the reciprocal rank of a list of ranked items</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Mean_reciprocal_rank
		/// </remarks>
		/// <param name="ranked_items">a list of ranked item IDs, the highest-ranking item first</param>
		/// <param name="correct_items">a collection of positive/correct item IDs</param>
		/// <returns>the mean reciprocal rank for the given data</returns>
		public static double Compute(IList<int> ranked_items, ICollection<int> correct_items)
		{
			int pos = 0;

			foreach (int item_id in ranked_items)
			{
				if (correct_items.Contains(ranked_items[pos++]))
					return (double) 1 / (pos);
			}

			return 0;
		}
	}
}
