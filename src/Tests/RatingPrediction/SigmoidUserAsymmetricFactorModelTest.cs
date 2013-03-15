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
	public class SigmoidUserAsymmetricFactorModelTest
	{
		[Test()]
		public void TestCurrentLearnRate()
		{
			var afm = new SigmoidUserAsymmetricFactorModel() { LearnRate = 1.1f, Ratings = TestUtils.CreateRatings() };

			afm.InitModel();
			Assert.AreEqual(1.1f, afm.LearnRate);
			Assert.AreEqual(1.1f, afm.current_learnrate);
		}

		[Test()]
		public void TestDefaultBehaviorIsNoDecay()
		{
			var afm = new SigmoidUserAsymmetricFactorModel() { LearnRate = 1.1f, NumIter = 10, Ratings = TestUtils.CreateRatings() };
			afm.Train();
			Assert.AreEqual(1.1f, afm.current_learnrate);
		}

		[Test()]
		public void TestDecay()
		{
			var afm = new SigmoidUserAsymmetricFactorModel()
			{
				LearnRate = 1.0f, Decay = 0.5f,
				NumIter = 1, Ratings = TestUtils.CreateRatings()
			};

			afm.Train();
			Assert.AreEqual(0.5f, afm.current_learnrate);

			afm.Iterate();
			Assert.AreEqual(0.25f, afm.current_learnrate);
		}

		[Test()]
		public void TestMatrixInit()
		{
			var afm = new SigmoidUserAsymmetricFactorModel() { Ratings = TestUtils.CreateRatings() };
			afm.InitModel();
			Assert.IsNotNull(afm.user_factors);
			Assert.IsNotNull(afm.item_factors);
			Assert.IsNotNull(afm.x);
			Assert.IsNotNull(afm.user_bias);
			Assert.IsNotNull(afm.item_bias);
		}
	}
}
