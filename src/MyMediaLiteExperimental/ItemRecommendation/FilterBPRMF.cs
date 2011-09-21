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
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Eval;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Matrix factroization optimized for attribute-filtered BPR</summary>
	public class FilterBPRMF : BPRMF, IItemAttributeAwareRecommender
	{
		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set	{
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private SparseBooleanMatrix item_attributes;

		///
		public int NumItemAttributes { get;	private set; }

		IList<Dictionary<int, ICollection<int>>> filtered_items_by_user;
		SparseBooleanMatrix items_by_attribute;

		///
		public override void Train()
		{
			filtered_items_by_user = new Dictionary<int, ICollection<int>>[MaxUserID + 1];
			items_by_attribute = (SparseBooleanMatrix) item_attributes.Transpose();

			Console.Error.WriteLine("max_user_id {0} max_item_id {1}", MaxUserID, MaxItemID);

			for (int u = 0; u < filtered_items_by_user.Count; u++)
				filtered_items_by_user[u] = ItemsFiltered.GetFilteredItems(u, Feedback, ItemAttributes);

			base.Train();
		}

		/// <summary>Sample an item pair for a given user</summary>
		/// <param name="u">the user ID</param>
		/// <param name="i">the ID of the higher-ranking item</param>
		/// <param name="j">the ID of the lower-ranking item</param>
		protected override void SampleItemPair(int u, out int i, out int j)
		{
			// sample filter attribute accessed by user
			var user_filter_attributes = filtered_items_by_user[u].Keys;
			if (user_filter_attributes.Count == 0)
				throw new ArgumentException("user w/o filter attributes");
			
			int a = user_filter_attributes.ElementAt(random.Next(user_filter_attributes.Count));

			// TODO catch condition that user has rated all comedies ...

			var user_filtered_items = filtered_items_by_user[u][a];
			// sample positive item
			i = user_filtered_items.ElementAt(random.Next(user_filtered_items.Count));
			// sample negative item
			do
				j = items_by_attribute[a].ElementAt(random.Next(items_by_attribute[a].Count));
			while (Feedback.UserMatrix[u, j]);
		}
	}
}
