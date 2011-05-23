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

using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Util;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Base class for item recommenders that use some kind of kNN model</summary>
	public abstract class KNN : ItemRecommender
	{
		/// <summary>The number of neighbors to take into account for prediction</summary>
		public uint K {	get { return k;	} set {	k = value; } }
		
		/// <summary>The number of neighbors to take into account for prediction</summary>
		protected uint k = 80;

		/// <summary>Precomputed nearest neighbors</summary>
		protected int[][] nearest_neighbors;

		/// <summary>Correlation matrix over some kind of entity</summary>
		protected CorrelationMatrix correlation;

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
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

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
			{
				int num_users = int.Parse(reader.ReadLine());
				var nearest_neighbors = new int[num_users][];
				for (int u = 0; u < nearest_neighbors.Length; u++)
				{
					string[] numbers = reader.ReadLine().Split(' ');

					nearest_neighbors[u] = new int[numbers.Length];
					for (int i = 0; i < numbers.Length; i++)
						nearest_neighbors[u][i] = int.Parse(numbers[i]);
				}

				this.correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);
				this.k = (uint) nearest_neighbors[0].Length;
				this.nearest_neighbors = nearest_neighbors;
			}
		}
	}
}