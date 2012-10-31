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
using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>k-nearest neighbor user-based collaborative filtering</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class UserKNN : KNN, IUserSimilarityProvider
	{
		///
		protected override IBooleanMatrix DataMatrix { get { return Feedback.UserMatrix; } }

		///
		public override void Train()
		{
			base.Train();

			int num_users = MaxUserID + 1;
			this.nearest_neighbors = new int[num_users][];
			for (int u = 0; u < num_users; u++)
				nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if ((user_id > MaxUserID) || (item_id > MaxItemID))
				return float.MinValue;

			if (k != uint.MaxValue)
			{
				double sum = 0;
				foreach (int neighbor in nearest_neighbors[user_id])
					if (Feedback.UserMatrix[neighbor, item_id])
						sum += Math.Pow(correlation[user_id, neighbor], Q);
				return (float) sum;
			}
			else
			{
				return (float) correlation.SumUp(item_id, Feedback.UserMatrix[user_id], Q);;
			}
		}

		///
		public float GetUserSimilarity(int user_id1, int user_id2)
		{
			return correlation[user_id1, user_id2];
		}

		///
		public IList<int> GetMostSimilarUsers(int user_id, uint n = 10)
		{
			if (n <= k)
				return nearest_neighbors[user_id].Take((int) n).ToArray();
			else
				return correlation.GetNearestNeighbors(user_id, n);
		}
	}
}