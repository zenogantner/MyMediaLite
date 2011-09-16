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
using MyMediaLite.DataType;
using NUnit.Framework;

namespace MyMediaLiteTest
{
	[TestFixture()]
	public class PosOnlyFeedbackTest
	{
		[Test()] public void TestMaxUserIDMaxItemID()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 7);
			feedback.Add(6, 3);

			Assert.AreEqual(6, feedback.MaxUserID);
			Assert.AreEqual(8, feedback.MaxItemID);
		}

		[Test()] public void TestAdd()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();

			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 7);
			feedback.Add(6, 3);
			feedback.Add(8, 1);

			Assert.IsTrue(feedback.UserMatrix[2, 5]);
			Assert.IsTrue(feedback.UserMatrix[1, 4]);
			Assert.IsTrue(feedback.UserMatrix[6, 3]);
			Assert.IsTrue(feedback.UserMatrix[2, 2]);
			Assert.IsFalse(feedback.UserMatrix[5, 2]);
			Assert.IsFalse(feedback.UserMatrix[4, 1]);
			Assert.IsFalse(feedback.UserMatrix[3, 6]);

			Assert.IsTrue(feedback.ItemMatrix[5, 2]);
			Assert.IsTrue(feedback.ItemMatrix[4, 1]);
			Assert.IsTrue(feedback.ItemMatrix[3, 6]);
			Assert.IsTrue(feedback.ItemMatrix[2, 2]);
			Assert.IsFalse(feedback.ItemMatrix[2, 5]);
			Assert.IsFalse(feedback.ItemMatrix[1, 4]);
			Assert.IsFalse(feedback.ItemMatrix[6, 3]);

			Assert.AreEqual(8, feedback.Count);
		}

		[Test()] public void TestRemove()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 7);
			feedback.Add(3, 3);
			feedback.Add(6, 3);

			Assert.AreEqual(8, feedback.Count);
			Assert.IsTrue(feedback.UserMatrix[2, 5]);
			feedback.Remove(2, 5);
			Assert.AreEqual(7, feedback.Count);
			feedback.Remove(6, 3);
			Assert.AreEqual(6, feedback.Count);

			Assert.IsFalse(feedback.UserMatrix[5, 2]);
			feedback.Remove(5, 2);
			Assert.IsFalse(feedback.UserMatrix[5, 2]);
			Assert.AreEqual(6, feedback.Count);
		}

		[Test()] public void TestRemoveUser()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 7);
			feedback.Add(3, 3);

			Assert.AreEqual(7, feedback.Count);
			Assert.IsTrue(feedback.UserMatrix[2, 5]);
			feedback.RemoveUser(2);
			Assert.AreEqual(4, feedback.Count);
			Assert.IsFalse(feedback.UserMatrix[2, 5]);
		}

		[Test()] public void TestRemoveItem()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 4);
			feedback.Add(3, 3);

			Assert.AreEqual(7, feedback.Count);
			Assert.IsTrue(feedback.UserMatrix[2, 4]);
			feedback.RemoveItem(4);
			Assert.IsFalse(feedback.UserMatrix[2, 4]);
			Assert.AreEqual(4, feedback.Count);
		}

		[Test()] public void TestAllUsers()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 7);
			feedback.Add(3, 3);
			feedback.Add(6, 3);

			Assert.AreEqual(4, feedback.AllUsers.Count);
		}

		[Test()] public void TestAllItems()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(1, 4);
			feedback.Add(1, 8);
			feedback.Add(2, 4);
			feedback.Add(2, 2);
			feedback.Add(2, 5);
			feedback.Add(3, 7);
			feedback.Add(3, 3);
			feedback.Add(6, 3);

			Assert.AreEqual(6, feedback.AllItems.Count);
		}

		[Test()] public void TestOverlapCount()
		{
			var feedback1 = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback1.Add(1, 4);
			feedback1.Add(1, 8);
			feedback1.Add(2, 4);
			feedback1.Add(2, 2);
			feedback1.Add(2, 5);

			var feedback2 = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback2.Add(2, 2);
			feedback2.Add(2, 5);
			feedback2.Add(3, 7);
			feedback2.Add(3, 3);
			feedback2.Add(6, 3);

			Assert.AreEqual(2, feedback1.OverlapCount(feedback2));
		}
	}
}