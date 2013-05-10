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
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>Class with common routines for symmetric correlations that are learned from binary data</summary>
	public abstract class BinaryDataSymmetricCorrelationMatrix : SymmetricCorrelationMatrix, IBinaryDataCorrelationMatrix
	{
		///
		public bool Weighted { get; set; }

		/// <summary>Creates an object of type BinaryDataCorrelation</summary>
		/// <param name="num_entities">the number of entities</param>
		/// <param name="weighted">if true, correlations based on more observations will be given higher weight</param>
		public BinaryDataSymmetricCorrelationMatrix(int num_entities, bool weighted = false) : base(num_entities)
		{
			Weighted = weighted;
		}

		///
		protected abstract float ComputeCorrelationFromOverlap(float overlap, float count_x, float count_y);

		///
		public void ComputeCorrelations(IBooleanMatrix entity_data)
		{
			Resize(entity_data.NumberOfRows);

			// the diagonal of the correlation matrix
			for (int i = 0; i < NumEntities; i++)
				this[i, i] = 1;
			
			Tuple<IMatrix<float>, IList<float>> overlap_and_entity_weights;
			if (Weighted)
				overlap_and_entity_weights = Overlap.ComputeWeighted(entity_data);
			else
				overlap_and_entity_weights = Overlap.Compute(entity_data);
			ComputeCorrelations(overlap_and_entity_weights.Item1, overlap_and_entity_weights.Item2);
		}

		void ComputeCorrelations(IMatrix<float> overlap, IList<float> entity_weights)
		{
			for (int x = 0; x < NumEntities; x++)
				for (int y = 0; y < x; y++)
					this[x, y] = ComputeCorrelationFromOverlap(overlap[x, y], entity_weights[x], entity_weights[y]);
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