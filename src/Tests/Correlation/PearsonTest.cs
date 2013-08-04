// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using System.Collections.Generic;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.Taxonomy;

namespace Tests.Correlation
{
	/// <summary>Class for testing the Pearson class</summary>
	[TestFixture()]
	public class PearsonTest
	{
		[Test()] public void TestCreate()
		{
			var interaction_list = new List<IInteraction>();
			interaction_list.Add(new FullInteraction(0, 1, 0.3f, DateTime.Now));
			interaction_list.Add(new FullInteraction(0, 2, 0.6f, DateTime.Now));
			interaction_list.Add(new FullInteraction(0, 4, 0.2f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 3, 0.4f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 4, 0.2f, DateTime.Now));
			interaction_list.Add(new FullInteraction(2, 0, 0.1f, DateTime.Now));
			interaction_list.Add(new FullInteraction(2, 1, 0.3f, DateTime.Now));
			var interactions = new Interactions(interaction_list);

			var correlation_matrix = new Pearson(interactions.MaxUserID + 1, 0f);
			correlation_matrix.ComputeCorrelations(interactions, EntityType.USER);
			Assert.AreEqual(3, correlation_matrix.NumberOfRows);
			Assert.IsTrue(correlation_matrix.IsSymmetric);
			Assert.AreEqual(0, correlation_matrix[0, 1]);
		}

		[Test()] public void TestComputeCorrelation()
		{
			// create test objects
			var interaction_list = new List<IInteraction>();
			interaction_list.Add(new FullInteraction(0, 1, 0.3f, DateTime.Now));
			interaction_list.Add(new FullInteraction(0, 4, 0.2f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 2, 0.6f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 3, 0.4f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 4, 0.2f, DateTime.Now));
			var interactions = new Interactions(interaction_list);

			// test
			var p = new Pearson(interactions.MaxUserID, 0f);
			Assert.AreEqual(0, p.ComputeCorrelation(interactions, EntityType.USER, 0, 1));
		}

		[Test()] public void TestComputeCorrelations()
		{
			// create test objects
			var pearson = new Pearson(3, 0f);
			var interaction_list = new List<IInteraction>();
			interaction_list.Add(new FullInteraction(0, 1, 0.3f, DateTime.Now));
			interaction_list.Add(new FullInteraction(0, 2, 0.6f, DateTime.Now));
			interaction_list.Add(new FullInteraction(0, 4, 0.2f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 3, 0.4f, DateTime.Now));
			interaction_list.Add(new FullInteraction(1, 4, 0.2f, DateTime.Now));
			interaction_list.Add(new FullInteraction(2, 0, 0.1f, DateTime.Now));
			interaction_list.Add(new FullInteraction(2, 1, 0.3f, DateTime.Now));
			var interactions = new Interactions(interaction_list);

			// test
			pearson.Shrinkage = 0;
			pearson.ComputeCorrelations(interactions, EntityType.USER);

			Assert.AreEqual(0, pearson[0, 2]);
		}

		[Test()]
		public void TestComputeCorrelations2()
		{
			// load data from disk
			var interactions = Interactions.FromFile("../../../../data/ml-100k/u1.base", new Mapping(), new Mapping());
			var p = new Pearson(interactions.MaxItemID + 1, 200f);
			Assert.AreEqual(-0.02788301f, p.ComputeCorrelation(interactions, EntityType.ITEM, 45, 311), 0.00001);
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
			var corr_matrix = new Pearson(0, 0f);
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
			var matrix = new Pearson(3, 0f);

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
			var corr_matrix = new Pearson(0, 0f);
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
			var matrix = new Pearson(3, 0f);
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
			var matrix = new Pearson(3, 0f);
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
			IList<int> cor_entities_list = matrix.GetPositivelyCorrelatedEntities(2, new int[] { 0, 1, 2 }, 3);
			int[] cor_entities = new int[1];
			cor_entities_list.CopyTo(cor_entities, 0);
			int[] pos_cor_entities = { 1 };
			Assert.AreEqual(pos_cor_entities, cor_entities);
		}

		[Test()]
		public void TestGetNearestNeighbors()
		{
			// create a test CorrelationMatrix
			var matrix = new Pearson(3, 0f);
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