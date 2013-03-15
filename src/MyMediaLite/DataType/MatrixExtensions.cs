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
using System.Globalization;
using System.IO;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace MyMediaLite.DataType
{
	/// <summary>Utilities to work with matrices</summary>
	public static class MatrixExtensions
	{
		/// <summary>Initializes one row of a float matrix with normal distributed (Gaussian) noise</summary>
		/// <param name="matrix">the matrix to initialize</param>
		/// <param name="row">the row to be initialized</param>
		/// <param name="mean">the mean of the normal distribution drawn from</param>
		/// <param name="stddev">the standard deviation of the normal distribution</param>
		static public void RowInitNormal(this Matrix<float> matrix, int row, double mean, double stddev)
		{
			var nd = new Normal(mean, stddev);
			nd.RandomSource = MyMediaLite.Random.GetInstance();

			for (int j = 0; j < matrix.dim2; j++)
				matrix[row, j] = (float) nd.Sample();
		}

		/// <summary>Initializes one column of a float matrix with normal distributed (Gaussian) noise</summary>
		/// <param name="matrix">the matrix to initialize</param>
		/// <param name="mean">the mean of the normal distribution drawn from</param>
		/// <param name="stddev">the standard deviation of the normal distribution</param>
		/// <param name="column">the column to be initialized</param>
		static public void ColumnInitNormal(this Matrix<float> matrix, int column, double mean, double stddev)
		{
			var nd = new Normal(mean, stddev);
			nd.RandomSource = MyMediaLite.Random.GetInstance();

			for (int i = 0; i < matrix.dim1; i++)
				matrix[i, column] = (float) nd.Sample();
		}

		/// <summary>Initializes a float matrix with normal distributed (Gaussian) noise</summary>
		/// <param name="matrix">the matrix to initialize</param>
		/// <param name="mean">the mean of the normal distribution drawn from</param>
		/// <param name="stddev">the standard deviation of the normal distribution</param>
		static public void InitNormal(this Matrix<float> matrix, double mean, double stddev)
		{
			var nd = new Normal(mean, stddev);
			nd.RandomSource = MyMediaLite.Random.GetInstance();

			for (int i = 0; i < matrix.data.Length; i++)
				matrix.data[i] = (float) nd.Sample();
		}

		/// <summary>Increments the specified matrix element by a double value</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="i">the row</param>
		/// <param name="j">the column</param>
		/// <param name="v">the value</param>
		static public void Inc(this Matrix<float> matrix, int i, int j, double v)
		{
			matrix.data[i * matrix.dim2 + j] += (float) v;
		}

		/// <summary>Increment the elements in one matrix by the ones in another</summary>
		/// <param name="matrix1">the matrix to be incremented</param>
		/// <param name="matrix2">the other matrix</param>
		static public void Inc(this Matrix<float> matrix1, Matrix<float> matrix2)
		{
			if (matrix1.dim1 != matrix2.dim1 || matrix1.dim2 != matrix2.dim2)
				throw new ArgumentOutOfRangeException("Matrix sizes do not match.");

			for (int i = 0; i < matrix1.data.Length; i++)
				matrix1.data[i] += matrix2.data[i];
		}

		/// <summary>Increments the specified matrix element by 1</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="i">the row</param>
		/// <param name="j">the column</param>
		static public void Inc(this Matrix<int> matrix, int i, int j)
		{
			matrix.data[i * matrix.dim2 + j]++;
		}

		/// <summary>Increment the seach matrix element by a given value</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="v">the value to increment with</param>
		static public void Inc(this Matrix<float> matrix, float v)
		{
			for (int i = 0; i < matrix.data.Length; i++)
				matrix.data[i] += v;
		}
		
		/// <summary>Multiplies one column of a matrix with a scalar</summary>
		/// <param name='matrix'>the matrix</param>
		/// <param name='j'>the column ID</param>
		/// <param name='scalar'>the scalar value to multiply with</param>
		static public void MultiplyColumn(this Matrix<float> matrix, int j, float scalar)
		{
			for (int i = 0; i < matrix.dim1; i++)
				matrix.data[i * matrix.dim2 + j] *= scalar;
		}

		/// <summary>Sum up a given number of rows of a matrix</summary>
		/// <returns>The vector representing the sum of the given rows</returns>
		/// <param name='matrix'>the matrix</param>
		/// <param name='row_ids'>a collection of row IDs</param>
		static public IList<float> SumOfRows(this Matrix<float> matrix, ICollection<int> row_ids)
		{
			var result = new float[matrix.dim2];
			foreach (int row_id in row_ids)
			{
				int offset = row_id * matrix.dim2;
				for (int j = 0; j < matrix.dim2; j++)
					result[j] += matrix.data[offset + j];
			}

			return result;
		}

		/// <summary>Compute the average value of the entries in a column of a matrix</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="col">the column ID</param>
		/// <returns>the average</returns>
		static public float ColumnAverage(this Matrix<float> matrix, int col)
		{
			if (matrix.dim1 == 0)
				throw new ArgumentOutOfRangeException("Cannot compute average of 0 entries.");

			double sum = 0;

			for (int x = 0; x < matrix.dim1; x++)
				sum += matrix.data[x * matrix.dim2 + col];

			return (float) (sum / matrix.dim1);
		}

		/// <summary>Multiply all entries of a matrix with a scalar</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="f">the number to multiply with</param>
		static public void Multiply(this Matrix<float> matrix, float f)
		{
			for (int i = 0; i < matrix.data.Length; i++)
				matrix.data[i] *= f;
		}

		/// <summary>Compute the Frobenius norm (square root of the sum of squared entries) of a matrix</summary>
		/// <remarks>
		/// See http://en.wikipedia.org/wiki/Matrix_norm
		/// </remarks>
		/// <param name="matrix">the matrix</param>
		/// <returns>the Frobenius norm of the matrix</returns>
		static public float FrobeniusNorm(this Matrix<float> matrix)
		{
			double squared_entry_sum = 0;
			for (int i = 0; i < matrix.data.Length; i++)
				squared_entry_sum += Math.Pow(matrix.data[i], 2);
			return (float) Math.Sqrt(squared_entry_sum);
		}

		/// <summary>Compute the scalar product between a vector and a row of the matrix</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="i">the row ID</param>
		/// <param name="vector">the numeric vector</param>
		/// <returns>the scalar product of row i and the vector</returns>
		static public float RowScalarProduct(this Matrix<float> matrix, int i, IList<float> vector)
		{
			if (i >= matrix.dim1)
				throw new ArgumentOutOfRangeException("i too big: " + i + ", dim1 is " + matrix.dim1);
			if (vector.Count != matrix.dim2)
				throw new ArgumentOutOfRangeException("wrong vector size: " + vector.Count + ", dim2 is " + matrix.dim2);

			float result = 0;
			int offset = i * matrix.dim2;
			for (int j = 0; j < matrix.dim2; j++)
				result += matrix.data[offset + j] * vector[j];

			return result;
		}

		/// <summary>Compute the scalar product between a vector and a row of the matrix</summary>
		/// <param name="matrix">the matrix</param>
		/// <param name="i">the row ID</param>
		/// <param name="vector">the numeric vector</param>
		/// <returns>the scalar product of row i and the vector</returns>
		static public double RowScalarProduct(this Matrix<float> matrix, int i, IList<double> vector)
		{
			if (i >= matrix.dim1)
				throw new ArgumentOutOfRangeException("i too big: " + i + ", dim1 is " + matrix.dim1);
			if (vector.Count != matrix.dim2)
				throw new ArgumentOutOfRangeException("wrong vector size: " + vector.Count + ", dim2 is " + matrix.dim2);

			double result = 0;
			int offset = i * matrix.dim2;
			for (int j = 0; j < matrix.dim2; j++)
				result += matrix.data[offset + j] * vector[j];

			return result;
		}

		/// <summary>Compute the scalar product between two rows of two matrices</summary>
		/// <param name="matrix1">the first matrix</param>
		/// <param name="i">the first row ID</param>
		/// <param name="matrix2">the second matrix</param>
		/// <param name="j">the second row ID</param>
		/// <returns>the scalar product of row i of matrix1 and row j of matrix2</returns>
		static public float RowScalarProduct(Matrix<float> matrix1, int i, Matrix<float> matrix2, int j)
		{
			if (i >= matrix1.dim1)
				throw new ArgumentOutOfRangeException("i too big: " + i + ", dim1 is " + matrix1.dim1);
			if (j >= matrix2.dim1)
				throw new ArgumentOutOfRangeException("j too big: " + j + ", dim1 is " + matrix2.dim1);

			if (matrix1.dim2 != matrix2.dim2)
				throw new ArgumentException("wrong row size: " + matrix1.dim2 + " vs. " + matrix2.dim2);

			float result = 0;
			int offset1 = i * matrix1.dim2;
			int offset2 = j * matrix2.dim2;
			for (int c = 0; c < matrix1.dim2; c++)
				result += matrix1.data[offset1 + c] * matrix2.data[offset2 + c];

			return result;
		}

		/// <summary>Compute the difference vector between two rows of two matrices</summary>
		/// <param name="matrix1">the first matrix</param>
		/// <param name="i">the first row ID</param>
		/// <param name="matrix2">the second matrix</param>
		/// <param name="j">the second row ID</param>
		/// <returns>the difference vector of row i of matrix1 and row j of matrix2</returns>
		static public IList<float> RowDifference(Matrix<float> matrix1, int i, Matrix<float> matrix2, int j)
		{
			if (i >= matrix1.dim1)
				throw new ArgumentOutOfRangeException("i too big: " + i + ", dim1 is " + matrix1.dim1);
			if (j >= matrix2.dim1)
				throw new ArgumentOutOfRangeException("j too big: " + j + ", dim1 is " + matrix2.dim1);

			if (matrix1.dim2 != matrix2.dim2)
				throw new ArgumentException("wrong row size: " + matrix1.dim2 + " vs. " + matrix2.dim2);

			var result = new float[matrix1.dim2];
			int offset1 = i * matrix1.dim2;
			int offset2 = j * matrix2.dim2;
			for (int c = 0; c < matrix1.dim2; c++)
				result[c] = matrix1.data[offset1 + c] - matrix2.data[offset2 + c];

			return result;
		}

		/// <summary>Compute the scalar product of a matrix row with the difference vector of two other matrix rows</summary>
		/// <param name="matrix1">the first matrix</param>
		/// <param name="i">the first row ID</param>
		/// <param name="matrix2">the second matrix</param>
		/// <param name="j">the second row ID</param>
		/// <param name="matrix3">the third matrix</param>
		/// <param name="k">the third row ID</param>///
		/// <returns>see summary</returns>
		static public double RowScalarProductWithRowDifference(Matrix<float> matrix1, int i, Matrix<float> matrix2, int j, Matrix<float> matrix3, int k)
		{
			if (i >= matrix1.dim1)
				throw new ArgumentOutOfRangeException("i too big: " + i + ", dim1 is " + matrix1.dim1);
			if (j >= matrix2.dim1)
				throw new ArgumentOutOfRangeException("j too big: " + j + ", dim1 is " + matrix2.dim1);
			if (k >= matrix3.dim1)
				throw new ArgumentOutOfRangeException("k too big: " + k + ", dim1 is " + matrix3.dim1);

			if (matrix1.dim2 != matrix2.dim2)
				throw new ArgumentException("wrong row size: (1) " + matrix1.dim2 + " vs. (2) " + matrix2.dim2);
			if (matrix1.dim2 != matrix3.dim2)
				throw new ArgumentException("wrong row size: (1) " + matrix1.dim2 + " vs. (3) " + matrix3.dim2);

			double result = 0;
			int offset1 = i * matrix1.dim2;
			int offset2 = j * matrix2.dim2;
			int offset3 = k * matrix3.dim2;
			for (int c = 0; c < matrix1.dim2; c++)
				result += matrix1.data[offset1 + c] * (matrix2.data[offset2 + c] - matrix3.data[offset3 + c]);

			return result;
		}

		/// <summary>return the maximum value contained in a matrix</summary>
		/// <param name='m'>the matrix</param>
		static public int Max(this Matrix<int> m)
		{
			return m.data.Max();
		}

		/// <summary>return the maximum value contained in a matrix</summary>
		/// <param name='m'>the matrix</param>
		static public float Max(this Matrix<float> m)
		{
			return m.data.Max();
		}
	}
}