// Copyright (C) 2011 Zeno Gantner
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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

namespace MyMediaLite.DataType
{
	/// <summary>Generic interface for matrix data types</summary>
	public interface IMatrix<T>
	{
		/// <summary>The value at (i,j)</summary>
		/// <value>The value at (i,j)</value>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		T this [int x, int y] { get; set; }

		/// <summary>The number of rows of the matrix</summary>
		/// <value>The number of rows of the matrix</value>
		int NumberOfRows { get; }

		/// <summary>The number of columns of the matrix</summary>
		/// <value>The number of columns of the matrix</value>
		int NumberOfColumns { get; }

		/// <summary>True if the matrix is symmetric, false otherwise</summary>
		/// <value>True if the matrix is symmetric, false otherwise</value>
		bool IsSymmetric { get; }

		/// <summary>Get the transpose of the matrix, i.e. a matrix where rows and columns are interchanged</summary>
		/// <returns>the transpose of the matrix (copy)</returns>
		IMatrix<T> Transpose();

		/// <summary>Create a matrix with a given number of rows and columns</summary>
		/// <param name="num_rows">the number of rows</param>
		/// <param name="num_columns">the number of columns</param>
		/// <returns>A matrix with num_rows rows and num_column columns</returns>
		IMatrix<T> CreateMatrix(int num_rows, int num_columns);
	}
}

