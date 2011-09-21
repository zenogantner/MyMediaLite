// Copyright(C) 2010 Christina Lichtenthäler
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
using NUnit.Framework;
using System.Collections.Generic;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;

namespace Tests.Correlation
{
	/// <summary>Class for testing the BinaryCosine class</summary>
	[TestFixture()]
	public class BinaryCosineTest
	{
		[Test()] public void TestCreate()
		{
			// create test objects
			var sparse_boolean_matrix = new SparseBooleanMatrix();
			sparse_boolean_matrix[0, 1] = true;
			sparse_boolean_matrix[0, 4] = true;
			sparse_boolean_matrix[1, 0] = true;
			sparse_boolean_matrix[1, 2] = true;
			sparse_boolean_matrix[1, 4] = true;
			sparse_boolean_matrix[3, 1] = true;
			sparse_boolean_matrix[3, 3] = true;
			sparse_boolean_matrix[3, 4] = true;
			// test
			var correlation_matrix = BinaryCosine.Create(sparse_boolean_matrix);
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(correlation_matrix[0, 1], 4));
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(correlation_matrix[1, 0], 4));
			Assert.AreEqual(Math.Round(1 / 3d, 4), Math.Round(correlation_matrix[1, 3], 4));
		}

		[Test()] public void TestComputeCorrelations()
		{
			// create test objects
			var sparse_boolean_matrix = new SparseBooleanMatrix();
			sparse_boolean_matrix[0, 1] = true;
			sparse_boolean_matrix[0, 4] = true;
			sparse_boolean_matrix[1, 0] = true;
			sparse_boolean_matrix[1, 2] = true;
			sparse_boolean_matrix[1, 4] = true;
			sparse_boolean_matrix[3, 1] = true;
			sparse_boolean_matrix[3, 3] = true;
			sparse_boolean_matrix[3, 4] = true;
			// test
			var cosine = new BinaryCosine(5);
			cosine.ComputeCorrelations(sparse_boolean_matrix);
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(cosine[0, 1], 4));
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(cosine[1, 0], 4));
			Assert.AreEqual(Math.Round(1 / 3d, 4), Math.Round(cosine[1, 3], 4));
		}

		[Test()] public void TestComputeCorrelation()
		{
			// create test objects
			var vector1 = new HashSet<int>();
			vector1.Add(0);
			vector1.Add(2);
			vector1.Add(4);
			var vector2 = new HashSet<int>();
			vector2.Add(1);
			vector2.Add(3);
			vector2.Add(4);
			// test
			Assert.AreEqual(Math.Round(1 / 3d, 4), Math.Round(BinaryCosine.ComputeCorrelation(vector1, vector2), 4));
		}
	}
}