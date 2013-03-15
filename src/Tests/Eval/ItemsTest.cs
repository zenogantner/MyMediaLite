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
using System.Linq;
using NUnit.Framework;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.ItemRecommendation;

namespace Tests.Eval
{
	[TestFixture()]
	public class ItemsTest
	{
		IList<int> all_users, candidate_items;
		ItemRecommender recommender;
		IPosOnlyFeedback training_data, test_data;

		[SetUp()]
		public void SetUp()
		{
			training_data = new PosOnlyFeedback<SparseBooleanMatrix>();
			training_data.Add(1, 1);
			training_data.Add(1, 2);
			training_data.Add(2, 2);
			training_data.Add(2, 3);
			training_data.Add(3, 1);
			training_data.Add(3, 2);

			recommender = new MostPopular() { Feedback = training_data };
			recommender.Train();

			test_data = new PosOnlyFeedback<SparseBooleanMatrix>();
			test_data.Add(2, 3);
			test_data.Add(2, 4);
			test_data.Add(4, 4);

			all_users = Enumerable.Range(1, 4).ToList();
			candidate_items = Enumerable.Range(1, 5).ToList();
		}

		[Test()]
		public void TestEvalDefault()
		{
			var results = Items.Evaluate(recommender, test_data, training_data);
			Assert.AreEqual(1, results["num_lists"]);
			Assert.AreEqual(0.5, results["AUC"]);
			Assert.AreEqual(0.0, results["prec@5"], 0.01);
		}

		[Test()]
		public void TestEvalDefaultGivenUserAndItems()
		{
			var results = Items.Evaluate(recommender, test_data, training_data, all_users, candidate_items);
			Assert.AreEqual(1, results["num_lists"]);
			Assert.AreEqual(0.5, results["AUC"]);
			Assert.AreEqual(0.0, results["prec@5"], 0.01);
		}
	}
}

