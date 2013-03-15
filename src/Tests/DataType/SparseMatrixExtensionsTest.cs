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
using MyMediaLite.DataType;
using NUnit.Framework;

namespace Tests.DataType
{
	/// <summary>Testing the SparseMatrixExtensions class</summary>
	[TestFixture()]
	public class SparseMatrixExtensionsTest
	{
		[Test()] public void TestMax()
		{
			var int_matrix = new SparseMatrix<int>(3, 3);
			Assert.AreEqual(0, int_matrix.Max());
			int_matrix[1, 1] = 9;
			Assert.AreEqual(9, int_matrix.Max());

			var float_matrix = new SparseMatrix<float>(3, 3);
			Assert.AreEqual(0, float_matrix.Max());
			float_matrix[1, 1] = 9.0f;
			Assert.AreEqual(9.0f, float_matrix.Max());
		}

		[Test()] public void TestFrobeniusNorm()
		{
			var float_matrix = new SparseMatrix<float>(5, 5);
			Assert.AreEqual(0, float_matrix.FrobeniusNorm());
			float_matrix[1, 1] = 5;
			Assert.AreEqual(Math.Sqrt(25), float_matrix.FrobeniusNorm());
		}
	}
}