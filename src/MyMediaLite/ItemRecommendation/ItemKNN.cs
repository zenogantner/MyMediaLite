// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.Correlation;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Unweighted k-nearest neighbor item-based collaborative filtering using cosine similarity</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class ItemKNN : KNN, IItemSimilarityProvider
	{
		///
		public override void Train()
		{
			correlation = BinaryCosine.Create(Feedback.ItemMatrix);

			int num_items = MaxItemID + 1;
			this.nearest_neighbors = new int[num_items][];
			for (int i = 0; i < num_items; i++)
				nearest_neighbors[i] = correlation.GetNearestNeighbors(i, k);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if ((user_id < 0) || (user_id > MaxUserID))
				return float.MinValue;
			if ((item_id < 0) || (item_id > MaxItemID))
				return float.MinValue;

			int count = 0;
			foreach (int neighbor in nearest_neighbors[item_id])
				if (Feedback.ItemMatrix[neighbor, user_id])
					count++;

			return (float) (count / k);
		}

		///
		public float GetItemSimilarity(int item_id1, int item_id2)
		{
			return correlation[item_id1, item_id2];
		}

		///
		public IList<int> GetMostSimilarItems(int item_id, uint n = 10)
		{
			if (n == k)
				return nearest_neighbors[item_id];
			else if (n < k)
				return nearest_neighbors[item_id].Take((int) n).ToArray();
			else
				return correlation.GetNearestNeighbors(item_id, n);
		}

		///
		public override string ToString()
		{
			return string.Format("ItemKNN k={0}", k == uint.MaxValue ? "inf" : k.ToString());
		}
	}
}