// Copyright (C) 2013 João Vinagre, Zeno Gantner
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
	public abstract class KNN : IncrementalItemRecommender
	{
		/// <summary>The number of neighbors to take into account for prediction</summary>
		public uint K { get { return k; } set { k = value; } }

		/// <summary>Alpha parameter for BidirectionalConditionalProbability</summary>
		public float Alpha { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="MyMediaLite.ItemRecommendation.KNN"/> is weighted.
		/// </summary>
		/// <remarks>
		/// TODO add literature reference
		/// </remarks>
		public bool Weighted { get; set; }

		/// <summary>Exponent to be used for transforming the neighbor's weights</summary>
		/// <remarks>
		///   <para>
		///     A value of 0 leads to counting of the relevant neighbors.
		///     1 is the usual weighted prediction.
		///     Values greater than 1 give higher weight to higher correlated neighbors.
		///   </para>
		///   <para>
		///     TODO LIT
		///   </para>
		/// </remarks>
		public float Q { get; set; }

		/// <summary>The kind of correlation to use</summary>
		public BinaryCorrelationType Correlation { get; set; }

		/// <summary>data matrix to learn the correlation from</summary>
		protected abstract IBooleanMatrix DataMatrix { get; }

		/// <summary>The number of neighbors to take into account for prediction</summary>
		protected uint k = 80;

		/// <summary>Precomputed nearest neighbors</summary>
		protected IList<IList<int>> nearest_neighbors;

		/// <summary>Correlation matrix over some kind of entity, e.g. users or items</summary>
		protected IBinaryDataCorrelationMatrix correlation;

		/// <summary>Default constructor</summary>
		public KNN()
		{
			Correlation = BinaryCorrelationType.Cosine;
			Alpha = 0.5f;
			Q = 1.0f;
			UpdateUsers = true;
			UpdateItems = true;
		}

		void InitModel()
		{
			int num_entities = 0;
			switch (Correlation)
			{
				case BinaryCorrelationType.Cosine:
					correlation = new BinaryCosine(num_entities);
					break;
				case BinaryCorrelationType.Jaccard:
					correlation = new Jaccard(num_entities);
					break;
				case BinaryCorrelationType.ConditionalProbability:
					correlation = new ConditionalProbability(num_entities);
					break;
				case BinaryCorrelationType.BidirectionalConditionalProbability:
					correlation = new BidirectionalConditionalProbability(num_entities, Alpha);
					break;
				case BinaryCorrelationType.Cooccurrence:
					correlation = new Cooccurrence(num_entities);
					break;
				default:
					throw new NotImplementedException(string.Format("Support for {0} is not implemented", Correlation));
			}
		}

		/// <summary>Update the correlation matrix for the given feedback</summary>
		/// <param name='feedback'>the feedback (user-item tuples)</param>
		protected void Update(ICollection<Tuple<int, int>> feedback)
		{
			var update_entities = new HashSet<int>();
			foreach (var t in feedback)
				update_entities.Add(t.Item1);

			foreach (int i in update_entities)
			{
				for (int j = 0; j < correlation.NumEntities; j++)
				{
					if (j < i && correlation.IsSymmetric && update_entities.Contains(j))
						continue;

					correlation[i, j] = correlation.ComputeCorrelation(DataMatrix.GetEntriesByRow(i), DataMatrix.GetEntriesByRow(j));
				}
			}
			RecomputeNeighbors(update_entities);
		}

		private void RecomputeNeighbors(ICollection<int> update_entities)
		{
			foreach (int entity_id in update_entities)
				nearest_neighbors[entity_id] = correlation.GetNearestNeighbors(entity_id, k);
		}

		///
		public override void Train()
		{
			InitModel();
			correlation.ComputeCorrelations(DataMatrix);
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "3.03") )
			{
				writer.WriteLine(Correlation);
				writer.WriteLine(nearest_neighbors.Count);
				foreach (IList<int> nn in nearest_neighbors)
					writer.WriteLine(String.Join(" ", nn));

				correlation.Write(writer);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				Correlation = (BinaryCorrelationType) Enum.Parse(typeof(BinaryCorrelationType), reader.ReadLine());

				int num_entities = int.Parse(reader.ReadLine());
				var nearest_neighbors = new int[num_entities][];
				for (int i = 0; i < nearest_neighbors.Length; i++)
				{
					string[] numbers = reader.ReadLine().Split(' ');

					nearest_neighbors[i] = new int[numbers.Length];
					for (int j = 0; j < numbers.Length; j++)
						nearest_neighbors[i][j] = int.Parse(numbers[j]);
				}

				InitModel();
				if (correlation is SymmetricCorrelationMatrix)
					((SymmetricCorrelationMatrix) correlation).ReadSymmetricCorrelationMatrix(reader);
				else if (correlation is AsymmetricCorrelationMatrix)
					((AsymmetricCorrelationMatrix) correlation).ReadAsymmetricCorrelationMatrix(reader);
				else
					throw new NotSupportedException("Unknown correlation type: " + correlation.GetType());

				this.k = (uint) nearest_neighbors[0].Length;
				this.nearest_neighbors = nearest_neighbors;
			}
		}

		/// <summary>Resizes the nearest neighbors list if necessary</summary>
		/// <param name='new_size'>the new size</param>
		protected void ResizeNearestNeighbors(int new_size)
		{
			if (new_size > nearest_neighbors.Count)
				for (int i = nearest_neighbors.Count; i < new_size; i++)
					nearest_neighbors.Add(null);
		}

		///
		public override string ToString()
		{
			return string.Format(
				"{0} k={1} correlation={2} q={3} weighted={4} alpha={5} (only for BidirectionalConditionalProbability)",
				this.GetType().Name, k == uint.MaxValue ? "inf" : k.ToString(), Correlation, Q, Weighted, Alpha);
		}
	}
}