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
using System.Collections.Generic;
using MyMediaLite.Correlation;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted item-based kNN with cosine similarity</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// </remarks>
	public class ItemKNNCosine : ItemKNN
	{
		IBinaryDataCorrelationMatrix BinaryDataCorrelation { get { return correlation as IBinaryDataCorrelationMatrix; } }

		///
		public override void Train()
		{
			baseline_predictor.Train();
			this.correlation = new BinaryCosine(data_item.NumberOfRows);
			((IBinaryDataCorrelationMatrix)correlation).ComputeCorrelations(data_item);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}

		///
		protected override void RetrainItem(int item_id)
		{
			baseline_predictor.RetrainItem(item_id);
			if (UpdateItems)
			{
				var item_users = new HashSet<int>(data_item[item_id]);
				for (int i = 0; i <= MaxItemID; i++)
					correlation[item_id, i] = BinaryDataCorrelation.ComputeCorrelation(item_users, new HashSet<int>(data_item[i]));
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} k={1} reg_u={2} reg_i={3} num_iter={4}",
				this.GetType().Name, K == uint.MaxValue ? "inf" : K.ToString(), RegU, RegI, NumIter);
		}
	}
}