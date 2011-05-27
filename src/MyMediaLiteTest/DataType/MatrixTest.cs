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
	/// <summary>Testing the Matrix class</summary>
	[TestFixture()]
	public class MatrixTest
	{
		[Test()] public void TestGetSetRow()
		{
			var matrix = new Matrix<int>(5, 5);
			int[] row = { 1, 2, 3, 4, 5 };
			matrix.SetRow(3, row);
			Assert.AreEqual(row, matrix.GetRow(3));
			Assert.AreEqual(0, matrix[0, 0]);
			Assert.AreEqual(1, matrix[3, 0]);
		}

		[Test()] public void TestGetSetColumn()
		{
			var matrix = new Matrix<int>(5, 5);
			int[] column = { 1, 2, 3, 4, 5 };
			matrix.SetColumn(3, column);
			Assert.AreEqual(column, matrix.GetColumn(3));
			Assert.AreEqual(0, matrix[0, 0]);
			Assert.AreEqual(1, matrix[0, 3]);
		}

		[Test()] public void TestInit()
		{
			var matrix = new Matrix<int>(5, 5);
			int[] row = { 2, 2, 2, 2, 2 };
			matrix.Init(2);
			Assert.AreEqual(row, matrix.GetRow(2));
		}

		[Test()] public void TestSetRowToOneValue()
		{
			var matrix = new Matrix<int>(5, 5);
			int[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			matrix.SetRowToOneValue(3, 10);
			int[] testrow = { 10, 10, 10, 10, 10 };
			Assert.AreEqual(testrow, matrix.GetRow(3));
		}

		[Test()] public void TestSetColumnToOneValue()
		{
			var matrix = new Matrix<int>(5, 5);
			int[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				matrix.SetRow(i, row);
			matrix.SetColumnToOneValue(3, 10);
			int[] testcolumn = { 10, 10, 10, 10, 10 };
			Assert.AreEqual(testcolumn, matrix.GetColumn(3));
		}

		[Test()] public void TestTranspose()
		{
			var matrix = new Matrix<int>(5, 6);
			matrix[1, 1] = 3;
			matrix[1, 3] = 4;
			matrix[3, 1] = 5;

			var transpose = matrix.Transpose();
			Assert.AreEqual(6, transpose.NumberOfRows);
			Assert.AreEqual(5, transpose.NumberOfColumns);
			Assert.AreEqual(3, transpose[1, 1]);
			Assert.AreEqual(4, transpose[3, 1]);
			Assert.AreEqual(5, transpose[1, 3]);

			// check whether it is really a copy
			transpose[1, 1] = 0;
			Assert.AreEqual(3, matrix[1, 1]);
		}
	}
}