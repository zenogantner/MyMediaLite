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

using System.Collections.Generic;
using MyMediaLite.Correlation;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted item-based kNN with cosine similarity</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// </remarks>
	public class ItemKNNCosine : ItemKNN
	{
		///
		public ItemKNNCosine() : base() { }

		///
		public override void Train()
		{
			base.Train();
			this.correlation = BinaryCosine.Create(data_item);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}

		///
		protected override void RetrainItem(int item_id)
		{
			base.RetrainUser(item_id);
			if (UpdateItems)
				for (int i = 0; i <= MaxItemID; i++)
					correlation[item_id, i] = BinaryCosine.ComputeCorrelation(new HashSet<int>(data_item[item_id]), new HashSet<int>(data_item[i]));
		}

		///
		public override string ToString()
		{
			return string.Format("ItemKNNCosine k={0} reg_u={1} reg_i={2}",
								 K == uint.MaxValue ? "inf" : K.ToString(), RegU, RegI);
		}
	}
}