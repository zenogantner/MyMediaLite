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
	/// <summary>Testing the SparseBooleanMatrix class</summary>
	[TestFixture()]
	public class SparseBooleanMatrixTest
	{
		[Test()] public void TestNonEmptyRows()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2)
				{
					matrix[i, 1]= true;
					matrix[i, 4]= true;
				}
			Assert.IsTrue(matrix[0, 1]);
			IList<KeyValuePair<int, HashSet<int>>> nonEmptyRows = matrix.NonEmptyRows;
			Assert.AreEqual(4, nonEmptyRows.Count);

			// TODO test contents
		}

		[Test()] public void TestNonEmptyRowIDs()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
				{
					matrix[i, 1]= true;
					matrix[i, 4]= true;
				}
			ICollection<int> rowIDs = matrix.NonEmptyRowIDs;
			IEnumerator <int> rowIDsEnum = rowIDs.GetEnumerator();
			rowIDsEnum.MoveNext();
			Assert.AreEqual(0, rowIDsEnum.Current);
			rowIDsEnum.MoveNext();
			Assert.AreEqual(1, rowIDsEnum.Current);
			rowIDsEnum.MoveNext();
			rowIDsEnum.MoveNext();
			Assert.AreEqual(4, rowIDsEnum.Current);
			Assert.IsFalse(rowIDsEnum.MoveNext());
		}

		[Test()] public void TestNumberOfRows()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
			{
				matrix[i, 1]= true;
				matrix[i, 4]= true;
			}
			Assert.AreEqual(5, matrix.NumberOfRows);
		}

		[Test()] public void TestNumberOfColumns()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
			{
				matrix[i, 1]= true;
				matrix[i, 4]= true;
			}
			Assert.AreEqual(5, matrix.NumberOfColumns);
		}

		[Test()] public void TestNumberOfEntries()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2 && i != 4)
				{
					matrix[i, 1]= true;
					matrix[i, 4]= false;
				}
			Assert.AreEqual(3, matrix.NumberOfEntries);
		}

		[Test()] public void TestRemoveColumn()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2 && i != 4)
				{
					matrix[i, 1]= true;
					matrix[i, 4]= true;
				}
			matrix[2, 2] = true;

			matrix.RemoveColumn(2);

			Assert.IsTrue(matrix[0, 3]);
			Assert.IsTrue(matrix[1, 3]);
			Assert.IsTrue(matrix[3, 3]);
			Assert.IsTrue(matrix[1, 1]);
		}

		[Test()] public void TestRemoveColumns()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 7; i++)
				if(i != 2 && i != 4)
				{
					matrix[i, 1]= true;
					matrix[i, 4]= true;
				}
			matrix[2, 2] = true;
			matrix[2, 5] = true;
			matrix[4, 3] = true;
			int[] delete_columns = {2, 4};
			matrix.RemoveColumn(delete_columns);

			// test the new columns
			Assert.IsTrue(matrix[4, 2]);
			Assert.IsTrue(matrix[2, 3]);
			Assert.IsFalse(matrix[1, 3]);
			Assert.IsFalse(matrix[4, 3]);
		}

		[Test()] public void TestTranspose()
		{
			var matrix = new SparseBooleanMatrix();
			for (int i = 0; i < 7; i++)
				if(i != 2 && i != 4)
				{
					matrix[i, 1]= true;
					matrix[i, 4]= true;
				}
			matrix[2, 2] = true;
			matrix[2, 5] = true;
			matrix[4, 3] = true;
			// transpose the matrix
			var transposedMatrix = (SparseBooleanMatrix) matrix.Transpose();
			// test the transposed matrix
			Assert.IsTrue(transposedMatrix[1,0]);
			Assert.IsTrue(transposedMatrix[4, 6]);
			Assert.IsFalse(transposedMatrix[3, 1]);
			Assert.IsFalse(transposedMatrix[5, 4]);
		}

		[Test()] public void TestOverlap()
		{
			var matrix = new SparseBooleanMatrix();
			matrix[2, 2] = true;
			matrix[2, 5] = true;
			matrix[4, 3] = true;
			matrix[4, 6] = true;
			matrix[5, 1] = true;
			matrix[5, 5] = true;

			var overlapMatrix = new SparseBooleanMatrix();
			overlapMatrix[2, 1] = true;
			overlapMatrix[2, 5] = true; // same entry
			overlapMatrix[4, 4] = true;
			overlapMatrix[4, 6] = true; // same entry
			overlapMatrix[5, 2] = true;
			overlapMatrix[5, 5] = true; // same entry

			Assert.AreEqual(3, matrix.Overlap(overlapMatrix));

		}
	}
}