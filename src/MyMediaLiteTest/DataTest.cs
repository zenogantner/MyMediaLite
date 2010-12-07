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

using System;
using System.Collections.Generic;
using MyMediaLite.Data;
using NUnit.Framework;


namespace MyMediaLiteTest
{
	/// <summary>Testing the data classes</summary>
	[TestFixture()]
	public class DataTest
	{
		/// <summary>
		/// Unit test of  EntityMapping.ToOriginalID(int internal_id)
		/// </summary>
		[Test()]
		public void ToOriginalID()
		{
			// TODO
		}

		/// <summary>
		/// Unit Test of RatingData.MaxUserID and RatingData.MaxItemID
		/// </summary>
		public void RatingDataMaxUserIDIemUserID()
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

		/// <summary>
		/// Unit test of RatingData.AddRating(RatingEvent rating)
		/// </summary>
		[Test()]
		public void RatingDataAddRating()
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

		//AddUser and AddItem could not be tested

		/// <summary>
		/// Unit test of RatingData.RemoveRating(RatingEvent rating)
		/// </summary>
		[Test()]
		public void RatingDataRemoveRating()
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

		/// <summary>
		/// Unit Test of RatingData.RemoveUser(int user_id)
		/// </summary>
		[Test()]
		public void RatingDataRemoveUser()
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
			testRatingData.RemoveUser (2);
			Assert.IsNull(testRatingData.All.FindRating(2, 5));
		}

		/// <summary>
		/// Unit test of RatingData.RemoveItem(int item_id)
		/// </summary>
		[Test()]
		public void RatingDataRemoveItem()
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
			testRatingData.RemoveItem (4);
			Assert.IsNull(testRatingData.All.FindRating(2, 4));
		}

		/// <summary>
		/// Unit Test of RatingData.FindRating(int user_id, int item_id)
		/// </summary>
		[Test()]
		public void RatingDataFindRating()
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

		/// <summary>
		/// Unit Test of Ratings.Shuffle()
		/// </summary>
		[Test()]
		public void RatingsShuffle()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			test_ratings.Shuffle ();
			// at least one rating must change his position
			Assert.IsTrue (test_ratings[0].rating != 0.1 || test_ratings[1].rating != 0.2 || test_ratings[2].rating != 0.3 || test_ratings[3].rating != 0.4 || test_ratings[4].rating != 0.5);

		}

		/// <summary>
		/// Unit test of Ratings.Average
		/// </summary>
		[Test()]
		public void RatingsAverage()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			Assert.AreEqual(0.3f, test_ratings.Average, 0.0001f);
		}

		/// <summary>
		/// Unit test of Ratings.Count
		/// </summary>
		[Test()]
		public void RatingsCount()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(1, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(2, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			Assert.AreEqual(5, test_ratings.Count);
		}

		/// <summary>
		/// Unit test of Ratings.RemoveRating(RatingEvent rating)
		/// </summary>
		[Test()]
		public void RatingsRemoveRating()
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

		/// <summary>
		/// Unit Test of Ratings.GetUsers()
		/// </summary>
		[Test()]
		public void RatingsGetUsers()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(5, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(4, 2, 0.4));

			HashSet<int> users = test_ratings.GetUsers ();
			int[] usersTest = new int[5];
			users.CopyTo (usersTest);
			Assert.AreEqual(usersTest[0], 1);
			Assert.AreEqual(usersTest[1], 5);
			Assert.AreEqual(usersTest[2], 2);
			Assert.AreEqual(usersTest[3], 4);
		}

		/// <summary>
		/// Unit Test of Ratings.GetItems()
		/// </summary>
		[Test()]
		public void RatingsGetItems()
		{
			var test_ratings = new Ratings();
			test_ratings.AddRating(new RatingEvent(1, 4, 0.1));
			test_ratings.AddRating(new RatingEvent(5, 8, 0.2));
			test_ratings.AddRating(new RatingEvent(2, 4, 0.3));
			test_ratings.AddRating(new RatingEvent(4, 2, 0.4));
			test_ratings.AddRating(new RatingEvent(2, 5, 0.5));

			HashSet<int> items = test_ratings.GetItems();
			int[] itemsTest = new int[5];
			items.CopyTo (itemsTest);
			Assert.AreEqual(itemsTest[0], 4);
			Assert.AreEqual(itemsTest[1], 8);
			Assert.AreEqual(itemsTest[2], 2);
			Assert.AreEqual(itemsTest[3], 5);
		}
	}
}

