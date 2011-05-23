// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>
	/// k-nearest neighbor item-based collaborative filtering using cosine-similarity over the item attibutes
	/// </summary>
	/// <remarks>
	/// This engine does not support online updates.
	/// </remarks>
	public class ItemAttributeKNN : ItemKNN, IItemAttributeAwareRecommender
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
		public int NumItemAttributes { get;	set; }

		///
		public override void Train()
		{
			this.correlation = BinaryCosine.Create(ItemAttributes);

			int num_items = MaxItemID + 1;
			this.nearest_neighbors = new int[num_items][];
			for (int i = 0; i < num_items; i++)
				nearest_neighbors[i] = correlation.GetNearestNeighbors(i, k);
		}

		///
		public override string ToString()
		{
			return string.Format("ItemAttributeKNN k={0}", k == uint.MaxValue ? "inf" : k.ToString());
		}
	}
}