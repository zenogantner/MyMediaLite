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
using MyMediaLite.data;
using NUnit.Framework;


namespace MyMediaLite
{
	/// <summary>Testing the data classes</summary>
	[TestFixture()]
	public class data_test
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
			RatingData testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			testRatingData.AddRating(new RatingEvent(2, 5, 0.4));
			testRatingData.AddRating(new RatingEvent(3, 7, 0.2));
			testRatingData.AddRating(new RatingEvent(6, 3, 0.3));

			Assert.AreEqual(6, testRatingData.MaxUserID);
			Assert.AreEqual(8, testRatingData.MaxItemID);
		}

		/// <summary>
		/// Unit test of RatingData.AddRating(RatingEvent rating)
		/// </summary>
		[Test()]
		public void RatingDataAddRating()
		{
			RatingData testRatingData = new RatingData();
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
			RatingData testRatingData = new RatingData();
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
			RatingData testRatingData = new RatingData();
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
			RatingData testRatingData = new RatingData();
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
		/// Unit Test of RatingData.ChangeRating(RatingEvent rating, double new_rating)
		/// </summary>
		[Test()]
		public void RatingDataChangeRating()
		{
			RatingData testRatingData = new RatingData();
			testRatingData.AddRating(new RatingEvent(1, 4, 0.3));
			testRatingData.AddRating(new RatingEvent(1, 8, 0.2));
			RatingEvent changeRating = new RatingEvent(2, 4, 0.2);
			testRatingData.AddRating(changeRating);
			testRatingData.AddRating(new RatingEvent(2, 2, 0.6));
			testRatingData.AddRating(new RatingEvent(2, 5, 0.4));
			testRatingData.AddRating(new RatingEvent(3, 4, 0.2));
			testRatingData.AddRating(new RatingEvent(3, 3, 0.3));

			Assert.AreEqual(0.2, testRatingData.All.FindRating(2, 4).rating);
			testRatingData.ChangeRating(changeRating, 0.4);
			Assert.AreEqual(0.4, testRatingData.All.FindRating(2, 4).rating);
		}

		/// <summary>
		/// Unit Test of RatingData.FindRating(int user_id, int item_id)
		/// </summary>
		[Test()]
		public void RatingDataFindRating()
		{
			RatingData testRatingData = new RatingData();
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
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(1, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(2, 2, 0.4));
			testRatings.AddRating(new RatingEvent(2, 5, 0.5));

			testRatings.Shuffle ();
			// at least one rating must change his position
			Assert.IsTrue (testRatings[0].rating != 0.1 || testRatings[1].rating != 0.2 || testRatings[2].rating != 0.3 || testRatings[3].rating != 0.4 || testRatings[4].rating != 0.5);

		}

		/// <summary>
		/// Unit test of Ratings.Count
		/// </summary>
		[Test()]
		public void RatingsCount()
		{
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(1, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(2, 2, 0.4));
			testRatings.AddRating(new RatingEvent(2, 5, 0.5));

			Assert.AreEqual(5, testRatings.Count);
		}

		/// <summary>
		/// Unit test of Ratings.Average
		/// </summary>
		[Test()]
		public void RatingsAverage()
		{
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(1, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(2, 2, 0.4));
			testRatings.AddRating(new RatingEvent(2, 5, 0.5));

			Assert.AreEqual(0.3, testRatings.Average);
		}

		/// <summary>
		/// Unit test of Ratings.ChangeRating(RatingEvent rating, double new_rating)
		/// </summary>
		[Test()]
		public void RatingsChangeRating()
		{
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(1, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(2, 2, 0.4));
			RatingEvent changeRating = new RatingEvent(2, 5, 0.5);
			testRatings.AddRating(changeRating);

			Assert.AreEqual(0.5, testRatings.FindRating(2, 5).rating);
			testRatings.ChangeRating(changeRating, 0.2);
			Assert.AreEqual(0.2, testRatings.FindRating(2, 5).rating);
		}

		/// <summary>
		/// Unit test of Ratings.RemoveRating(RatingEvent rating)
		/// </summary>
		[Test()]
		public void RatingsRemoveRating()
		{
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(1, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(2, 2, 0.4));
			RatingEvent removeRating = new RatingEvent(2, 5, 0.5);
			testRatings.AddRating(removeRating);

			testRatings.RemoveRating(removeRating);
			Assert.IsNull(testRatings.FindRating(2, 5));
		}

		/// <summary>
		/// Unit Test of Ratings.GetUsers()
		/// </summary>
		[Test()]
		public void RatingsGetUsers()
		{
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(5, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 5, 0.5));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(4, 2, 0.4));

			HashSet<int> users = testRatings.GetUsers ();
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
			Ratings testRatings = new Ratings();
			testRatings.AddRating(new RatingEvent(1, 4, 0.1));
			testRatings.AddRating(new RatingEvent(5, 8, 0.2));
			testRatings.AddRating(new RatingEvent(2, 4, 0.3));
			testRatings.AddRating(new RatingEvent(4, 2, 0.4));
			testRatings.AddRating(new RatingEvent(2, 5, 0.5));

			HashSet<int> items = testRatings.GetItems();
			int[] itemsTest = new int[5];
			items.CopyTo (itemsTest);
			Assert.AreEqual(itemsTest[0], 4);
			Assert.AreEqual(itemsTest[1], 8);
			Assert.AreEqual(itemsTest[2], 2);
			Assert.AreEqual(itemsTest[3], 5);
		}
	}
}

