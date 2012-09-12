// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using System.Diagnostics;
using System.Linq;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;

/*! \namespace MyMediaLite.Diversification
 *  \brief This namespace contains methods for diversifying result lists.
 */
namespace MyMediaLite.Diversification
{
	/// <summary>Sequential diversification</summary>
	/// <remarks>
	/// Literature:
	/// <list type="bullet">
	///   <item><description>
	///   Cai-Nicolas Ziegler, Sean McNee, Joseph A. Konstan, Georg Lausen:
	///   Improving Recommendation Lists Through Topic Diversification.
	///   WWW 2005
	///   </description></item>
	/// </list>
	/// </remarks>
	public class SequentialDiversification
	{
		SymmetricCorrelationMatrix ItemCorrelations { get; set; }

		/// <summary>Constructor</summary>
		/// <param name="item_correlation">the similarity measure to use for diversification</param>
		public SequentialDiversification(SymmetricCorrelationMatrix item_correlation)
		{
			ItemCorrelations = item_correlation;
		}

		/// <summary>Diversify an item list</summary>
		/// <param name="item_list">a list of items</param>
		/// <param name="diversification_parameter">the diversification parameter (higher means more diverse)</param>
		/// <returns>a list re-ordered to ensure maximum diversity at the top of the list</returns>
		public IList<int> DiversifySequential(IList<int> item_list, float diversification_parameter)
		{
			Trace.Assert(item_list.Count > 0);

			var item_rank_by_rating = new Dictionary<int, int>();
			for (int i = 0; i < item_list.Count; i++)
				item_rank_by_rating[item_list[i]] = i;

			var diversified_item_list = new List<int>();
			int top_item = item_list[0];
			diversified_item_list.Add(top_item);

			var item_set = new HashSet<int>(item_list);
			item_set.Remove(top_item);
			while (item_set.Count > 0)
			{
				// rank remaining items by diversity
				var items_by_diversity = new List<Tuple<int, float>>();
				foreach (int item_id in item_set)
				{
					float similarity = Similarity(item_id, diversified_item_list, ItemCorrelations);
					items_by_diversity.Add(Tuple.Create(item_id, similarity));
				}
				items_by_diversity = items_by_diversity.OrderBy(x => x.Item2).ToList();

				var items_by_merged_rank = new List<Tuple<int, float>>();
				for (int i = 0; i < items_by_diversity.Count; i++)
				{
					int item_id = items_by_diversity[i].Item1;
					// i is the dissimilarity rank
					// TODO adjust for ties
					float score = item_rank_by_rating[item_id] * (1f - diversification_parameter) + i * diversification_parameter;

					items_by_merged_rank.Add(Tuple.Create(item_id, score));
				}
				items_by_merged_rank = items_by_merged_rank.OrderBy(x => x.Item2).ToList();

				int next_item_id = items_by_merged_rank[0].Item1;
				diversified_item_list.Add(next_item_id);
				item_set.Remove(next_item_id);
			}
			return diversified_item_list;
		}

		/// <summary>Compute similarity between one item and a collection of items</summary>
		/// <param name="item_id">the item ID</param>
		/// <param name="items">a collection of items</param>
		/// <param name="item_correlation">the similarity measure to use</param>
		/// <returns>the similarity between the item and the collection</returns>
		public static float Similarity(int item_id, ICollection<int> items, SymmetricCorrelationMatrix item_correlation)
		{
			double similarity = 0;
			foreach (int other_item_id in items)
				similarity += item_correlation[item_id, other_item_id];
			return (float) similarity;
		}

		/// <summary>Compute the intra-set similarity of an item collection</summary>
		/// <param name="items">a collection of items</param>
		/// <param name="item_correlation">the similarity measure to use</param>
		/// <returns>the intra-set similarity of the collection</returns>
		public static float Similarity(ICollection<int> items, SymmetricCorrelationMatrix item_correlation)
		{
			double similarity = 0;
			for (int i = 0; i < items.Count; i++)
				for (int j = i + 1; j < items.Count; j++)
					similarity += item_correlation[i, j];

			return (float) similarity;
		}
	}
}