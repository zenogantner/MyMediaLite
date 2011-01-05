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
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;


namespace MyMediaLiteTest
{
	/// <summary>Class for testing the Pearson class</summary>
	[TestFixture()]
	public class PearsonTest
	{
		/// <summary>Unit test of Pearson.Create(RatingData ratings, EntityType entity_type, float shrinkage)</summary>
		[Test()]
		public void TestCreate()
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
		public void TestComputeCorrelation()
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
		public void TestComputeCorrelations()
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