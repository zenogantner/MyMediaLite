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
using MyMediaLite.IO;
using MyMediaLite.Taxonomy;

namespace MyMediaLiteTest
{
	/// <summary>Class for testing the Pearson class</summary>
	[TestFixture()]
	public class PearsonTest
	{
		[Test()] public void TestCreate()
		{
			// create test objects
			var ratings = new Ratings();
			ratings.Add(0, 1, 0.3);
			ratings.Add(0, 2, 0.6);
			ratings.Add(0, 4, 0.2);
			ratings.Add(1, 3, 0.4);
			ratings.Add(1, 4, 0.2);
			ratings.Add(2, 0, 0.1);
			ratings.Add(2, 1, 0.3);
			// test
			var pearson = Pearson.Create(ratings, EntityType.USER, 0f);
			Assert.AreEqual(0, pearson[0, 1]);
		}

		[Test()] public void TestComputeCorrelation()
		{
			// create test objects	
			var ratings = new Ratings();
			ratings.Add(0, 1, 0.3);
			ratings.Add(0, 4, 0.2);
			ratings.Add(1, 2, 0.6);
			ratings.Add(1, 3, 0.4);
			ratings.Add(1, 4, 0.2);

			// test
			Assert.AreEqual(0, Pearson.ComputeCorrelation(ratings, EntityType.USER, 0, 1, 0));
		}

		[Test()] public void TestComputeCorrelations()
		{
			// create test objects
			var pearson = new Pearson(3);
			var rating_data = new Ratings();
			rating_data.Add(0, 1, 0.3);
			rating_data.Add(0, 2, 0.6);
			rating_data.Add(0, 4, 0.2);
			rating_data.Add(1, 3, 0.4);
			rating_data.Add(1, 4, 0.2);
			rating_data.Add(2, 0, 0.1);
			rating_data.Add(2, 1, 0.3);
			// test
			pearson.shrinkage = 0;
			pearson.ComputeCorrelations(rating_data, EntityType.USER);

			Assert.AreEqual(0, pearson[0, 2]);
		}
		
		[Test()] public void TestComputeCorrelations2()
		{
			// load data from disk
			var user_mapping = new EntityMapping();
			var item_mapping = new EntityMapping();
			var ratings = RatingPrediction.Read("../../../../data/ml100k/u1.base", user_mapping, item_mapping);
						
			Assert.AreEqual(-0.02855815f, Pearson.ComputeCorrelation(ratings, EntityType.ITEM, 45, 311, 200f), 0.00001);			
		}
	}
}