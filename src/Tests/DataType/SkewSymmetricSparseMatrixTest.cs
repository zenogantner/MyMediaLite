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
using MyMediaLite.DataType;
using NUnit.Framework;

namespace Tests.DataType
{
	/// <summary>Tests for the SkewSymmetricSparseMatrix class</summary>
	[TestFixture()]
	public class SkewSymmetricSparseMatrixTest
	{
		[Test()] public void TestGetSet()
		{
			var matrix = new SkewSymmetricSparseMatrix(5);

			matrix[1, 3] = 1.0f;
			Assert.AreEqual( 1.0f, matrix[1, 3]);
			Assert.AreEqual(-1.0f, matrix[3, 1]);

			matrix[4, 1] = -2.0f;
			Assert.AreEqual(-2.0f, matrix[4, 1]);
			Assert.AreEqual( 2.0f, matrix[1, 4]);
		}

		[Test()] public void TestAntiSymmetricity()
		{
			var matrix = new SkewSymmetricSparseMatrix(5);
			matrix[1, 3] = 1.0f;

			Assert.AreEqual(-1.0f, matrix[3, 1]);
		}

		[Test()] public void TestIsSymmetric()
		{
			var matrix = new SkewSymmetricSparseMatrix(5);
			Assert.IsTrue(matrix.IsSymmetric);

			matrix[1, 3] = 1.0f;
			Assert.IsFalse(matrix.IsSymmetric);

			matrix[1, 3] = 0f;
			Assert.IsTrue(matrix.IsSymmetric);
		}

		[Test()] public void TestNumberOfRows()
		{
			var matrix = new SkewSymmetricSparseMatrix(3);
			Assert.AreEqual(3, matrix.NumberOfRows);
		}

		[Test()] public void TestNumberOfColumns()
		{
			var matrix = new SkewSymmetricSparseMatrix(3);
			Assert.AreEqual(3, matrix.NumberOfColumns);
		}

		[Test()] public void TestCreateMatrix()
		{
			var matrix1 = new SkewSymmetricSparseMatrix(5);
			var matrix2 = matrix1.CreateMatrix(4, 4);
			Assert.IsInstanceOfType(matrix1.GetType(), matrix2);
		}

		[Test()] public void TestNonEmptyEntryIDs()
		{
			var matrix = new SkewSymmetricSparseMatrix(5);
			Assert.AreEqual(0, matrix.NonEmptyEntryIDs.Count);

			matrix[2, 2] = 0.1f;
			Assert.AreEqual(1, matrix.NonEmptyEntryIDs.Count);

			matrix[3, 1] = 1.0f;
			Assert.AreEqual(3, matrix.NonEmptyEntryIDs.Count);
		}

		[Test()] public void TestNumberOfNonEmptyEntries()
		{
			var matrix = new SkewSymmetricSparseMatrix(5);
			Assert.AreEqual(0, matrix.NumberOfNonEmptyEntries);

			matrix[3, 1] = 1.0f;
			Assert.AreEqual(2, matrix.NumberOfNonEmptyEntries);

			matrix[3, 1] = 2.0f;
			Assert.AreEqual(2, matrix.NumberOfNonEmptyEntries);

			matrix[3, 3] = 1.0f;
			Assert.AreEqual(3, matrix.NumberOfNonEmptyEntries);
		}
	}
}