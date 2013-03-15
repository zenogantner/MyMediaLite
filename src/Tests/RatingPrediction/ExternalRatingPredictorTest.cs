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
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

namespace Tests.RatingPrediction
{
	[TestFixture()]
	public class ExternalRatingPredictorTest
	{
		[Test()]
		public void TestCase()
		{
			string filename = "../../../../tests/example.test";
			var mapping = new IdentityMapping();
			var ratings = RatingData.Read(filename);

			var recommender = new ExternalRatingPredictor() { PredictionFile = filename, UserMapping = mapping, ItemMapping = mapping };
			recommender.Train();
			for (int i = 0; i < ratings.Count; i++)
				Assert.AreEqual(ratings[i], recommender.Predict(ratings.Users[i], ratings.Items[i]));
		}
	}
}

