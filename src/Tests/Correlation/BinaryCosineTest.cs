// Copyright (C) 2010, 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Christina Lichtenthäler
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
		static readonly float DELTA = 0.0001f;
		
		[Test()] public void TestCreate()
		{
			var sparse_boolean_matrix = new SparseBooleanMatrix();
			sparse_boolean_matrix[0, 1] = true;
			sparse_boolean_matrix[0, 4] = true;
			sparse_boolean_matrix[1, 0] = true;
			sparse_boolean_matrix[1, 2] = true;
			sparse_boolean_matrix[1, 4] = true;
			sparse_boolean_matrix[3, 1] = true;
			sparse_boolean_matrix[3, 3] = true;
			sparse_boolean_matrix[3, 4] = true;

			var correlation_matrix = new BinaryCosine(sparse_boolean_matrix.NumberOfRows);
			correlation_matrix.ComputeCorrelations(sparse_boolean_matrix);
			
			Assert.AreEqual(4, correlation_matrix.NumberOfRows);
			Assert.IsTrue(correlation_matrix.IsSymmetric);
			
			Assert.AreEqual(1 / Math.Sqrt(6), correlation_matrix[0, 1], DELTA);
			Assert.AreEqual(1 / Math.Sqrt(6), correlation_matrix[1, 0], DELTA);
			Assert.AreEqual(1 / 3d, correlation_matrix[1, 3], DELTA);

			Assert.AreEqual(0f, correlation_matrix[2, 0]);
			Assert.AreEqual(0f, correlation_matrix[2, 1]);
			Assert.AreEqual(1f, correlation_matrix[2, 2]);
			Assert.AreEqual(0f, correlation_matrix[2, 3]);

			Assert.AreEqual(0f, correlation_matrix[0, 2]);
			Assert.AreEqual(0f, correlation_matrix[1, 2]);
			Assert.AreEqual(0f, correlation_matrix[3, 2]);
		}

		[Test()] public void TestComputeCorrelations()
		{
			var sparse_boolean_matrix = new SparseBooleanMatrix();
			sparse_boolean_matrix[0, 1] = true;
			sparse_boolean_matrix[0, 4] = true;
			sparse_boolean_matrix[1, 0] = true;
			sparse_boolean_matrix[1, 2] = true;
			sparse_boolean_matrix[1, 4] = true;
			sparse_boolean_matrix[3, 1] = true;
			sparse_boolean_matrix[3, 3] = true;
			sparse_boolean_matrix[3, 4] = true;

			var correlation = new BinaryCosine(4);
			correlation.ComputeCorrelations(sparse_boolean_matrix);
			Assert.AreEqual(1 / Math.Sqrt(6), correlation[0, 1], DELTA);
			Assert.AreEqual(1 / Math.Sqrt(6), correlation[1, 0], DELTA);
			Assert.AreEqual(1 / 3d, correlation[1, 3], DELTA);
		}

		[Test()] public void TestComputeCorrelation()
		{
			var vector1 = new HashSet<int>();
			vector1.Add(0);
			vector1.Add(2);
			vector1.Add(4);
			var vector2 = new HashSet<int>();
			vector2.Add(1);
			vector2.Add(3);
			vector2.Add(4);
			
			var cosine = new BinaryCosine(4);
			
			Assert.AreEqual(1 / 3f, cosine.ComputeCorrelation(vector1, vector2), DELTA);
			Assert.AreEqual(0f, cosine.ComputeCorrelation(vector1, new HashSet<int>()), DELTA);
		}
	}
}