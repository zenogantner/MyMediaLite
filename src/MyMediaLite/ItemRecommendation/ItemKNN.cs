// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.Correlation;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Unweighted k-nearest neighbor item-based collaborative filtering using cosine similarity</summary>
	/// <remarks>
	/// This engine does not support online updates.
	/// </remarks>
	public class ItemKNN : KNN
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
		public override double Predict(int user_id, int item_id)
		{
			if ((user_id < 0) || (user_id > MaxUserID))
				throw new ArgumentException("user is unknown: " + user_id);
			if ((item_id < 0) || (item_id > MaxItemID))
				throw new ArgumentException("item is unknown: " + item_id);

			int count = 0;
			foreach (int neighbor in nearest_neighbors[item_id])
				if (Feedback.UserMatrix[neighbor, user_id])
					count++;

			return (double) count / k;
		}

		///
		public override string ToString()
		{
			return string.Format("ItemKNN k={0}" , k == uint.MaxValue ? "inf" : k.ToString());
		}
	}
}