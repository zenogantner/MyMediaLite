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
		[Test()] public void TestInc()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			MatrixUtils.Inc(matrix, 3, 4, 2.5);
			Assert.AreEqual(7.5, matrix[3, 4]);
		}

		[Test()] public void TestInc2()
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

		[Test()] public void TestColumnAverage()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			Assert.AreEqual(2.0, MatrixUtils.ColumnAverage(matrix, 1));
			Assert.AreEqual(5.0, MatrixUtils.ColumnAverage(matrix, 4));
		}

		[Test()] public void TestRowAverage()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			Assert.AreEqual(3.0, MatrixUtils.RowAverage(matrix, 1));
			Assert.AreEqual(3.0, MatrixUtils.RowAverage(matrix, 4));
		}

		[Test()] public void TestMultiply()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i<5; i++)
				matrix.SetRow(i, row);
			MatrixUtils.Multiply(matrix, 2.5);
			double[] testrow = { 2.5, 5, 7.5, 10, 12.5 };
			Assert.AreEqual(testrow, matrix.GetRow(3));
		}

		[Test()] public void TestFrobeniusNorm()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			double result =Math.Sqrt(275.0);
			Assert.AreEqual(result,MatrixUtils.FrobeniusNorm(matrix));
		}

		[Test()] public void TestRowScalarProduct()
		{
			var matrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			double[] vector = { 1, 2, 3, 4, 5 };
			double result = 55;
			Assert.AreEqual(result, MatrixUtils.RowScalarProduct(matrix, 2, vector));
		}
	}
}