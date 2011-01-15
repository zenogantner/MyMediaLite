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
	/// <summary>Testing the Ratings class</summary>
	[TestFixture()]
	public class RatingsTest
	{
		[Test()] public void TestShuffle()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			test_ratings.Shuffle();
			// at least one rating must change his position
			Assert.IsTrue(test_ratings[0].rating != 0.1 || test_ratings[1].rating != 0.2 || test_ratings[2].rating != 0.3 || test_ratings[3].rating != 0.4 || test_ratings[4].rating != 0.5);
			// TODO this is a wrong assumption!
		}

		[Test()] public void TestAverage()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			Assert.AreEqual(0.3f, test_ratings.Average, 0.0001f);
		}

		[Test()] public void TestCount()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			Assert.AreEqual(5, test_ratings.Count);
		}

		[Test()] public void TestRemoveRating()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			RatingEvent removeRating = new RatingEvent(2, 5, 0.5);
			test_ratings.AddRating(removeRating);

			test_ratings.RemoveRating(removeRating);
			Assert.IsNull(test_ratings.FindRating(2, 5));
		}

		[Test()] public void TestGetUsers()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(5, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(4, 2, 0.4));

			List<int> user_list = test_ratings.GetUsers().ToList();
			Assert.Contains(1, user_list);
			Assert.Contains(2, user_list);
			Assert.Contains(4, user_list);
			Assert.Contains(5, user_list);
		}

		[Test()] public void TestGetItems()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(5, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(4, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			List<int> item_list = test_ratings.GetItems().ToList();
			Assert.Contains(2, item_list);
			Assert.Contains(4, item_list);
			Assert.Contains(5, item_list);
			Assert.Contains(8, item_list);
		}
	}
}