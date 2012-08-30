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
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	// TODO Jaccard, Cosine, and this actually only differ in a few lines - DRY!
	
	/// <summary>Class for storing and computing conditional probabilities</summary>
	/// <remarks>
	/// </remarks>
	public sealed class ConditionalProbability : AsymmetricCorrelationMatrix, IBinaryDataCorrelationMatrix
	{
		/// <summary>Creates an object of type Jaccard</summary>
		/// <param name="num_entities">the number of entities</param>
		public ConditionalProbability(int num_entities) : base(num_entities) { }

		/// <summary>Creates conditional probability matrix from given data</summary>
		/// <param name="vectors">the boolean data</param>
		/// <returns>the similarity matrix based on the data</returns>
		static public ConditionalProbability Create(IBooleanMatrix vectors)
		{
			ConditionalProbability cm;
			int num_entities = vectors.NumberOfRows;
			try
			{
				cm = new ConditionalProbability(num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			cm.ComputeCorrelations(vectors);
			return cm;
		}

		///
		public void ComputeCorrelations(IBooleanMatrix entity_data)
		{
			var transpose = entity_data.Transpose() as IBooleanMatrix;

			var overlap = new SymmetricMatrix<int>(entity_data.NumberOfRows);

			// go over all (other) entities
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					for (int j = 0; j < row.Count; j++)
					{
						int y = row[j];
						overlap[x, y]++;
					}
				}
			}

			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			// compute conditional probabilities
			for (int x = 0; x < num_entities; x++)
				for (int y = 0; y < x; y++)
					this[x, y] = (float) (overlap[x, y] / entity_data.NumEntriesByRow(x));
		}

		/// <summary>Computes the Jaccard index of two binary vectors</summary>
		/// <param name="vector_i">the first vector</param>
		/// <param name="vector_j">the second vector</param>
		/// <returns>the cosine similarity between the two vectors</returns>
		public static float ComputeCorrelation(HashSet<int> vector_i, HashSet<int> vector_j)
		{
			int cntr = 0;
			foreach (int k in vector_j)
				if (vector_i.Contains(k))
					cntr++;
			return (float) ( cntr / vector_i.Count);
		}
	}
}
