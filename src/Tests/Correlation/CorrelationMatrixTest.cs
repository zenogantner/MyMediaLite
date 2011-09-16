// Copyright(C) 2010 Christina Lichtenthäler
// Copyright(C) 2011 Zeno Gantner
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
using System.Globalization;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLiteTest.Correlation
{
	/// <summary>Class for testing the CorrelationMatrix class</summary>
	[TestFixture()]
	public class CorrelationMatrixTest
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
			var corr_matrix = CorrelationMatrix.ReadCorrelationMatrix(reader);
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
			var matrix = new CorrelationMatrix(3);
			float[] row1 = { 1f, 0.1f, 0.2f };
			float[] row2 = { 0.1f, 1f, 0.3f };
			float[] row3 = { 0.2f, 0.3f, 1f };

			matrix.SetRow(0, row1);
			matrix.SetRow(1, row2);
			matrix.SetRow(2, row3);

			// test
			string filename = "testCorrelationMatrixWriter.txt";
			var writer = new StreamWriter(filename);
			matrix.Write(writer);
			writer.Close();

			var reader1 = new StreamReader(filename);
			Assert.AreEqual("3",       reader1.ReadLine().Trim());
			Assert.AreEqual("0 1 0.1", reader1.ReadLine().Trim());
			Assert.AreEqual("0 2 0.2", reader1.ReadLine().Trim());
			Assert.AreEqual("1 2 0.3", reader1.ReadLine().Trim());

			var reader2 = new StreamReader(filename);
			var corr_matrix = CorrelationMatrix.ReadCorrelationMatrix(reader2);

			Assert.AreEqual(1f,   corr_matrix[0, 0]);
			Assert.AreEqual(0.1f, corr_matrix[0, 1]);
			Assert.AreEqual(0.2f, corr_matrix[0, 2]);
			Assert.AreEqual(0.1f, corr_matrix[1, 0]);
			Assert.AreEqual(1f,   corr_matrix[1, 1]);
			Assert.AreEqual(0.3f, corr_matrix[1, 2]);
			Assert.AreEqual(0.2f, corr_matrix[2, 0]);
			Assert.AreEqual(0.3f, corr_matrix[2, 1]);
			Assert.AreEqual(1f,   corr_matrix[2, 2]);
			// close streams and delete the text file
			reader1.Close();
			reader2.Close();
			//File.Delete(filename);
		}

		[Test()]
		public void TestAddEntity()
		{
			// create a test CorrelationMatrix
			var matrix = new CorrelationMatrix(4);
			float[] row1 = { 0.1f, 0.4f, 0.2f, 0.3f };
			float[] row2 = { 0.3f, 0.1f, 0.6f, 0.7f };
			float[] row3 = { 0.2f, 0.6f, 0.3f, 0.5f };
			float[] row4 = { 0.4f, 0.2f, 0.5f, 0.1f };

			matrix.SetRow(0, row1);
			matrix.SetRow(1, row2);
			matrix.SetRow(2, row3);
			matrix.SetRow(3, row4);

			// test
			matrix.AddEntity(4);
			Assert.AreEqual(5, matrix.dim1);
		}

		[Test()]
		public void TestSumUp()
		{
			// create a test CorrelationMatrix
			var matrix = new CorrelationMatrix(4);
			float[] row1 = { 0.1f, 0.4f, 0.2f, 0.3f };
			float[] row2 = { 0.3f, 0.1f, 0.6f, 0.7f };
			float[] row3 = { 0.2f, 0.6f, 0.3f, 0.5f };
			float[] row4 = { 0.4f, 0.2f, 0.5f, 0.1f };

			matrix.SetRow(0, row1);
			matrix.SetRow(1, row2);
			matrix.SetRow(2, row3);
			matrix.SetRow(3, row4);

			// test
			matrix.AddEntity(4);
			Assert.AreEqual(5, matrix.dim1);
		}

		[Test()]
		public void TestGetPositivelyCorrelatedEntities()
		{
			// create a test CorrelationMatrix
			var matrix = new CorrelationMatrix(4);
			float[] row1 = { 0.1f, 0.4f, 0.2f, 0.3f };
			float[] row2 = { 0.3f, 0.1f, 0.6f, 0.7f };
			float[] row3 = { 0.2f, 0.6f, 0.3f, 0.5f };
			float[] row4 = { 0.4f, 0.2f, 0.5f, 0.1f };

			matrix.SetRow(0, row1);
			matrix.SetRow(1, row2);
			matrix.SetRow(2, row3);
			matrix.SetRow(3, row4);

			Assert.AreEqual(0.1f, matrix[0, 0]);
			Assert.AreEqual(0.5f, matrix[3, 2]);

			// test
			IList<int> cor_entities_list = matrix.GetPositivelyCorrelatedEntities(2);
			int[] cor_entities = new int[5];
			cor_entities_list.CopyTo(cor_entities, 0);
			int[] pos_cor_entities = { 1, 3, 0, 0, 0 };
			Assert.AreEqual(pos_cor_entities, cor_entities);
		}

		[Test()]
		public void TestGetNearestNeighbors()
		{
			// create a test CorrelationMatrix
			var matrix = new CorrelationMatrix(4);
			float[] row1 = { 0.1f, 0.4f, 0.2f, 0.3f };
			float[] row2 = { 0.3f, 0.1f, 0.6f, 0.7f };
			float[] row3 = { 0.2f, 0.6f, 0.3f, 0.5f };
			float[] row4 = { 0.4f, 0.2f, 0.5f, 0.1f };

			matrix.SetRow(0, row1);
			matrix.SetRow(1, row2);
			matrix.SetRow(2, row3);
			matrix.SetRow(3, row4);

			// test
			int[] nn_test = matrix.GetNearestNeighbors(2, 2);
			int[] nn_sol = { 1, 3 };
			Assert.AreEqual(nn_sol, nn_test);
		}
	}
}