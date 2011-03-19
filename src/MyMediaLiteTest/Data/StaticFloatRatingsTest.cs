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
	[TestFixture()]
	public class StaticFloatRatingsTest
	{
		[Test()]
		[ExpectedException(typeof(Exception))]
		public void TestFull()
		{
			var ratings = new StaticFloatRatings(2);
			Assert.AreEqual(0, ratings.Count);
			ratings.Add(1, 4, 0.3);
			Assert.AreEqual(1, ratings.Count);
			ratings.Add(1, 8, 0.2);
			Assert.AreEqual(2, ratings.Count);
			ratings.Add(2, 4, 0.2);
		}

		[Test()] public void TestMaxUserIDMaxItemID()
		{
			var ratings = new StaticFloatRatings(7);
			ratings.Add(1, 4, 0.3);
			ratings.Add(1, 8, 0.2);
			ratings.Add(2, 4, 0.2);
			ratings.Add(2, 2, 0.6);
			ratings.Add(2, 5, 0.4);
			ratings.Add(3, 7, 0.2);
			ratings.Add(6, 3, 0.3);

			Assert.AreEqual(6, ratings.MaxUserID);
			Assert.AreEqual(8, ratings.MaxItemID);
		}

		[Test()] public void TestAddRating()
		{
			var ratings = new StaticFloatRatings(7);
			ratings.Add(1, 4, 0.3f);
			Assert.AreEqual(1, ratings.Count);
			ratings.Add(1, 8, 0.2f);
			Assert.AreEqual(2, ratings.Count);
			ratings.Add(2, 4, 0.2f);
			ratings.Add(2, 2, 0.6f);
			ratings.Add(2, 5, 0.4f);
			ratings.Add(3, 7, 0.2f);
			ratings.Add(6, 3, 0.3f);

			Assert.AreEqual(0.4f, ratings.Get(2, 5));
			Assert.AreEqual(0.3f, ratings.Get(1, 4));
			Assert.AreEqual(0.3f, ratings.Get(6, 3));
			Assert.AreEqual(7, ratings.Count);
		}

		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestRemoveAt()
		{
			var ratings = new StaticFloatRatings(8);
			ratings.Add(1, 4, 0.3);
			ratings.Add(1, 8, 0.2);
			ratings.Add(2, 4, 0.2);
			ratings.Add(2, 2, 0.6);
			ratings.Add(2, 5, 0.4);
			ratings.Add(3, 7, 0.2);
			ratings.Add(3, 3, 0.3);
			ratings.Add(6, 3, 0.3);

			Assert.AreEqual(8, ratings.Count);
			ratings.RemoveAt(ratings.GetIndex(2, 5));
		}

		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestRemoveUser()
		{
			var ratings = new StaticFloatRatings(7);
			ratings.Add(1, 4, 0.3);
			ratings.Add(1, 8, 0.2);
			ratings.Add(2, 4, 0.2);
			ratings.Add(2, 2, 0.6);
			ratings.Add(2, 5, 0.4);
			ratings.Add(3, 7, 0.2);
			ratings.Add(3, 3, 0.3);

			Assert.AreEqual(7, ratings.Count);
			ratings.RemoveUser(2);
		}

		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void TestRemoveItem()
		{
			var ratings = new StaticFloatRatings(7);
			ratings.Add(1, 4, 0.3);
			ratings.Add(1, 8, 0.2);
			ratings.Add(2, 4, 0.2);
			ratings.Add(2, 2, 0.6);
			ratings.Add(2, 5, 0.4);
			ratings.Add(3, 4, 0.2);
			ratings.Add(3, 3, 0.3);

			Assert.AreEqual(7, ratings.Count);
			ratings.RemoveItem(4);
		}

		[Test()] public void TestGet()
		{
			var ratings = new StaticFloatRatings(8);
			ratings.Add(1, 4, 0.3f);
			ratings.Add(1, 8, 0.2f);
			ratings.Add(2, 4, 0.2f);
			ratings.Add(2, 2, 0.6f);
			ratings.Add(2, 5, 0.4f);
			ratings.Add(3, 4, 0.2f);
			ratings.Add(3, 3, 0.3f);
			ratings.Add(6, 3, 0.3f);

			// test Get
			Assert.AreEqual(0.2f, ratings.Get(2, 4));
			Assert.AreEqual(0.3f, ratings.Get(3, 3));
			Assert.AreEqual(0.3f, ratings.Get(6, 3));

			// test index[,]
			Assert.AreEqual(0.3f, ratings[1, 4]);
			Assert.AreEqual(0.2f, ratings[1, 8]);
			Assert.AreEqual(0.6f, ratings[2, 2]);
		}
	}
}