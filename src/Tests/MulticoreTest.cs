// Copyright (C) 2012 Zeno Gantner
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
//
using System;
using NUnit.Framework;
using MyMediaLite;

namespace Tests
{
	[TestFixture()]
	public class MulticoreTest
	{
		[Test()]
		public void TestPartitionUsersAndItems()
		{
			int NUM_RATINGS = 300;
			int NUM_USERS = 15;
			int NUM_ITEMS = 30;
			int NUM_GROUPS = 3;

			var ratings = TestUtils.CreateRandomRatings(NUM_USERS, NUM_ITEMS, NUM_RATINGS);
			var partition = ratings.PartitionUsersAndItems(NUM_GROUPS);
			Assert.AreEqual(NUM_GROUPS, partition.GetLength(0));
			Assert.AreEqual(NUM_GROUPS, partition.GetLength(1));
		}

		[Test()]
		public void TestPartitionUsersAndItemsLessUsersThanGroups()
		{
			int NUM_RATINGS = 300;
			int NUM_USERS = 15;
			int NUM_ITEMS = 30;
			int NUM_GROUPS = 20;

			var ratings = TestUtils.CreateRandomRatings(NUM_USERS, NUM_ITEMS, NUM_RATINGS);
			var partition = ratings.PartitionUsersAndItems(NUM_GROUPS);
			Assert.AreEqual(ratings.MaxUserID + 1, partition.GetLength(0));
			Assert.AreEqual(ratings.MaxUserID + 1, partition.GetLength(1));
		}

		[Test()]
		public void TestPartitionItemsAndItemsLessUsersThanGroups()
		{
			int NUM_RATINGS = 300;
			int NUM_USERS = 30;
			int NUM_ITEMS = 15;
			int NUM_GROUPS = 20;

			var ratings = TestUtils.CreateRandomRatings(NUM_USERS, NUM_ITEMS, NUM_RATINGS);
			var partition = ratings.PartitionUsersAndItems(NUM_GROUPS);
			Assert.AreEqual(ratings.MaxItemID + 1, partition.GetLength(0));
			Assert.AreEqual(ratings.MaxItemID + 1, partition.GetLength(1));
		}

		[Test()]
		public void TestPartitionIndices()
		{
			int NUM_RATINGS = 300;
			int NUM_GROUPS = 10;
			Assert.AreEqual(0, NUM_RATINGS % NUM_GROUPS, "For this test, NUM_RATINGS must be dividable by NUM_GROUPS");
			int group_size = NUM_RATINGS / NUM_GROUPS;

			var ratings = TestUtils.CreateRandomRatings(15, 15, NUM_RATINGS);
			var partition = ratings.PartitionIndices(NUM_GROUPS);
			Assert.AreEqual(NUM_GROUPS, partition.Count);
			for (int i = 0; i < partition.Count; i++)
				Assert.AreEqual(group_size, partition[i].Count);
		}

		[Test()]
		public void TestPartitionIndicesLessRatingsThanThreads()
		{
			int NUM_RATINGS = 10;
			int NUM_GROUPS = 50;

			var ratings = TestUtils.CreateRandomRatings(15, 15, NUM_RATINGS);
			var partition = ratings.PartitionIndices(NUM_GROUPS);
			Assert.AreEqual(NUM_RATINGS, partition.Count);
		}
	}
}

