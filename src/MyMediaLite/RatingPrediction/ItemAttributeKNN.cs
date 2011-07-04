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
using System.Collections.Generic;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Attribute-aware weighted item-based kNN recommender</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class ItemAttributeKNN : ItemKNN, IItemAttributeAwareRecommender
	{
		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private SparseBooleanMatrix item_attributes;

		///
		public int NumItemAttributes { get;	private set; }

		///
		public override void Train()
		{
			base.Train();
			this.correlation = BinaryCosine.Create(ItemAttributes);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}

		///
		public override string ToString()
		{
			return string.Format("ItemAttributeKNN k={0} reg_u={1} reg_i={2}",
								 K == uint.MaxValue ? "inf" : K.ToString(), RegU, RegI);
		}
	}

}
