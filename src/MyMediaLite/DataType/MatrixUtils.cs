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
using System.Globalization;
using System.IO;
using MyMediaLite.Util;

namespace MyMediaLite.DataType
{
	/// <summary>Utilities to work with matrices</summary>
	public class MatrixUtils
	{
		/// <summary>Initializes one row of a double matrix with normal distributed (Gaussian) noise</summary>
		/// <param name="matrix">the matrix to initialize</param>
		/// <param name="mean">the mean of the normal distribution drawn from</param>
		/// <param name="stdev">the standard deviation of the normal distribution</param>
		/// <param name="row">the row to be initialized</param>
		static public void RowInitNormal(Matrix<double> matrix, double mean, double stdev, int row)
		{
			var rnd = Util.Random.GetInstance();
			for (int j = 0; j < matrix.dim2; j++)
				matrix[row, j] = rnd.NextNormal(mean, stdev);
		}

		/// <summary>Initializes one column of a double matrix with normal distributed (Gaussian) noise</summary>
		/// <param name="matrix">the matrix to initialize</param>
		/// <param name="mean">the mean of the normal distribution drawn from</param>
		/// <param name="stdev">the standard deviation of the normal distribution</param>
		/// <param name="column">the column to be initialized</param>
		static public void ColumnInitNormal(Matrix<double> matrix, double mean, double stdev, int column)
		{
			var rnd = Util.Random.GetInstance();
			for (int i = 0; i < matrix.dim1; i++)
				matrix[i, column] = rnd.NextNormal(mean, stdev);
		}

		/// <summary>Initializes a double matrix with normal distributed (Gaussian) noise</summary>
		/// <param name="matrix">the matrix to initialize</param>
		/// <param name="mean">the mean of the normal distribution drawn from</param>
		/// <param name="stdev">the standard deviation of the normal distribution</param>
		static public void RowInitNormal(Matrix<double> matrix, double mean, double stdev)
		{
			var rnd = Util.Random.GetInstance();
			for (int i = 0; i < matrix.dim1; i++)
				for (int j = 0; j < matrix.dim2; j++)
					matrix[i, j] = rnd.NextNormal(mean, stdev);
		}

		/// <summary>Increments the specified matrix element by a double value</summary>
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

		/// <summary>Compute the average value of the entries in a column of a matrix</summary>
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

		/// <summary>Compute the average value of the entries in a row of a matrix</summary>
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

		/// <summary>Compute the scalar product between a vector and a row of the matrix</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="i">the row ID</param>
		/// <param name="vector">the numeric vector</param>
		/// <returns>the scalar product of row i and the vector</returns>
		static public double RowScalarProduct(Matrix<double> matrix, int i, IList<double> vector)
		{
			if (i >= matrix.dim1)
				throw new ArgumentException("i too big: " + i + ", dim1 is " + matrix.dim1);
			if (vector.Count != matrix.dim2)
				throw new ArgumentException("wrong vector size: " + vector.Count + ", dim2 is " + matrix.dim2);

			double result = 0;
			for (int j = 0; j < matrix.dim2; j++)
				result += matrix.data[i * matrix.dim2 + j] * vector[j];

			return result;
		}

		/// <summary>Compute the scalar product between two rows of two matrices</summary>
		/// <param name="matrix1">the first matrix</param>
		/// <param name="i">the first row ID</param>
		/// <param name="matrix2">the second matrix</param>
		/// <param name="j">the second row ID</param>
		/// <returns>the scalar product of row i of matrix1 and row j of matrix2</returns>
		static public double RowScalarProduct(Matrix<double> matrix1, int i, Matrix<double> matrix2, int j)
		{
			if (i >= matrix1.dim1)
				throw new ArgumentException("i too big: " + i + ", dim1 is " + matrix1.dim1);
			if (j >= matrix2.dim1)
				throw new ArgumentException("j too big: " + j + ", dim1 is " + matrix2.dim1);

			if (matrix1.dim2 != matrix2.dim2)
				throw new ArgumentException("wrong row size: " + matrix1.dim2 + " vs. " + matrix2.dim2);

			double result = 0;
			for (int c = 0; c < matrix1.dim2; c++)
				result += matrix1.data[i * matrix1.dim2 + c] * matrix2.data[j * matrix2.dim2 + c];

			return result;
		}
		// TODO unit tests
	}
}