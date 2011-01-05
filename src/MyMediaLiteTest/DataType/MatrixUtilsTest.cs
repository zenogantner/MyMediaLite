// Copyright (C) 2010 Tina Lichtenthäler, Zeno Gantner
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
using System.Collections;
using System.Collections.Generic;
using MyMediaLite.DataType;
using NUnit.Framework;


namespace MyMediaLiteTest
{
	/// <summary>Testing the MatrixUtils class</summary>
	[TestFixture()]
	public class MatrixUtilsTest
	{
		/// <summary>Unit test of MatrixUtils.Inc(Matrix&lt;double&gt; matrix, int i, int j, double v)</summary>
		[Test()] public void Inc()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			MatrixUtils.Inc(matrix, 3, 4, 2.5);
			Assert.AreEqual(7.5, matrix[3, 4]);
		}

		/// <summary>Unit test of MatrixUtils.Inc(Matrix&lt;double&gt; matrix1, Matrix&lt;double&gt; matrix2)</summary>
		[Test()] public void Inc2()
		{
			var matrix1 = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix1.SetRow(i, row);

			var matrix2 = new Matrix<double>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix2.SetRow(i, row);
			double[] testrow = {2, 4, 6, 8, 10};
			MatrixUtils.Inc(matrix1, matrix2);
			Assert.AreEqual(testrow, matrix1.GetRow(2));
		}

		/// <summary>Unit test of MatrixUtils.ColumnAverage(Matrix&lt;double&gt; matrix, int col)</summary>
		[Test()] public void ColumnAverage()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			Assert.AreEqual(2.0, MatrixUtils.ColumnAverage(matrix, 1));
			Assert.AreEqual(5.0, MatrixUtils.ColumnAverage(matrix, 4));
		}

		/// <summary>Unit test of MatrixUtils.RowAverage(Matrix&lt;double&gt; matrix, int row)</summary>
		[Test()] public void RowAverage()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			Assert.AreEqual(3.0, MatrixUtils.RowAverage(matrix, 1));
			Assert.AreEqual(3.0, MatrixUtils.RowAverage(matrix, 4));
		}

		/// <summary>Unit test of MatrixUtils.Multiply(Matrix&lt;double&gt; matrix, double d)</summary>
		[Test()] public void Multiply()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i<5; i++)
				matrix.SetRow(i, row);
			MatrixUtils.Multiply(matrix, 2.5);
			double[] testrow = { 2.5, 5, 7.5, 10, 12.5 };
			Assert.AreEqual(testrow, matrix.GetRow(3));
		}

		/// <summary>Unit test of MatrixUtils.FrobeniusNorm(Matrix&lt;double&gt; matrix)</summary>
		[Test()] public void FrobeniusNorm()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			double result =Math.Sqrt(275.0);
			Assert.AreEqual(result,MatrixUtils.FrobeniusNorm(matrix));
		}

		/// <summary>Unit test of MatrixUtils.RowScalarProduct(Matrix&lt;double&gt; matrix, int i, double[] vector)</summary>
		[Test()] public void RowScalarProduct()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			double[] vector = { 1, 2, 3, 4, 5 };
			double result = 55;
			Assert.AreEqual(result, MatrixUtils.RowScalarProduct(matrix, 2, vector));
		}

		/// <summary>Unit test of MatrixUtils.ContainsNaN(Matrix&lt;double&gt; matrix)</summary>
		[Test()] public void ContainsNaN()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			var matrixtrue = new Matrix<double>(5, 5);
			double[] row2 = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrixtrue.SetRow(i, row2);

			Assert.IsFalse(MatrixUtils.ContainsNaN(matrix));
			// TODO insert Nan in matrix
			// Assert.IsTrue(MatrixUtils.ContainsNaN(matrixtrue));
		}
	}
}