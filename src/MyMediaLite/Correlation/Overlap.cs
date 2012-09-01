// Copyright (C) 2012 Zeno Gantner
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
//
using System;
using System.Collections.Generic;
using MyMediaLite.DataType;

namespace MyMediaLite.Correlation
{
	/// <summary>Class containing routines for computing overlaps</summary>
	public static class Overlap
	{
		/// <summary>Compute the overlap between the vectors in a binary matrix</summary>
		/// <returns>a sparse matrix with the overlaps</returns>
		/// <param name='entity_data'>the binary matrix</param>
		public static Tuple<IMatrix<float>, IList<float>> ComputeWeighted(IBooleanMatrix entity_data)
		{
			var transpose = (IBooleanMatrix) entity_data.Transpose();

			var other_entity_weights = new float[transpose.NumberOfRows];
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				int freq = transpose.GetEntriesByRow(row_id).Count;
				other_entity_weights[row_id] = 1f / (float) Math.Log(3 + freq, 2); // TODO make configurable
			}

			IMatrix<float> weighted_overlap = new SymmetricMatrix<float>(entity_data.NumberOfRows);
			IList<float> entity_weights = new float[entity_data.NumberOfRows];

			// go over all (other) entities
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					entity_weights[x] += other_entity_weights[row_id];
					for (int j = i + 1; j < row.Count; j++)
					{
						int y = row[j];
						weighted_overlap[x, y] += other_entity_weights[row_id] * other_entity_weights[row_id];
					}
				}
			}

			return Tuple.Create(weighted_overlap, entity_weights);
		}

		/// <summary>Compute the overlap between the vectors in a binary matrix</summary>
		/// <returns>a sparse matrix with the overlaps</returns>
		/// <param name='entity_data'>the binary matrix</param>
		public static IMatrix<uint> ComputeUInt(IBooleanMatrix entity_data)
		{
			var transpose = entity_data.Transpose() as IBooleanMatrix;

			var overlap = new SymmetricSparseMatrix<uint>(entity_data.NumberOfRows);

			// go over all (other) entities
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					for (int j = i + 1; j < row.Count; j++)
						overlap[x, row[j]]++;
				}
			}
			return overlap;
		}

		/// <summary>Computes the overlap between the vectors in a binary matrix</summary>
		/// <returns>a sparse matrix with the overlaps</returns>
		/// <param name='entity_data'>the binary matrix</param>
		public static IMatrix<ushort> ComputeUShort(IBooleanMatrix entity_data)
		{
			var transpose = entity_data.Transpose() as IBooleanMatrix;

			var overlap = new SymmetricSparseMatrix<ushort>(entity_data.NumberOfRows);

			// go over all (other) entities
			for (int row_id = 0; row_id < transpose.NumberOfRows; row_id++)
			{
				var row = transpose.GetEntriesByRow(row_id);
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					for (int j = i + 1; j < row.Count; j++)
						overlap[x, row[j]]++;
				}
			}
			return overlap;
		}
	}
}

