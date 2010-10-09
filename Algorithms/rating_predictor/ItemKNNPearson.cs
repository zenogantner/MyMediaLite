// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.correlation;
using MyMediaLite.taxonomy;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	/// <summary>Item-based kNN with pearson correlation</summary>
	public class ItemKNNPearson : ItemKNN
	{
        /// <inheritdoc />
        public override void Train()
        {
			base.Train();

			correlation.Pearson pearson_correlation = new Pearson(MaxItemID + 1);
			pearson_correlation.shrinkage = (float) this.shrinkage;
			pearson_correlation.ComputeCorrelations(ratings, EntityType.ITEM);
			this.correlation = pearson_correlation;

			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
        }

		/// <inheritdoc />
		protected override void RetrainItem(int item_id)
		{
			base.RetrainUser(item_id);
			if (UpdateItems)
				for (int i = 0; i <= MaxItemID; i++)
				{
					float cor = Pearson.ComputeCorrelation(ratings.ByItem[item_id], ratings.ByItem[i], EntityType.ITEM, item_id, i, (float) shrinkage);
					correlation[item_id, i] = cor;
					correlation[i, item_id] = cor;
				}
		}

        /// <inheritdoc />
		public override string ToString()
		{
			return string.Format("item-kNN-pearson k={0} shrinkage={1} reg_u={2} reg_i={3}",
			                     k == uint.MaxValue ? "inf" : k.ToString(), shrinkage, reg_u, reg_i);
		}
	}
}