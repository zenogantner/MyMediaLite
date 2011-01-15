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

namespace MyMediaLiteTest
{
	/// <summary>Tests for the SparseMatrix class</summary>
	[TestFixture()]
	public class SparseMatrixTest
	{
		public void TestIsSymmetric()
		{
			var matrix1 = new SparseMatrix<double>(3, 5);
			Assert.IsFalse(matrix1.IsSymmetric);
			
			var matrix2 = new SparseMatrix<double>(5, 5);
			Assert.IsTrue(matrix2.IsSymmetric);
			
			matrix2[1, 3] = 1.0;
			Assert.IsFalse(matrix2.IsSymmetric);
			
			matrix2[3, 1] = 1.0;
			Assert.IsTrue(matrix2.IsSymmetric);
		}
		
		public void TestNumberOfRows()
		{
			var matrix = new SparseMatrix<double>(3, 5);
			Assert.AreEqual(3, matrix.NumberOfRows);
		}
		
		public void TestNumberOfColumns()
		{
			var matrix = new SparseMatrix<double>(3, 5);
			Assert.AreEqual(5, matrix.NumberOfColumns);
		}
		
		public void TestCreateMatrix()
		{
			var matrix1 = new SparseMatrix<double>(3, 5);
			var matrix2 = matrix1.CreateMatrix(4, 4);
			Assert.IsInstanceOfType(matrix1.GetType(), matrix2);
		}
		
		public void TestNonEmptyRows()
		{
			var matrix  = new SparseMatrix<double>(3, 5);
			Assert.AreEqual(0, matrix .NonEmptyRows.Count);
			
			matrix [3, 1] = 1.0;
			Assert.AreEqual(1, matrix.NonEmptyRows.Count);
			Assert.AreEqual(3, matrix .NonEmptyRows[0].Key);
		}
		
		public void TestNonEmptyEntryIDs()
		{
			var matrix = new SparseMatrix<double>(3, 5);
			Assert.AreEqual(0, matrix.NonEmptyEntryIDs.Count);
			
			matrix[3, 1] = 1.0;
			Assert.AreEqual(1, matrix.NonEmptyEntryIDs.Count);
			foreach (var pair in matrix.NonEmptyEntryIDs)
			{
				Assert.AreEqual(3, pair.First);
				Assert.AreEqual(1, pair.Second);
			}
		}
	}
}