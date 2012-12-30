// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Collections.Generic;
using MyMediaLite.DataType;
using NUnit.Framework;

namespace Tests.DataType
{
	/// <summary>Tests for the MatrixExtensions class</summary>
	[TestFixture()]
	public class MatrixExtensionsTest
	{
		[Test()] public void TestInc()
		{
			var matrix = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			matrix.Inc(3, 4, 2.5);
			Assert.AreEqual(7.5, matrix[3, 4]);

			var matrix1 = new Matrix<float>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix1.SetRow(i, row);
			var matrix2 = new Matrix<float>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix2.SetRow(i, row);
			float[] testrow = { 2, 4, 6, 8, 10 };
			matrix1.Inc(matrix2);
			Assert.AreEqual(testrow, matrix1.GetRow(2));
			
			/*
			var matrix3 = new Matrix<float>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix3.SetRow(i, row);
			matrix3.Inc(1.0);
			for (int j = 0; j < 5; j++)
				Assert.AreEqual(row[j] + 1, matrix3[1, j]);
			 */
			 
			var matrix4 = new Matrix<int>(5, 5);
			int[] int_row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix4.SetRow(i, int_row);
			Assert.AreEqual(matrix4[1, 2], 3);
			matrix4.Inc(1, 2);
			Assert.AreEqual(matrix4[1, 2], 4);
		}

		[Test()] public void TestColumnAverage()
		{
			var matrix = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			Assert.AreEqual(2.0, matrix.ColumnAverage(1));
			Assert.AreEqual(5.0, matrix.ColumnAverage(4));
		}

		[Test()] public void TestMultiply()
		{
			var matrix = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			matrix.Multiply(2.5f);
			float[] testrow = { 2.5f, 5f, 7.5f, 10f, 12.5f };
			Assert.AreEqual(testrow, matrix.GetRow(3));
		}

		[Test()] public void TestFrobeniusNorm()
		{
			var matrix = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			float result = (float) Math.Sqrt(275.0);
			Assert.AreEqual(result, matrix.FrobeniusNorm());
		}

		[Test()] public void TestRowScalarProduct()
		{
			var matrix = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			float[] vector = { 1, 2, 3, 4, 5 };
			Assert.AreEqual(55, MatrixExtensions.RowScalarProduct(matrix, 2, vector));

			var matrix2 = new Matrix<float>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix2.SetRow(i, row);
			Assert.AreEqual(55, MatrixExtensions.RowScalarProduct(matrix, 2, matrix2, 3));
		}

		[Test()] public void TestRowDifference()
		{
			var matrix1 = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix1.SetRow(i, row);
			var matrix2 = new Matrix<float>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix2.SetRow(i, row);

			var result = MatrixExtensions.RowDifference(matrix1, 2, matrix2, 3);
			for (int i = 0; i < 5; i++)
				Assert.AreEqual(0, result[0]);
		}

		[Test()] public void TestScalarProductWithRowDifference()
		{
			var matrix1 = new Matrix<float>(5, 5);
			float[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix1.SetRow(i, row);
			var matrix2 = new Matrix<float>(5, 5);
			for (int i = 0; i < 5; i++)
				matrix2.SetRow(i, row);
			var matrix3 = new Matrix<float>(5, 5);
			MatrixExtensions.Inc(matrix3, 1.0f);

			Assert.AreEqual(40, MatrixExtensions.RowScalarProductWithRowDifference(matrix1, 2, matrix2, 3, matrix3, 1));
		}

		[Test()] public void TestMax()
		{
			var int_matrix = new Matrix<int>(3, 3);
			Assert.AreEqual(0, int_matrix.Max());
			int_matrix[1, 1] = 9;
			Assert.AreEqual(9, MatrixExtensions.Max(int_matrix));

			var float_matrix = new Matrix<float>(3, 3);
			Assert.AreEqual(0, float_matrix.Max());
			float_matrix[1, 1] = 9.0f;
			Assert.AreEqual(9.0, float_matrix.Max());
		}
	}
}