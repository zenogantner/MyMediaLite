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
using System.IO;
using MyMediaLite.correlation;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// k-nearest neighbor user-based collaborative filtering using cosine-similarity
    /// </summary>
    /// <remarks>
    /// k=inf equals most-popular.
    ///
    /// This engine does not support online updates.
	/// </remarks>
    public class UserKNN : KNN
    {
		/// <summary>
		/// Precomputed nearest neighbors
		/// </summary>
		protected int[][] nearest_neighbors;

        /// <inheritdoc/>
        public override void Train()
        {
			correlation = Cosine.Create(data_user);

			int num_users = MaxUserID + 1;
			nearest_neighbors = new int[num_users][];
			for (int u = 0; u < num_users; u++)
				nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
        }

        /// <inheritdoc/>
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

		/// <inheritdoc/>
		public override void SaveModel(string filePath)
		{
			using ( StreamWriter writer = Engine.GetWriter(filePath, this.GetType()) )
			{
				writer.WriteLine(nearest_neighbors.Length);
				foreach (int[] nn in nearest_neighbors)
				{
					writer.Write(nn[0]);
					for (int i = 1; i < nn.Length; i++)
					 	writer.Write(" {0}", nn[i]);
					writer.WriteLine();
				}

				correlation.Write(writer);
			}
		}

		/// <inheritdoc/>
		public override void LoadModel(string filePath)
		{
			using ( StreamReader reader = Engine.GetReader(filePath, this.GetType()) )
			{
				int num_users = int.Parse(reader.ReadLine());
				int[][] nearest_neighbors = new int[num_users][];
				for (int u = 0; u < nearest_neighbors.Length; u++)
				{
					string[] numbers = reader.ReadLine().Split(' ');

					nearest_neighbors[u] = new int[numbers.Length];
					for (int i = 0; i < numbers.Length; i++)
						nearest_neighbors[u][i] = int.Parse(numbers[i]);
				}

				this.correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);
				this.k = (uint) nearest_neighbors[0].Length; // TODO add warning if we have a different k
				this.nearest_neighbors = nearest_neighbors;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("user-kNN k={0}",
			                     k == uint.MaxValue ? "inf" : k.ToString());
		}
    }
}