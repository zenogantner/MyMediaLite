// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using MyMediaLite.Util;

namespace MyMediaLite.DataType
{
	/// <summary>Class for storing dense matrices</summary>
	/// <remarks>
	/// The data is stored in row-major mode.
	/// Indexes are zero-based.
	/// </remarks>
	/// <typeparam name="T">the type of the matrix entries</typeparam>
	public class Matrix<T> : IMatrix<T>
	{
		/// <summary>Data array: data is stored in columns.</summary>
		public T[] data;
		/// <summary>Dimension 1, the number of rows</summary>
		public int dim1;
		/// <summary>Dimension 2, the number of columns</summary>
		public int dim2;

		///
		public virtual bool IsSymmetric
		{
			get {
				if (dim1 != dim2)
					return false;
				for (int i = 0; i < dim1; i++)
					for (int j = i + 1; j < dim2; j++)
						if (!this[i, j].Equals(this[j, i]))
							return false;
				return true;
			}
		}

		///
		public int NumberOfRows { get { return dim1; } }

		///
		public int NumberOfColumns { get { return dim2; } }

		/// <summary>Initializes a new instance of the Matrix class</summary>
		/// <param name="dim1">the number of rows</param>
		/// <param name="dim2">the number of columns</param>
		public Matrix(int dim1, int dim2)
		{
			if (dim1 < 0)
				throw new ArgumentException("dim1 must be at least 0");
			if (dim2 < 0)
				throw new ArgumentException("dim2 must be at least 0");

			this.dim1 = dim1;
			this.dim2 = dim2;
			this.data = new T[dim1 * dim2];
		}

		/// <summary>Initializes a new instance of the Matrix class</summary>
		/// <param name="dim1">the number of rows</param>
		/// <param name="dim2">the number of columns</param>
		public Matrix(int dim1, uint dim2)
		{
			this.dim1 = dim1;
			this.dim2 = (int) dim2;
			this.data = new T[dim1 * dim2];
		}

		/// <summary>Copy constructor. Creates a deep copy of the given matrix.</summary>
		/// <param name="matrix">the matrix to be copied</param>
		public Matrix(Matrix<T> matrix)
		{
			this.dim1 = matrix.dim1;
			this.dim2 = matrix.dim2;
			this.data = new T[this.dim1 * this.dim2];
			matrix.data.CopyTo(this.data, 0);
		}

		/// <summary>Constructor that takes a list of lists to initialize the matrix</summary>
		/// <param name="data">a list of lists of T</param>
		public Matrix(IList<IList<T>> data)
		{
			this.dim1 = data.Count;
			this.dim2 = data[0].Count;
			this.data = new T[this.dim1 * this.dim2];
			for (int i = 0; i < dim1; i++)
				for (int j = 0; j < dim2; j++)
					this.data[i * dim2 + j] = data[i][j];
		}

		///
		public IMatrix<T> CreateMatrix(int num_rows, int num_columns)
		{
			return new Matrix<T>(num_rows, num_columns);
		}

		///
		public IMatrix<T> Transpose()
		{
			var transpose = new Matrix<T>(NumberOfColumns, NumberOfRows);
			for (int i = 0; i < dim1; i++)
				for (int j = 0; j < dim2; j++)
					transpose.data[j * dim1 + i] = data[i * dim2 + j];
			return transpose;
		}

		///
		public virtual T this [int i, int j]
		{
			get	{
#if DEBUG
				if (i >= this.dim1)
					throw new ArgumentException("i too big: " + i + ", dim1 is " + this.dim1);
				if (j >= this.dim2)
					throw new ArgumentException("j too big: " + j + ", dim2 is " + this.dim2);
#endif
				return data[i * dim2 + j];
			}
			set	{
#if DEBUG
				if (i >= this.dim1)
					throw new ArgumentException("i too big: " + i + ", dim1 is " + this.dim1);
				if (j >= this.dim2)
					throw new ArgumentException("j too big: " + j + ", dim2 is " + this.dim2);
#endif
				data[i * dim2 + j] = value;
			}
		}

