// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.Correlation
{
	/// <summary>Class with commoin routines for asymmetric correlations that are learned from binary data</summary>
	public abstract class BinaryDataAsymmetricCorrelationMatrix : AsymmetricCorrelationMatrix, IBinaryDataCorrelationMatrix
	{
		///
		public bool Weighted { get; set; }

		/// <summary>Creates an object of type BinaryDataAsymmetricCorrelationMatrix</summary>
		/// <param name="num_entities">the number of entities</param>
		public BinaryDataAsymmetricCorrelationMatrix(int num_entities) : base(num_entities) { }

		///
		protected abstract float ComputeCorrelationFromOverlap(float overlap, float count_x, float count_y);

		///
		//public void ComputeCorrelations(IInteractions interactions, EntityType entity_type)

		///
		public void ComputeCorrelations(IBooleanMatrix entity_data)
		{
			Resize(entity_data.NumberOfRows);

			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			Tuple<IMatrix<float>, IList<float>> overlap_and_entity_weights;
			if (Weighted)
				overlap_and_entity_weights = Overlap.ComputeWeighted(entity_data);
			else
				overlap_and_entity_weights = Overlap.Compute(entity_data);
			ComputeCorrelations(overlap_and_entity_weights.Item1, overlap_and_entity_weights.Item2);
		}

		///
		public override void ComputeCorrelations(IInteractions interactions, EntityType entity_type)
		{
			Tuple<IMatrix<float>, IList<float>> overlap_and_entity_weights;
			if (Weighted)
				overlap_and_entity_weights = Overlap.ComputeWeighted(interactions, entity_type);
			else
				overlap_and_entity_weights = Overlap.Compute(interactions, entity_type);
			ComputeCorrelations(overlap_and_entity_weights.Item1, overlap_and_entity_weights.Item2);
		}

		void ComputeCorrelations(IMatrix<float> overlap, IList<float> entity_weights)
		{
			for (int x = 0; x < num_entities; x++)
				for (int y = 0; y < x; y++)
				{
					this[x, y] = ComputeCorrelationFromOverlap(overlap[x, y], entity_weights[x], entity_weights[y]);
					this[y, x] = ComputeCorrelationFromOverlap(overlap[x, y], entity_weights[y], entity_weights[x]);
				}
		}

		///
		public float ComputeCorrelation(ICollection<int> vector_i, ICollection<int> vector_j)
		{
			uint count = 0;
			foreach (int k in vector_j)
				if (vector_i.Contains(k))
					count++;

			if (Weighted)
				throw new NotImplementedException();
			else
				return ComputeCorrelationFromOverlap(count, vector_i.Count, vector_j.Count);
		}
	}
}
