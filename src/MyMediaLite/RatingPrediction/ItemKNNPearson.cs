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
using MyMediaLite.Taxonomy;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted item-based kNN with pearson correlation</summary>
	/// <remarks>This engine supports online updates.</remarks>
	public class ItemKNNPearson : ItemKNN
	{
		/// <summary>Shrinkage parameter</summary>
		public float Shrinkage { get { return shrinkage; } set { shrinkage = value; } }
		private float shrinkage = 10;

		/// <inheritdoc/>
		public ItemKNNPearson() : base() { }

		/// <inheritdoc/>
		public override void Train()
		{
			base.Train();
			this.correlation = Pearson.Create(ratings, EntityType.ITEM, Shrinkage);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}

		/// <inheritdoc/>
		protected override void RetrainItem(int item_id)
		{
			base.RetrainUser(item_id);
			if (UpdateItems)
				for (int i = 0; i <= MaxItemID; i++)
					correlation[item_id, i] = Pearson.ComputeCorrelation(ratings, EntityType.ITEM, item_id, i, Shrinkage);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("ItemKNNPearson k={0} shrinkage={1} reg_u={2} reg_i={3}",
								 K == uint.MaxValue ? "inf" : K.ToString(), Shrinkage, RegU, RegI);
		}
	}
}