		/// <summary>Returns a copy of the i-th row of the matrix</summary>
		/// <param name="i">the row ID</param>
		/// <returns>a list of T containing the row data</returns>
		public IList<T> GetRow(int i)
		{
			T[] row = new T[this.dim2];
			Array.Copy(data, i * dim2, row, 0, dim2);
			return row;
		}

		/// <summary>Returns a copy of the j-th column of the matrix</summary>
		/// <param name="j">the column ID</param>
		/// <returns>a list of T containing the column data</returns>
		public IList<T> GetColumn(int j)
		{
			T[] column = new T[this.dim1];
			for (int x = 0; x < this.dim1; x++)
				column[x] = this[x, j];
			return column;
		}

		/// <summary>Sets the values of the i-th row to the values in a given array</summary>
		/// <param name="i">the row ID</param>
		/// <param name="row">a list of T of length dim1</param>
		public void SetRow(int i, IList<T> row)
		{
			if (row.Count != this.dim2)
				throw new ArgumentException(string.Format("Array length ({0}) must equal number of columns ({1}",
														  row.Count, this.dim2));

			row.CopyTo(data, i * dim2);
		}

		/// <summary>Sets the values of the j-th column to the values in a given array</summary>
		/// <param name="j">the column ID</param>
		/// <param name="column">a list of T of length dim2</param>
		public void SetColumn(int j, IList<T> column)
		{
			if (column.Count != this.dim1)
				throw new ArgumentException(string.Format("Array length ({0}) must equal number of rows ({1}",
														  column.Count, this.dim1));

			for (int i = 0; i < this.dim1; i++)
				this[i, j] = column[i];
		}

		/// <summary>Init the matrix with a default value</summary>
		/// <param name="d">the default value</param>
		public void Init(T d)
		{
			for (int i = 0; i < dim1 * dim2; i++)
				data[i] = d;
		}

		/// <summary>Enlarges the matrix to num_rows rows</summary>
		/// <remarks>
		/// Do nothing if num_rows is less than dim1.
		/// The new entries are filled with zeros.
		/// </remarks>
		/// <param name="num_rows">the minimum number of rows</param>
		public void AddRows(int num_rows)
		{
			if (num_rows > dim1)
			{
				// create new data structure
				var data_new = new T[num_rows * dim2];
				data.CopyTo(data_new, 0);

				// replace old data structure
				this.dim1 = num_rows;
				this.data = data_new;
			}
		}

		/// <summary>Grows the matrix to the requested size, if necessary</summary>
		/// <remarks>
		/// The new entries are filled with zeros.
		/// </remarks>
		/// <param name="num_rows">the minimum number of rows</param>
		/// <param name="num_cols">the minimum number of columns</param>
		public void Grow(int num_rows, int num_cols)
		{
			if (num_rows > dim1 || num_cols > dim2)
			{
				// create new data structure
				var new_data = new T[num_rows * num_cols];
				for (int i = 0; i < dim1; i++)
					for (int j = 0; j < dim2; j++)
						new_data[i * num_cols + j] = this[i, j];

				// replace old data structure
				this.dim1 = num_rows;
				this.dim2 = num_cols;
				this.data = new_data;
			}
		}

		/// <summary>Sets an entire row to a specified value</summary>
		/// <param name="v">the value to be used</param>
		/// <param name="i">the row ID</param>
		public void SetRowToOneValue(int i, T v)
		{
			for (int j = 0; j < dim2; j++)
				this[i, j] = v;
		}

		/// <summary>
		/// Sets an entire column to a specified value
		/// </summary>
		/// <param name="v">the value to be used</param>
		/// <param name="j">the column ID</param>
		public void SetColumnToOneValue(int j, T v)
		{
			for (int i = 0; i < dim1; i++)
				this[i, j] = v;
		}

	}
}