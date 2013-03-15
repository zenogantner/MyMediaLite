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
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using Tests.Data;

namespace Tests.RatingPrediction
{
	[TestFixture()]
	public class MatrixFactorizationTest
	{
		[Test()]
		public void TestCurrentLearnRate()
		{
			var mf = new MatrixFactorization() { LearnRate = 1.1f, Ratings = TestUtils.CreateRatings() };

			mf.InitModel();
			Assert.AreEqual(1.1f, mf.LearnRate);
			Assert.AreEqual(1.1f, mf.current_learnrate);
		}

		[Test()]
		public void TestDefaultBehaviorIsNoDecay()
		{
			var mf = new MatrixFactorization() { LearnRate = 1.1f, NumIter = 10, Ratings = TestUtils.CreateRatings() };
			mf.Train();
			Assert.AreEqual(1.1f, mf.current_learnrate);
		}

		[Test()]
		public void TestDecay()
		{
			var mf = new MatrixFactorization()
			{
				LearnRate = 1.0f, Decay = 0.5f,
				NumIter = 1, Ratings = TestUtils.CreateRatings()
			};

			mf.Train();
			Assert.AreEqual(0.5f, mf.current_learnrate);

			mf.Iterate();
			Assert.AreEqual(0.25f, mf.current_learnrate);
		}

		[Test()]
		public void TestMatrixInit()
		{
			var mf = new MatrixFactorization() { Ratings = TestUtils.CreateRatings() };
			mf.InitModel();
			Assert.IsNotNull(mf.user_factors);
			Assert.IsNotNull(mf.item_factors);
		}
	}
}
