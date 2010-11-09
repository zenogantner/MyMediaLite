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
using MyMediaLite.util;


namespace MyMediaLite.data_type
{
    /// <summary>Class for storing dense matrices</summary>
    /// <remarks>
    /// The data is stored in row-major mode.
    /// Indexes are zero-based.
    /// </remarks>
    public class Matrix<T>
    {
        /// <summary>Data array: data is stored in columns.</summary>
        public T[] data;
        /// <summary>Dimension 1, the number of rows</summary>
        public int dim1;
        /// <summary>Dimension 2, the number of columns</summary>
        public int dim2;

		// TODO IMatrix interface
		// TODO properties NumberOfRows NumberOfColumns
		
		/// <summary>True if the matrix is symmetric, false otherwise</summary>
		public virtual bool IsSymmetric
		{
			get
			{
				for (int i = 0; i < dim1; i++)
					for (int j = i + 1; j < dim2; j++)
						if (!this[i, j].Equals(this[j, i]))
							return false;
				return true;
			}
		}

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
            data = new T[dim1 * dim2];
        }

        /// <summary>
        /// Copy constructor. Creates a deep copy of the given matrix.
        /// </summary>
        /// <param name="matrix">the matrix to be copied</param>
        public Matrix(Matrix<T> matrix)
        {
        	this.dim1 = matrix.dim1;
        	this.dim2 = matrix.dim2;
        	data = new T[this.dim1 * this.dim2];
			matrix.data.CopyTo(this.data, 0);
		}

        /// <summary>The value at (i,j)</summary>
        /// <param name="i">the row ID</param>
        /// <param name="j">the column ID</param>
        public virtual T this [int i, int j]
        {
			get
			{
        		if (i >= this.dim1)
        			throw new ArgumentException("i too big: " + i + ", dim1 is " + this.dim1);
				if (j >= this.dim2)
					throw new ArgumentException("j too big: " + j + ", dim2 is " + this.dim2);

            	return data[i * dim2 + j];
			}
			set
        	{
            	data[i * dim2 + j] = value;
        	}
		}

		/// <summary>Returns a copy of the i-th row of the matrix</summary>
		/// <param name="i">the row ID</param>
		/// <returns>A <see cref="T[]"/> containing the row data</returns>
		public T[] GetRow(int i)
		{
			// TODO speed up using Array.Copy()?
			T[] row = new T[this.dim2];
			for (int x = 0; x < this.dim2; x++)
				row[x] = this[i, x];
			return row;
		}

		/// <summary>Returns a copy of the j-th column of the matrix</summary>
		/// <param name="j">the column ID</param>
		/// <returns><see cref="T[]"/> containing the column data</returns>
		public T[] GetColumn(int j)
		{
			T[] column = new T[this.dim1];
			for (int x = 0; x < this.dim1; x++)
				column[x] = this[x, j];
			return column;
		}

		/// <summary>Sets the values of the i-th row to the values in a given array</summary>
		/// <param name="i">the row ID</param>
		/// <param name="row">A <see cref="T[]"/> of length dim1</param>
		public void SetRow(int i, T[] row)
		{
			// TODO speed up using Array.Copy()?
			if (row.Length != this.dim2)
				throw new ArgumentException(string.Format("Array length ({0}) must equal number of columns ({1}",
				                                          row.Length, this.dim2));

			for (int j = 0; j < this.dim2; j++)
				this[i, j] = row[j];
		}

		/// <summary>
		/// Sets the values of the j-th column to the values in a given array
		/// </summary>
		/// <param name="j">the column ID</param>
		/// <param name="column">A <see cref="T[]"/> of length dim2</param>
		public void SetColumn(int j, T[] column)
		{
			if (column.Length != this.dim1)
				throw new ArgumentException(string.Format("Array length ({0}) must equal number of columns ({1}",
				                                          column.Length, this.dim1));

			for (int i = 0; i < this.dim1; i++)
				this[i, j] = column[i];
		}

		/// <summary>
        /// Init the matrix with a default value
        /// </summary>
        /// <param name="d">the default value</param>
        public void Init(T d)
        {
            for (int i = 0; i < dim1 * dim2; i++)
                data[i] = d;
        }

        /// <summary>
        /// Enlarges the matrix to num_rows rows.
        /// Do nothing if num_rows is less than dim1.
        /// The new entries are filled with zeros.
        /// </summary>
        /// <param name="num_rows">the minimum number of rows</param>
        public void AddRows(int num_rows)
        {
            if (num_rows > dim1)
            {
				// create new data structure
                T[] data_new = new T[num_rows * dim2];
                data.CopyTo(data_new, 0);

				// replace old data structure
                this.dim1 = num_rows;
                this.data = data_new;
            }
        }

        /// <summary>
        /// Grows the matrix to the requested size, if necessary
		///
		/// The new entries are filled with zeros.
        /// </summary>
        /// <param name="num_rows">the minimum number of rows</param>
        /// <param name="num_cols">the minimum number of columns</param>
        public void Grow(int num_rows, int num_cols)
        {
			if (num_rows > dim1 || num_cols > dim2)
			{
				// create new data structure
				T[] new_data = new T[num_rows * num_cols];
				for (int i = 0; i < dim1; i++)
					for (int j = 0; j < dim2; j++)
						new_data[i * num_cols + j] = this[i, j];

				// replace old data structure
				this.dim1 = num_rows;
				this.dim2 = num_cols;
				this.data = new_data;
			}
        }

		/// <summary>
		/// Sets an entire row to a specified value
		/// </summary>
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

