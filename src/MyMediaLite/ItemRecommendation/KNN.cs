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
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Base class for item recommenders that use some kind of k-nearest neighbors (kNN) model</summary>
	/// <seealso cref="MyMediaLite.ItemRecommendation.KNN"/>
	public abstract class KNN : ItemRecommender
	{
		/// <summary>The number of neighbors to take into account for prediction</summary>
		public uint K { get { return k; } set { k = value; } }

		public float Alpha { get; set; }

		public BinaryCorrelationType Correlation { get; set; }

		/// <summary>The number of neighbors to take into account for prediction</summary>
		protected uint k = 80;

		/// <summary>Precomputed nearest neighbors</summary>
		protected IList<IList<int>> nearest_neighbors;

		/// <summary>Correlation matrix over some kind of entity</summary>
		protected ICorrelationMatrix correlation;

		protected abstract IBooleanMatrix DataMatrix { get; }

		public KNN()
		{
			Correlation = BinaryCorrelationType.Cosine;
			Alpha = 0.5;
		}

		public override void Train()
		{
			switch (Correlation)
			{
				case BinaryCorrelationType.Cosine:
					correlation = BinaryCosine.Create(DataMatrix);
					break;
				case BinaryCorrelationType.Jaccard:
					correlation = Jaccard.Create(DataMatrix);
					break;
				case BinaryCorrelationType.ConditionalProbability:
					correlation = ConditionalProbability.Create(DataMatrix);
					break;
				case BinaryCorrelationType.BidirectionalConditionalProbability:
					correlation = BidirectionalConditionalProbability.Create(DataMatrix, Alpha);
					break;
				case BinaryCorrelationType.WeightedCosine:
					correlation = WeightedBinaryCosine.Create(DataMatrix);
					break;
				case BinaryCorrelationType.Cooccurrence:
					correlation = Cooccurrence.Create(DataMatrix);
					break;
				default:
					throw new NotImplementedException(string.Format("Support for {0} is not implemented", Correlation));
			}
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.03") )
			{
				writer.WriteLine(nearest_neighbors.Count);
				foreach (IList<int> nn in nearest_neighbors)
				{
					writer.Write(nn[0]);
					for (int i = 1; i < nn.Count; i++)
					 	writer.Write(" {0}", nn[i]);
					writer.WriteLine();
				}

				correlation.Write(writer);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
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

				this.correlation = SymmetricCorrelationMatrix.ReadCorrelationMatrix(reader);
				this.k = (uint) nearest_neighbors[0].Length;
				this.nearest_neighbors = nearest_neighbors;
			}
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} k={1} correlation={2} alpha={3} (only for BidirectionalConditionalProbability)",
				this.GetType().Name, k == uint.MaxValue ? "inf" : k.ToString(), Correlation, Alpha);
		}
	}
}