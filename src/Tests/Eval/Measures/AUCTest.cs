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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MyMediaLite.Eval.Measures;

namespace Tests.Eval.Measures
{
	[TestFixture()]
	public class AUCTest
	{
		IList<int> ranking;

		[SetUp()]
		public void SetUp()
		{
			ranking = Enumerable.Range(1, 4).ToList();
		}

		[Test()]
		public void TestComputeAUCAllCorrect()
		{
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1 }, 0));
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2 }, 0));
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2, 3 }, 0));
		}

		public void TestComputeAUCAllWrong()
		{
			Assert.AreEqual(0.0, AUC.Compute(ranking, new int[] { 4 }, 0));
		}

		public void TestComputeAUCOneThird()
		{
			Assert.AreEqual(0.333, AUC.Compute(ranking, new int[] { 3 }, 0), 0.01);
		}

		public void TestComputeAUCTwoThirds()
		{
			Assert.AreEqual(0.666, AUC.Compute(ranking, new int[] { 2 }, 0), 0.01);
		}

		public void TestComputeAUCThreeQuarters()
		{
			Assert.AreEqual(0.75, AUC.Compute(ranking, new int[] { 1, 3 }, 0));
		}

		public void TestComputeAUCHalf()
		{
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 1, 2, 3, 4 }, 0));
		}

		public void TestComputeAUCOneQuarter()
		{
			Assert.AreEqual(0.25, AUC.Compute(ranking, new int[] { 2, 4 }, 0));
		}

		[Test()]
		public void TestComputeWithDroppedItems()
		{
			for (int i = 0; i < 10; i++)
			{
				Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1 }, i));
				Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2 }, i));
				Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2, 3 }, i));
			}

			for (int i = 0; i < 10; i++)
				Assert.AreEqual((double) i / (i + 3), AUC.Compute(ranking, new int[] { 4 }, i));
		}
	}
}

