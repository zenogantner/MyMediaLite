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
using System.Collections.Generic;
using NUnit.Framework;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using Tests.Data;

namespace Tests.RatingPrediction
{
	[TestFixture()]
	public class SVDPlusPlusTest
	{
		[Test()]
		public void TestCurrentLearnRate()
		{
			var mf = new SVDPlusPlus() { LearnRate = 1.1f, Ratings = TestUtils.CreateRatings() };

			mf.InitModel();
			Assert.AreEqual(1.1f, mf.LearnRate);
			Assert.AreEqual(1.1f, mf.current_learnrate);
		}

		[Test()]
		public void TestDefaultBehaviorIsNoDecay()
		{
			var mf = new SVDPlusPlus() { LearnRate = 1.1f, NumIter = 10, Ratings = TestUtils.CreateRatings() };
			mf.Train();
			Assert.AreEqual(1.1f, mf.current_learnrate);
		}

		[Test()]
		public void TestDecay()
		{
			var svdpp = new SVDPlusPlus()
			{
				LearnRate = 1.0f, Decay = 0.5f,
				NumIter = 1, Ratings = TestUtils.CreateRatings()
			};

			svdpp.Train();
			Assert.AreEqual(0.5f, svdpp.current_learnrate);

			svdpp.Iterate();
			Assert.AreEqual(0.25f, svdpp.current_learnrate);
		}

		[Test()]
		public void TestMatrixInit()
		{
			var svdpp = new SVDPlusPlus() { Ratings = TestUtils.CreateRatings() };
			svdpp.InitModel();
			Assert.IsNotNull(svdpp.user_factors);
			Assert.IsNotNull(svdpp.item_factors);
			Assert.IsNotNull(svdpp.p);
			Assert.IsNotNull(svdpp.y);
			Assert.IsNotNull(svdpp.user_bias);
			Assert.IsNotNull(svdpp.item_bias);
		}

		[Test()]
		public void TestFoldIn()
		{
			var svdpp = new SVDPlusPlus() { Ratings = TestUtils.CreateRatings() };
			svdpp.Train();
			var user_ratings = new List<Tuple<int, float>>();
			user_ratings.Add(new Tuple<int, float>(0, 4.0f));
			var candidate_items = new List<int> { 0, 1 }; // have a known and an unknown item
			var results = svdpp.ScoreItems(user_ratings, candidate_items);
			Assert.AreEqual(2, results.Count);
		}
	}
}
