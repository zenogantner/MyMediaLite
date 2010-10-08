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
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	/// <summary>Item-based kNN with cosine similarity</summary>
	public class ItemKNNCosine : ItemKNN
	{
        /// <inheritdoc />
        public override void Train()
        {
			base.Train();

			correlation.Cosine cosine_correlation = new Cosine(MaxItemID + 1);
			cosine_correlation.ComputeCorrelations(data_item);
			this.correlation = cosine_correlation;

			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
        }

		/// <inheritdoc />
		protected override void RetrainItem(int item_id)
		{
			base.RetrainUser(item_id);
			if (UpdateItems)
				for (int i = 0; i < MaxItemID; i++)
				{
					float cor = Cosine.ComputeCorrelation(data_item[item_id], data_item[i]);
					correlation.data.Set(item_id, i, cor);
					correlation.data.Set(i, item_id, cor);
				}
		}

        /// <inheritdoc />
		public override string ToString()
		{
			return string.Format("item-kNN-cosine k={0}, reg_u={1}, reg_i={2}",
			                     k == uint.MaxValue ? "inf" : k.ToString(), reg_u, reg_i);
		}
	}
}