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
using NUnit.Framework;
using MyMediaLite.Data;

namespace MyMediaLiteTest
{
	[TestFixture()]
	public class CombinedRatingsTest
	{
		IRatings CreateRatings1()
		{
			var ratings = new Ratings();
			ratings.Add(1, 4, 0.3);
			ratings.Add(1, 8, 0.2);
			ratings.Add(2, 4, 0.2);
			ratings.Add(2, 2, 0.6);
			ratings.Add(2, 5, 0.4);
			ratings.Add(3, 7, 0.2);
			ratings.Add(6, 3, 0.3);

			return ratings;
		}

		IRatings CreateRatings2()
		{
			var ratings = new Ratings();
			ratings.Add(1, 5, 0.9);
			ratings.Add(7, 9, 0.5);

			return ratings;
		}

		[Test()] public void TestMaxUserIDMaxItemID()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			Assert.AreEqual(7, ratings.MaxUserID);
			Assert.AreEqual(9, ratings.MaxItemID);
		}

		[Test()] public void TestCount()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			Assert.AreEqual(CreateRatings1().Count + CreateRatings2().Count, ratings.Count);
			Assert.AreEqual(9, ratings.Count);
		}

		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestAdd()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			ratings.Add(7, 5, 0.3);
		}

		/*
		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestRemoveAt()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			ratings.RemoveAt(ratings.GetIndex(2, 5));
			Assert.AreEqual(CreateRatings1().Count + CreateRatings2().Count - 1, ratings.Count);
			double r;
			Assert.IsFalse(ratings.TryGet(2, 5, out r));
			
			ratings.RemoveAt(ratings.GetIndex(7, 9));
			Assert.AreEqual(CreateRatings1().Count + CreateRatings2().Count - 2, ratings.Count);
			Assert.IsFalse(ratings.TryGet(7, 9, out r));
		}

		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestRemoveUser()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			ratings.RemoveUser(2);
		}

		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestRemoveItem()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			ratings.RemoveItem(5);
		}
		*/

		[Test()] public void TestGet()
		{
			var ratings = new CombinedRatings(CreateRatings1(), CreateRatings2());

			// test Get
			Assert.AreEqual(0.2, ratings.Get(2, 4));
			Assert.AreEqual(0.3, ratings.Get(6, 3));
			Assert.AreEqual(0.3, ratings.Get(6, 3));
			Assert.AreEqual(0.9, ratings.Get(1, 5));

			// test index[,]
			Assert.AreEqual(0.3, ratings[1, 4]);
			Assert.AreEqual(0.2, ratings[1, 8]);
			Assert.AreEqual(0.6, ratings[2, 2]);
			Assert.AreEqual(0.5, ratings[7, 9]);
		}
	}
}