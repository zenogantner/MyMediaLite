// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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
using NUnit.Framework;
using System;
using System.IO;
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

		[Test()]
		public void TestReadCorrelationMatrix()
		{
			// create test object
			const string filename = "correlation_matrix.txt";
			var writer = new StreamWriter(filename);
			writer.WriteLine(3);
			writer.WriteLine("0 1 0.1");
			writer.WriteLine("0 2 0.2");
			writer.WriteLine("1 2 0.3");
			writer.Close();

			var reader = new StreamReader(filename);
			var corr_matrix = new BinaryCosine(0);
			corr_matrix.ReadSymmetricCorrelationMatrix(reader);
			Assert.AreEqual(1f,   corr_matrix[0, 0]);
			Assert.AreEqual(1f,   corr_matrix[1, 1]);
			Assert.AreEqual(1f,   corr_matrix[2, 2]);

			Assert.AreEqual(0.1f, corr_matrix[0, 1]);
			Assert.AreEqual(0.1f, corr_matrix[1, 0]);

			Assert.AreEqual(0.2f, corr_matrix[0, 2]);
			Assert.AreEqual(0.2f, corr_matrix[2, 0]);

			Assert.AreEqual(0.3f, corr_matrix[1, 2]);
			Assert.AreEqual(0.3f, corr_matrix[2, 1]);


			// TODO test Exception
			// test with wrong format

			// close streams an delete the text file
			reader.Close();
			//File.Delete(filename);
		}

		[Test()]
		public void TestWrite()
		{
			// create a test CorrelationMatrix
			var matrix = new BinaryCosine(3);

			matrix[0, 0] = 1f;
			matrix[0, 1] = 0.1f;
			matrix[0, 2] = 0.2f;
			matrix[1, 0] = 0.1f;
			matrix[1, 1] = 1f;
			matrix[1, 2] = 0.3f;
			matrix[2, 0] = 0.2f;
			matrix[2, 1] = 0.3f;
			matrix[2, 2] = 1f;

			// test
			string filename = "testCorrelationMatrixWriter.txt";
			var writer = new StreamWriter(filename);
			matrix.Write(writer);
			writer.Close();

			// check file format
			var reader1 = new StreamReader(filename);
			Assert.AreEqual("3",       reader1.ReadLine().Trim());
			Assert.AreEqual("0 1 0.1", reader1.ReadLine().Trim());
			Assert.AreEqual("0 2 0.2", reader1.ReadLine().Trim());
			Assert.AreEqual("1 2 0.3", reader1.ReadLine().Trim());
			reader1.Close();

			// check result of reading in the file
			var reader2 = new StreamReader(filename);
			var corr_matrix = new BinaryCosine(0);
			corr_matrix.ReadSymmetricCorrelationMatrix(reader2);

			Assert.AreEqual(1f,   corr_matrix[0, 0]);
			Assert.AreEqual(0.1f, corr_matrix[0, 1]);
			Assert.AreEqual(0.2f, corr_matrix[0, 2]);
			Assert.AreEqual(0.1f, corr_matrix[1, 0]);
			Assert.AreEqual(1f,   corr_matrix[1, 1]);
			Assert.AreEqual(0.3f, corr_matrix[1, 2]);
			Assert.AreEqual(0.2f, corr_matrix[2, 0]);
			Assert.AreEqual(0.3f, corr_matrix[2, 1]);
			Assert.AreEqual(1f,   corr_matrix[2, 2]);
			reader2.Close();

			// clean up
			File.Delete(filename);
		}

		[Test()]
		public void TestSumUp()
		{
			// create a test CorrelationMatrix
			var matrix = new BinaryCosine(3);
			matrix[0, 0] = 1f;
			matrix[0, 1] = 0.1f;
			matrix[0, 2] = 0.2f;
			matrix[1, 0] = 0.1f;
			matrix[1, 1] = 1f;
			matrix[1, 2] = 0.3f;
			matrix[2, 0] = 0.2f;
			matrix[2, 1] = 0.3f;
			matrix[2, 2] = 1f;

			// test
			Assert.AreEqual(0.3f, matrix.SumUp(0, new int[] {1, 2}), 0.00001);
		}

		[Test()]
		public void TestGetPositivelyCorrelatedEntities()
		{
			// create a test CorrelationMatrix
			var matrix = new BinaryCosine(3);
			matrix[0, 0] = 1f;
			matrix[0, 1] = 0.1f;
			matrix[0, 2] = -0.2f;
			matrix[1, 0] = 0.1f;
			matrix[1, 1] = 1f;
			matrix[1, 2] = 0.3f;
			matrix[2, 0] = -0.2f;
			matrix[2, 1] = 0.3f;
			matrix[2, 2] = 1f;

			Assert.AreEqual(1f, matrix[0, 0]);
			Assert.AreEqual(0.3f, matrix[1, 2]);

			// test
			IList<int> cor_entities_list = matrix.GetPositivelyCorrelatedEntities(2);
			int[] cor_entities = new int[1];
			cor_entities_list.CopyTo(cor_entities, 0);
			int[] pos_cor_entities = { 1 };
			Assert.AreEqual(pos_cor_entities, cor_entities);
		}

		[Test()]
		public void TestGetNearestNeighbors()
		{
			// create a test CorrelationMatrix
			var matrix = new BinaryCosine(3);
			matrix[0, 0] = 1f;
			matrix[0, 1] = 0.1f;
			matrix[0, 2] = 0.2f;
			matrix[1, 0] = 0.1f;
			matrix[1, 1] = 1f;
			matrix[1, 2] = 0.3f;
			matrix[2, 0] = 0.2f;
			matrix[2, 1] = 0.3f;
			matrix[2, 2] = 1f;

			// test
			IList<int> nn_test = matrix.GetNearestNeighbors(2, 2);
			IList<int> nn_sol = new int[]{ 1, 0 };
			Assert.AreEqual(nn_sol, nn_test);
		}

	}
}