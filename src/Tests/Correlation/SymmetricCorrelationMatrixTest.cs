// Copyright(C) 2011, 2012 Zeno Gantner
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
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace Tests.Correlation
{
	/// <summary>Class for testing the CorrelationMatrix class</summary>
	[TestFixture()]
	public class SymmetricCorrelationMatrixTest
	{
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
			var corr_matrix = new SymmetricCorrelationMatrix(0);
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
			var matrix = new SymmetricCorrelationMatrix(3);

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
			var corr_matrix = new SymmetricCorrelationMatrix(0);
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
		public void TestAddEntity()
		{
			// create a test CorrelationMatrix
			var matrix = new SymmetricCorrelationMatrix(3);

			matrix[0, 0] = 1f;
			matrix[0, 2] = 0.2f;
			matrix[1, 1] = 1f;
			matrix[1, 2] = 0.3f;
			matrix[2, 1] = 0.3f;
			matrix[2, 2] = 1f;

			// test
			Assert.AreEqual(3, matrix.NumberOfRows);
			matrix.AddEntity(3);
			Assert.AreEqual(4, matrix.NumberOfRows);
		}

		[Test()]
		public void TestSumUp()
		{
			// create a test CorrelationMatrix
			var matrix = new SymmetricCorrelationMatrix(3);
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
			var matrix = new SymmetricCorrelationMatrix(3);
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
			var matrix = new SymmetricCorrelationMatrix(3);
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