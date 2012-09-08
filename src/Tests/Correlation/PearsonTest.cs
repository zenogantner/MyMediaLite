// Copyright (C) 2011, 2012 Zeno Gantner
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
			var ratings = new Ratings();
			ratings.Add(0, 1, 0.3f);
			ratings.Add(0, 2, 0.6f);
			ratings.Add(0, 4, 0.2f);
			ratings.Add(1, 3, 0.4f);
			ratings.Add(1, 4, 0.2f);
			ratings.Add(2, 0, 0.1f);
			ratings.Add(2, 1, 0.3f);

			var correlation_matrix = new Pearson(ratings.MaxUserID + 1, 0f);
			correlation_matrix.ComputeCorrelations(ratings, EntityType.USER);
			Assert.AreEqual(3, correlation_matrix.NumberOfRows);
			Assert.IsTrue(correlation_matrix.IsSymmetric);
			Assert.AreEqual(0, correlation_matrix[0, 1]);
		}

		[Test()] public void TestComputeCorrelation()
		{
			// create test objects
			var ratings = new Ratings();
			ratings.Add(0, 1, 0.3f);
			ratings.Add(0, 4, 0.2f);
			ratings.Add(1, 2, 0.6f);
			ratings.Add(1, 3, 0.4f);
			ratings.Add(1, 4, 0.2f);

			// test
			var p = new Pearson(ratings.AllUsers.Count, 0f);
			Assert.AreEqual(0, p.ComputeCorrelation(ratings, EntityType.USER, 0, 1));
		}

		[Test()] public void TestComputeCorrelations()
		{
			// create test objects
			var pearson = new Pearson(3, 0f);
			var rating_data = new Ratings();
			rating_data.Add(0, 1, 0.3f);
			rating_data.Add(0, 2, 0.6f);
			rating_data.Add(0, 4, 0.2f);
			rating_data.Add(1, 3, 0.4f);
			rating_data.Add(1, 4, 0.2f);
			rating_data.Add(2, 0, 0.1f);
			rating_data.Add(2, 1, 0.3f);
			// test
			pearson.Shrinkage = 0;
			pearson.ComputeCorrelations(rating_data, EntityType.USER);

			Assert.AreEqual(0, pearson[0, 2]);
		}

		[Test()] public void TestComputeCorrelations2()
		{
			// load data from disk
			var user_mapping = new Mapping();
			var item_mapping = new Mapping();
			var ratings = RatingData.Read("../../../../data/ml-100k/u1.base", user_mapping, item_mapping);
			
			var p = new Pearson(ratings.AllUsers.Count, 200f);
			Assert.AreEqual(-0.02788301f, p.ComputeCorrelation(ratings, EntityType.ITEM, 45, 311), 0.00001);
		}
	}
}