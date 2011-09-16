// Copyright (C) 2010 Tina Lichtenthäler, Zeno Gantner
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
using System.Collections;
using System.Collections.Generic;
using MyMediaLite.DataType;
using NUnit.Framework;

namespace MyMediaLiteTest.DataType
{
	/// <summary>Tests for the SparseBooleanMatrixStatic class</summary>
	[TestFixture()]
	public class SparseBooleanMatrixStaticTest
	{
		[Test()] public void TestCreateMatrix()
		{
			var matrix = new SparseBooleanMatrixStatic();

			var other_matrix = matrix.CreateMatrix(2, 2);
			Assert.IsInstanceOfType(matrix.GetType(), other_matrix);
		}

		[Test()] public void TestGetEntriesByColumn()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.AreEqual(0, matrix.GetEntriesByColumn(0).Count);
			Assert.AreEqual(3, matrix.GetEntriesByColumn(1).Count);
			Assert.AreEqual(0, matrix.GetEntriesByColumn(2).Count);
			Assert.AreEqual(0, matrix.GetEntriesByColumn(3).Count);
			Assert.AreEqual(3, matrix.GetEntriesByColumn(4).Count);
		}

		[Test()] public void TestGetEntriesByRow()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.AreEqual(2, matrix.GetEntriesByRow(0).Count);
			Assert.AreEqual(2, matrix.GetEntriesByRow(1).Count);
			Assert.AreEqual(0, matrix.GetEntriesByRow(2).Count);
			Assert.AreEqual(0, matrix.GetEntriesByRow(3).Count);
			Assert.AreEqual(2, matrix.GetEntriesByRow(4).Count);
		}

		[Test()] public void TestIsSymmetric()
		{
			var matrix = new SparseBooleanMatrixStatic();
			Assert.IsTrue(matrix.IsSymmetric);

			var row0  = new int[0];
			var row1  = new int[] { 1 };
			var row12 = new int[] { 1, 2 };

			matrix[1] = row1;
			Assert.IsTrue(matrix.IsSymmetric);

			matrix[2] = row1;
			Assert.IsFalse(matrix.IsSymmetric);

			matrix[1] = row12;
			Assert.IsTrue(matrix.IsSymmetric);

			matrix[2] = row0;
			Assert.IsFalse(matrix.IsSymmetric);
		}

		[Test()] public void TestNonEmptyColumnIDs()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.AreEqual(2, matrix.NonEmptyColumnIDs.Count);
		}

		[Test()] public void TestNonEmptyRowIDs()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.AreEqual(3, matrix.NonEmptyRowIDs.Count);

			var rowIDs = matrix.NonEmptyRowIDs;
			var rowIDsEnum = rowIDs.GetEnumerator();
			rowIDsEnum.MoveNext();
			Assert.AreEqual(0, rowIDsEnum.Current);
			rowIDsEnum.MoveNext();
			Assert.AreEqual(1, rowIDsEnum.Current);
			rowIDsEnum.MoveNext();
			Assert.AreEqual(4, rowIDsEnum.Current);
			Assert.IsFalse(rowIDsEnum.MoveNext());
		}

		[Test()] public void TestNonEmptyRows()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.IsTrue(matrix[0, 1]);
			var nonEmptyRows = matrix.NonEmptyRows;
			Assert.AreEqual(3, nonEmptyRows.Count);
		}

		[Test()] public void TestNumberOfEntries()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;
			Assert.AreEqual(6, matrix.NumberOfEntries);
		}

		[Test()] public void TestNumberOfColumns()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;
			Assert.AreEqual(5, matrix.NumberOfColumns);
		}

		[Test()] public void TestNumberOfRows()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;
			Assert.AreEqual(5, matrix.NumberOfRows);
		}

		[Test()] public void TestNumEntriesByColumn()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.AreEqual(0, matrix.NumEntriesByColumn(0));
			Assert.AreEqual(3, matrix.NumEntriesByColumn(1));
			Assert.AreEqual(0, matrix.NumEntriesByColumn(2));
			Assert.AreEqual(0, matrix.NumEntriesByColumn(3));
			Assert.AreEqual(3, matrix.NumEntriesByColumn(4));
		}

		[Test()] public void TestNumEntriesByRow()
		{
			var matrix = new SparseBooleanMatrixStatic();
			var row = new int[] { 1, 4 };
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
					matrix[i] = row;

			Assert.AreEqual(2, matrix.NumEntriesByRow(0));
			Assert.AreEqual(2, matrix.NumEntriesByRow(1));
			Assert.AreEqual(0, matrix.NumEntriesByRow(2));
			Assert.AreEqual(0, matrix.NumEntriesByRow(3));
			Assert.AreEqual(2, matrix.NumEntriesByRow(4));
		}

		[Test()] public void TestOverlapCount()
		{
			var row25 = new int[] { 2, 5 };
			var row36 = new int[] { 3, 6 };
			var row15 = new int[] { 1, 5 };
			var matrix = new SparseBooleanMatrixStatic();
			matrix[2] = row25;
			matrix[4] = row36;
			matrix[5] = row15;

			var overlapMatrix = new SparseBooleanMatrix();
			overlapMatrix[2, 1] = true;
			overlapMatrix[2, 5] = true; // same entry
			overlapMatrix[4, 4] = true;
			overlapMatrix[4, 6] = true; // same entry
			overlapMatrix[5, 2] = true;
			overlapMatrix[5, 5] = true; // same entry

			Assert.AreEqual(3, matrix.Overlap(overlapMatrix));
		}

		[Test()] public void TestTranspose()
		{
			// contruct the matrix
			var matrix = new SparseBooleanMatrixStatic();
			var row14 = new int[] { 1, 4 };
			for (int i = 0; i < 7; i++)
				if (i != 2 && i != 4)
					matrix[i] = row14;
			var row25 = new int[] { 2, 5 };
			var row3 = new int[] { 3 };
			matrix[2] = row25;
			matrix[4] = row3;

			// transpose the matrix
			var transposed_matrix = (IBooleanMatrix) matrix.Transpose();
			// test the transposed matrix
			Assert.IsTrue(transposed_matrix[1,0]);
			Assert.IsTrue(transposed_matrix[4, 6]);
			Assert.IsFalse(transposed_matrix[3, 1]);
			Assert.IsFalse(transposed_matrix[5, 4]);
		}
	}
}