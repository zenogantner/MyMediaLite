// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Linq;

namespace MyMediaLite.DataType
{
	/// <summary>Utilities to work with matrices</summary>
	public static class SparseMatrixExtensions
	{
		/// <summary>return the maximum value contained in a matrix</summary>
		/// <param name='m'>the matrix</param>
		static public int Max(this SparseMatrix<int> m)
		{
			if (m.value_list.Count == 0)
				return 0;
			else
				return m.value_list.Max(r => (r.Count == 0 ? 0 : r.Max()));
		}

		/// <summary>return the maximum value contained in a matrix</summary>
		/// <param name='m'>the matrix</param>
		static public float Max(this SparseMatrix<float> m)
		{
			if (m.value_list.Count == 0)
				return 0;
			else
				return m.value_list.Max(r => (r.Count == 0 ? 0 : r.Max()));
		}

		/// <summary>Compute the Frobenius norm (square root of the sum of squared entries) of a matrix</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Matrix_norm
		/// </remarks>
		/// <param name="matrix">the matrix</param>
		/// <returns>the Frobenius norm of the matrix</returns>
		static public float FrobeniusNorm(this SparseMatrix<float> matrix)
		{
			double squared_entry_sum = 0; // TODO speed-up: go over all entries of value_list
			foreach (var entry in matrix.NonEmptyEntryIDs)
				squared_entry_sum += Math.Pow(matrix[entry.Item1, entry.Item2], 2);
			return (float) Math.Sqrt(squared_entry_sum);
		}
	}
}