    /// <summary>
    /// Utilities to work with matrices.
    /// </summary>
    public class MatrixUtils
    {
        /// <summary>
        /// Initializes one row of a double matrix with normal distributed (Gaussian) noise
        /// </summary>
        /// <param name="matrix">the matrix to initialize</param>
        /// <param name="mean">the mean of the normal distribution drawn from</param>
        /// <param name="stdev">the standard deviation of the normal distribution</param>
        /// <param name="row">the row to be initialized</param>
        static public void InitNormal(Matrix<double> matrix, double mean, double stdev, int row)
        {
            var rnd = MyMediaLite.util.Random.GetInstance();
            for (int j = 0; j < matrix.dim2; j++)
                matrix[row, j] = rnd.NextNormal(mean, stdev);
        }

        /// <summary>
        /// Initializes a double matrix with normal distributed (Gaussian) noise
        /// </summary>
        /// <param name="matrix">the matrix to initialize</param>
        /// <param name="mean">the mean of the normal distribution drawn from</param>
        /// <param name="stdev">the standard deviation of the normal distribution</param>
        static public void InitNormal(Matrix<double> matrix, double mean, double stdev)
        {
            var rnd = MyMediaLite.util.Random.GetInstance();
            for (int i = 0; i < matrix.dim1; i++)
                for (int j = 0; j < matrix.dim2; j++)
                    matrix[i, j] = rnd.NextNormal(mean, stdev);
        }

		/// <summary>
        /// Increments the specified matrix element by a double value.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="i">the row</param>
        /// <param name="j">the column</param>
        /// <param name="v">the value</param>
        static public void Inc(Matrix<double> matrix, int i, int j, double v)
        {
            matrix.data[i * matrix.dim2 + j] += v;
        }

		/// <summary>
		/// Increment the elements in one matrix by the ones in another
		/// </summary>
		/// <param name="matrix1">the matrix to be incremented</param>
		/// <param name="matrix2">the other matrix</param>
        static public void Inc(Matrix<double> matrix1, Matrix<double> matrix2)
        {
			if (matrix1.dim1 != matrix2.dim1 || matrix1.dim2 != matrix2.dim2)
				throw new ArgumentException("Matrix sizes do not match.");

			int dim1 = matrix1.dim1;
			int dim2 = matrix1.dim2;

			for (int x = 0; x < dim1; x++)
				for (int y = 0; y < dim2; y++)
					matrix1.data[x * dim2 + y] += matrix2.data[x * dim2 + y];
        }

		/// <summary>
		/// Compute the average value of the entries in a column of a matrix
		/// </summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="col">the column ID</param>
		/// <returns>the average</returns>
		static public double ColumnAverage(Matrix<double> matrix, int col)
		{
			if (matrix.dim1 == 0)
				throw new Exception("Cannot compute average of 0 entries.");

			double sum = 0;

			for (int x = 0; x < matrix.dim1; x++)
				sum += matrix.data[x * matrix.dim2 + col];

			return sum / matrix.dim1;
		}

		/// <summary>
		/// Compute the average value of the entries in a row of a matrix
		/// </summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="row">the row ID</param>
		/// <returns>the average</returns>
		static public double RowAverage(Matrix<double> matrix, int row)
		{
			if (matrix.dim2 == 0)
				throw new Exception("Cannot compute average of 0 entries.");

			double sum = 0;

			for (int y = 0; y < matrix.dim2; y++)
				sum += matrix.data[row * matrix.dim2 + y];

			return sum / matrix.dim2;
		}

		/// <summary>
		/// Multiply all entries of a matrix with a scalar
		/// </summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="d">the number to multiply with</param>
		static public void Multiply(Matrix<double> matrix, double d)
		{
			for (int x = 0; x < matrix.dim1; x++)
				for (int y = 0; y < matrix.dim2; y++)
					matrix.data[x * matrix.dim2 + y] *= d;
		}

		/// <summary>
		/// Compute the Frobenius norm (square root of the sum of squared entries) of a matrix
		/// </summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Matrix_norm
		/// </remarks>
		/// <param name="matrix">the matrix</param>
		/// <returns>the Frobenius norm of the matrix</returns>
		static public double FrobeniusNorm(Matrix<double> matrix)
		{
			double squared_entry_sum = 0;
			for (int x = 0; x < matrix.dim1 * matrix.dim2; x++)
				squared_entry_sum += Math.Pow(matrix.data[x], 2);
			return Math.Sqrt(squared_entry_sum);
		}

		/// <summary>
		/// Compute the scalar product between a vector and a row of the matrix
		/// </summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="i">the row ID</param>
		/// <param name="vector">the numeric vector</param>
		/// <returns>the scalar product of row i and the vector</returns>
		static public double RowScalarProduct(Matrix<double> matrix, int i, double[] vector)
		{
        	if (i >= matrix.dim1)
        		throw new ArgumentException("i too big: " + i + ", dim1 is " + matrix.dim1);
			if (vector.Length != matrix.dim2)
				throw new ArgumentException("wrong vector size: " + vector.Length + ", dim2 is " + matrix.dim2);

            double result = 0;
            for (int j = 0; j < matrix.dim2; j++)
                result += matrix.data[i * matrix.dim2 + j] * vector[j];

            return result;
		}

		/// <summary>
		/// Check whether a matrix contains NaN ("not a number") elements
		/// </summary>
		/// <param name="matrix">a matrix</param>
		/// <returns>true if there are NaN elements in the matrix, false otherwise</returns>
		static public bool ContainsNaN(Matrix<double> matrix)
		{
			int nan_counter = 0;
            for (int x = 0; x < matrix.dim1; x++)
                for (int y = 0; y < matrix.dim2; y++)
                    if ( Double.IsNaN(matrix[x, y]) )
						nan_counter++;
			if (nan_counter > 0) {
				Console.Error.WriteLine("Number of NaNs: " + nan_counter);
				return true;
			}
			return false;
		}
    }
}