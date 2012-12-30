// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.Eval.Measures;

namespace Tests.Eval.Measures
{
	[TestFixture()]
	public class PrecisionAndRecallTest
	{
		static readonly int[] list5 = new int[] { 1, 2, 3, 4, 5 };
		static readonly int[] list1 = new int[] { 1 };
		static readonly int[] list3 = new int[] { 1, 2, 3 };
		static readonly int[] list_last = new int[] { 5 };

		[Test()]
		public void TestAP()
		{
			Assert.AreEqual(1, PrecisionAndRecall.AP(list5, list1));
			Assert.AreEqual(1, PrecisionAndRecall.AP(list5, list5));
			Assert.AreEqual(0, PrecisionAndRecall.AP(list3, list_last));
			Assert.AreEqual((double) 1/5, PrecisionAndRecall.AP(list5, list_last));

			Assert.AreEqual(1, PrecisionAndRecall.AP(list5, list1));
			Assert.AreEqual(1, PrecisionAndRecall.AP(list5, list5));
			Assert.AreEqual(0, PrecisionAndRecall.AP(list3, list_last));
			Assert.AreEqual((double) 1/5, PrecisionAndRecall.AP(list5, list_last));
		}

		[Test()]
		public void TestHitsAt()
		{
			Assert.AreEqual(1, PrecisionAndRecall.HitsAt(list1, list1, 1));
			Assert.AreEqual(1, PrecisionAndRecall.HitsAt(list3, list1, 1));
			Assert.AreEqual(1, PrecisionAndRecall.HitsAt(list3, list1, 2));
			Assert.AreEqual(1, PrecisionAndRecall.HitsAt(list3, list1, 3));
			Assert.AreEqual(1, PrecisionAndRecall.HitsAt(list3, list1, 4));
		}

		[Test()]
		public void TestPrecisionAt()
		{
			Assert.AreEqual(1, PrecisionAndRecall.PrecisionAt(list1, list1, 1));
			Assert.AreEqual(1, PrecisionAndRecall.PrecisionAt(list3, list1, 1));
			Assert.AreEqual((double) 1/2, PrecisionAndRecall.PrecisionAt(list3, list1, 2));
			Assert.AreEqual((double) 1/3, PrecisionAndRecall.PrecisionAt(list3, list1, 3));
		}

		[Test()]
		public void TestRecallAt()
		{
			Assert.AreEqual(1, PrecisionAndRecall.RecallAt(list1, list1, 1));
			Assert.AreEqual(1, PrecisionAndRecall.RecallAt(list3, list1, 1));
			Assert.AreEqual(1, PrecisionAndRecall.RecallAt(list3, list1, 2));
			Assert.AreEqual(1, PrecisionAndRecall.RecallAt(list3, list1, 3));
		}
	}
}

