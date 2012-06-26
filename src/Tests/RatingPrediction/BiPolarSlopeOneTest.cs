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

using System;
using NUnit.Framework;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;

namespace Tests.RatingPrediction
{
	[TestFixture()]
	public class BiPolarSlopeOneTest
	{
		[Test()]
		public void TestNewItemInTestSet()
		{
			var recommender = new BiPolarSlopeOne();

			var training_data = new Ratings();
			training_data.Add(0, 0, 1.0f);
			training_data.Add(1, 1, 5.0f);
			training_data.InitScale();

			recommender.Ratings = training_data;
			recommender.Train();

			Assert.AreEqual( 3.0f, recommender.Predict(0, 2) );
		}

		[Test()]
		public void TestNewUserInTestSet()
		{
			var recommender = new BiPolarSlopeOne();

			var training_data = new Ratings();
			training_data.Add(0, 0, 1.0f);
			training_data.Add(1, 1, 5.0f);
			training_data.InitScale();

			recommender.Ratings = training_data;
			recommender.Train();

			Assert.AreEqual( 3.0f, recommender.Predict(2, 1) );
		}
	}
}