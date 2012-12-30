// Copyright (C) 2011, 2012 Zeno Gantner
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
	/// <summary>Tests for the SymmetricMatrix<T> class</summary>
	[TestFixture()]
	public class SymmetricMatrixTest
	{
		[Test()] public void TestSymmetricity()
		{
			var matrix = new SymmetricMatrix<float>(5);
			matrix[1, 3] = 1.0f;

			Assert.AreEqual(1.0, matrix[3, 1]);
		}

		[Test()] public void TestIsSymmetric()
		{
			var matrix = new SymmetricMatrix<float>(5);
			Assert.IsTrue(matrix.IsSymmetric);

			matrix[1, 3] = 1.0f;
			Assert.IsTrue(matrix.IsSymmetric);
		}

		[Test()] public void TestNumberOfRows()
		{
			var matrix = new SymmetricMatrix<float>(3);
			Assert.AreEqual(3, matrix.NumberOfRows);
		}

		[Test()] public void TestNumberOfColumns()
		{
			var matrix = new SymmetricMatrix<float>(3);
			Assert.AreEqual(3, matrix.NumberOfColumns);
		}

		[Test()] public void TestCreateMatrix()
		{
			var matrix1 = new SymmetricMatrix<float>(5);
			var matrix2 = matrix1.CreateMatrix(4, 4);
			Assert.IsInstanceOfType(matrix1.GetType(), matrix2);
		}
	}
}