// Copyright (C) 2012, 2013 Zeno Gantner
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
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.ItemRecommendation;

namespace Tests.Eval
{
	[TestFixture()]
	public class ItemsTest
	{
		IList<int> allUsers, candidateItems;
		IRecommender recommender;
		IInteractions trainingData, testData;

		[SetUp()]
		public void SetUp()
		{
			trainingData = TestUtils.CreateFeedback(new Tuple<int, int>[] {
				Tuple.Create(1, 1),
				Tuple.Create(1, 2),
				Tuple.Create(2, 2),
				Tuple.Create(3, 1),
				Tuple.Create(3, 2),
				Tuple.Create(3, 3),
			});

			var factory = new MostPopular();
			var model = factory.Train(trainingData, null);
			recommender = factory.CreateRecommender(model);

			testData = TestUtils.CreateFeedback(new Tuple<int, int>[] {
				Tuple.Create(2, 3),
				Tuple.Create(2, 4),
				Tuple.Create(4, 1),
				Tuple.Create(4, 4),
			});

			allUsers = Enumerable.Range(1, 4).ToList();
			candidateItems = Enumerable.Range(1, 5).ToList();
		}

		[Test()]
		public void TestEvalDefault()
		{
			var results = Items.Evaluate(recommender, testData, trainingData);
			Assert.AreEqual(2, results["num_lists"]);
			Assert.AreEqual(0.5, results["AUC"]);
			Assert.AreEqual(0.5, results["prec@5"], 0.01);
		}

		[Test()]
		public void TestEvalDefaultGivenUserAndItems()
		{
			var results = Items.Evaluate(recommender, testData, trainingData, allUsers, candidateItems);
			Assert.AreEqual(3, results["num_lists"]);
			Assert.AreEqual(0.5, results["AUC"]);
			Assert.AreEqual(0.333, results["prec@5"], 0.01);
		}
	}
}

