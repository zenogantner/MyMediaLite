// Copyright (C) 2010 Christina Lichtenthäler
// Copyright (C) 2011 Zeno Gantner
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
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Data;
using NUnit.Framework;

namespace MyMediaLiteTest
{
	/// <summary>Testing the data classes</summary>
	[TestFixture()]
	public class RatingDataTest
	{
		[Test()] public void TestMaxUserIDItemID()
		{
			var rating_data = new RatingData();
			rating_data.AddRating(new RatingEvent(1, 4, 0.3));
			rating_data.AddRating(new RatingEvent(1, 8, 0.2));
			rating_data.AddRating(new RatingEvent(2, 4, 0.2));
			rating_data.AddRating(new RatingEvent(2, 2, 0.6));
			rating_data.AddRating(new RatingEvent(2, 5, 0.4));
			rating_data.AddRating(new RatingEvent(3, 7, 0.2));
			rating_data.AddRating(new RatingEvent(6, 3, 0.3));

			Assert.AreEqual(6, rating_data.MaxUserID);
			Assert.AreEqual(8, rating_data.MaxItemID);
		}

		[Test()] public void TestAddRating()
		{
			var testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			testRatingData.AddRating(new RatingEvent(2, 5, 0.4));
			testRatingData.AddRating(new RatingEvent(3, 7, 0.2));
			testRatingData.AddRating(new RatingEvent(6, 3, 0.3));

			Assert.AreEqual(0.4, testRatingData.All.FindRating(2, 5).rating);
			Assert.AreEqual(0.3, testRatingData.All.FindRating(1, 4).rating);
			Assert.AreEqual(0.3, testRatingData.All.FindRating(6, 3).rating);
		}

		[Test()] public void TestRemoveRating()
		{
			var testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			RatingEvent removeEvent = new RatingEvent(2, 5, 0.4);
			testRatingData.AddRating(removeEvent);
			testRatingData.AddRating(new RatingEvent(3, 7, 0.2));
			testRatingData.AddRating(new RatingEvent(3, 3, 0.3));
			RatingEvent removeEvent2 = new RatingEvent(6, 3, 0.3);
			testRatingData.AddRating(removeEvent2);

			Assert.AreEqual(0.4, testRatingData.All.FindRating(2, 5).rating);
			testRatingData.RemoveRating(removeEvent);
			testRatingData.RemoveRating(removeEvent2);
			Assert.IsNull(testRatingData.All.FindRating(2, 5));
			Assert.IsNull(testRatingData.All.FindRating(6, 3));
		}

		[Test()] public void TestRemoveUser()
		{
			var testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			testRatingData.AddRating(new RatingEvent(2, 5, 0.4));
			testRatingData.AddRating(new RatingEvent(3, 7, 0.2));
			testRatingData.AddRating(new RatingEvent(3, 3, 0.3));

			Assert.AreEqual(0.4, testRatingData.All.FindRating(2, 5).rating);
			testRatingData.RemoveUser(2);
			Assert.IsNull(testRatingData.All.FindRating(2, 5));
		}

		[Test()] public void TestRemoveItem()
		{
			var testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			testRatingData.AddRating(new RatingEvent(2, 5, 0.4));
			testRatingData.AddRating(new RatingEvent(3, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(3, 3, 0.3));

			Assert.AreEqual(0.2, testRatingData.All.FindRating(2, 4).rating);
			testRatingData.RemoveItem(4);
			Assert.IsNull(testRatingData.All.FindRating(2, 4));
		}

		[Test()] public void TestFindRating()
		{
			var testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			testRatingData.AddRating(new RatingEvent(2, 5, 0.4));
			testRatingData.AddRating(new RatingEvent(3, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(3, 3, 0.3));
			testRatingData.AddRating(new RatingEvent(6, 3, 0.3));

			Assert.AreEqual(0.2, testRatingData.All.FindRating(2, 4).rating);
			Assert.AreEqual(0.3, testRatingData.All.FindRating(3, 3).rating);
			Assert.AreEqual(0.3, testRatingData.All.FindRating(6, 3).rating);
		}
	}
}