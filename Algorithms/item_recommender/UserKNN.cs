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

using System;
using MyMediaLite.correlation;


namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// k-nearest neighbor user-based collaborative filtering using cosine-similarity
    /// k=inf equals most-popular.
    ///
    /// This engine does not support online updates.
    /// </summary>
    public class UserKNN : KNN
    {
		/// <summary>
		/// Precomputed nearest neighbors
		/// </summary>
		protected int[][] nearest_neighbors;

        /// <inheritdoc />
        public override void Train()
        {
			correlation = Cosine.Create(data_user);

			int num_users = MaxUserID + 1;
			nearest_neighbors = new int[num_users][];
			for (int u = 0; u < num_users; u++)
				nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
        }

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id > MaxUserID))
                throw new ArgumentException("User is unknown: " + user_id);
            if ((item_id < 0) || (item_id > MaxItemID))
                throw new ArgumentException("Item is unknown: " + item_id);

			int count = 0;
			foreach (int neighbor in nearest_neighbors[user_id])
			{
				if (data_user[neighbor, item_id])
					count++;
			}
			return (double) count / k;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("user-kNN, k={0}",
			                     k == uint.MaxValue ? "inf" : k.ToString());
		}
    }
}