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
using System.Globalization;
using System.IO;
using MyMediaLite.correlation;
using MyMediaLite.data_type;
using MyMediaLite.taxonomy;
using MyMediaLite.data;


namespace MyMediaLite
{
	/// <summary>Class for testing the correlation classes with NUnit</summary>
	[TestFixture()]
	public class CorrelationTest
	{

		/// <summary>Unit test of CorrelationMatrix.ReadCorrelationMatrix(StreamReader reader)</summary>
		[Test()]
		public void CorrelationMatrixReadCorrelationMatrix()
		{
			// create test object
			const string filename = "correlation_matrix.txt";
			var writer = new StreamWriter(filename);
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';
			writer.WriteLine(3);
			writer.WriteLine("0 1 0.1");
			writer.WriteLine("0 2 0.2");
			writer.WriteLine("1 2 0.3");
			writer.Close();

			var reader = new StreamReader(filename);
			var corr_matrix = CorrelationMatrix.ReadCorrelationMatrix(reader);
			//test textfile:
			//3
			//0 1 0.1
			//0 2 0.2
			//1 2 0.3
			Assert.AreEqual(1f,   corr_matrix[0, 0]);
			Assert.AreEqual(0.1f, corr_matrix[0, 1]);
			Assert.AreEqual(0.2f, corr_matrix[0, 2]);
			Assert.AreEqual(0.1f, corr_matrix[1, 0]);
			Assert.AreEqual(1f,   corr_matrix[1, 1]);
			Assert.AreEqual(0.3f, corr_matrix[1, 2]);
			Assert.AreEqual(0.2f, corr_matrix[2, 0]);
			Assert.AreEqual(0.3f, corr_matrix[2, 1]);
			Assert.AreEqual(1f,   corr_matrix[2, 2]);

			// TODO test Exception
			// test with wrong format

			// close streams an delete the text file
			reader.Close();
			File.Delete(filename);
		}

		/// <summary>Unit test of CorrelationMatrix.Write(StreamWriter writer)</summary>
		[Test()]
		public void CorrelationMatrixWrite()
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
			File.Delete(filename);
		}

		/// <summary>Unit test of CorrelationMatrix.AddEntity(int entity_id)</summary>
		[Test()]
		public void CorrelationMatrixAddEntity()
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

		/// <summary>Unit test of CorrelationMatrix.SumUp()</summary>
		[Test()]
		public void CorrelationMatrixSumUp()
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

		/// <summary>Unit test of CorrelationMatrix.GetPositivelyCorrelatedEntities(int entity_id)</summary>
		[Test()]
		public void CorrelationMatrixGetPositivelyCorrelatedEntities()
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

		/// <summary>Unit test of CorrelationMatrix.GetNearestNeighbors(int entity_id, uint k)</summary>
		[Test()]
		public void CorrelationMatrixGetNearestNeighbors()
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

		/// <summary>Unit test of Cosine.CorrelationMatrix Create(SparseBooleanMatrix vectors)</summary>
		[Test()]
		public void CosineCreate()
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
			var correlation_matrix = Cosine.Create(sparse_boolean_matrix);
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(correlation_matrix[0, 1], 4));
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(correlation_matrix[1, 0], 4));
			Assert.AreEqual(Math.Round(1 / 3d, 4), Math.Round(correlation_matrix[1, 3], 4));
		}

		/// <summary>Unit test of Cosine.ComputeCorrelations(SparseBooleanMatrix entity_data)</summary>
		[Test()]
		public void CosineComputeCorrelations()
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
			var cosine = new Cosine(5);
			cosine.ComputeCorrelations(sparse_boolean_matrix);
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(cosine[0, 1], 4));
			Assert.AreEqual(Math.Round(1 / Math.Sqrt(6), 4), Math.Round(cosine[1, 0], 4));
			Assert.AreEqual(Math.Round(1 / 3d, 4), Math.Round(cosine[1, 3], 4));
		}

		/// <summary>Unit test of Cosine.ComputeCorrelation(HashSet<int> vector_i, HashSet<int> vector_j)</summary>
		[Test()]
		public void CosineComputeCorrelation()
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
			Assert.AreEqual(Math.Round(1 / 3d, 4), Math.Round(Cosine.ComputeCorrelation(vector1, vector2), 4));
		}

		/// <summary>Unit test of Pearson.Create(RatingData ratings, EntityType entity_type, float shrinkage)</summary>
		[Test()]
		public void PearsonCreate()
		{
			// create test objects
			var rating_data = new RatingData();
			rating_data.AddRating(new RatingEvent(0, 1, 0.3));
			rating_data.AddRating(new RatingEvent(0, 2, 0.6));
			rating_data.AddRating(new RatingEvent(0, 4, 0.2));
			rating_data.AddRating(new RatingEvent(1, 3, 0.4));
			rating_data.AddRating(new RatingEvent(1, 4, 0.2));
			rating_data.AddRating(new RatingEvent(2, 0, 0.1));
			rating_data.AddRating(new RatingEvent(2, 1, 0.3));
			// test
			var pearson = Pearson.Create(rating_data, EntityType.USER, 0f);
			Assert.AreEqual(0, pearson[0, 1]);
		}

		/// <summary>
		/// Unit test of Pearson.ComputeCorrelation(Ratings ratings_1, Ratings ratings_2, EntityType entity_type, int i, int j, float shrinkage)
		/// </summary>
		[Test()]
		public void PearsonComputeCorrelation()
		{
			// create test objects
			var rating1 = new Ratings();
			var rating2 = new Ratings();
			rating1.AddRating(new RatingEvent(0, 1, 0.3));
			rating1.AddRating(new RatingEvent(0, 4, 0.2));
			rating2.AddRating(new RatingEvent(1, 2, 0.6));
			rating2.AddRating(new RatingEvent(1, 3, 0.4));
			rating2.AddRating(new RatingEvent(1, 4, 0.2));

			// test
			Assert.AreEqual(0, Pearson.ComputeCorrelation(rating1, rating2, EntityType.USER, 0, 1, 0));
		}

		/// <summary>Unit test of Pearson.ComputeCorrelations(RatingData ratings, EntityType entity_type)</summary>
		[Test()]
		public void PearsonComputeCorrelations()
		{
			// create test objects
			var pearson = new Pearson(3);
			var rating_data = new RatingData();
			rating_data.AddRating(new RatingEvent(0, 1, 0.3));
			rating_data.AddRating(new RatingEvent(0, 2, 0.6));
			rating_data.AddRating(new RatingEvent(0, 4, 0.2));
			rating_data.AddRating(new RatingEvent(1, 3, 0.4));
			rating_data.AddRating(new RatingEvent(1, 4, 0.2));
			rating_data.AddRating(new RatingEvent(2, 0, 0.1));
			rating_data.AddRating(new RatingEvent(2, 1, 0.3));
			// test
			pearson.shrinkage = 0;
			pearson.ComputeCorrelations(rating_data, EntityType.USER);

			Assert.AreEqual(0, pearson[0, 2]);
		}
	}
}