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
using NUnit.Framework;
using MyMediaLite;
using MyMediaLite.DataType;
using MyMediaLite.ItemRecommendation;

namespace Tests.ItemRecommendation
{
	[TestFixture()]
	public class ItemRecommendersTest
	{
		[Test()]
		public void TestToString()
		{
			foreach (Type type in Utils.GetTypes("MyMediaLite.ItemRecommendation"))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IRecommender") != null)
				{
					var recommender = type.CreateItemRecommender();
					Assert.IsFalse(
						recommender.ToString().Contains(","),
						string.Format("ToString() output of {0} contains commas: '{1}'", type.Name, recommender.ToString())
					);
					Assert.IsTrue(
						recommender.ToString().StartsWith(type.Name),
						string.Format("ToString() output of {0} does not start with class name: '{1}'", type.Name, recommender.ToString ())
					);
				}
		}

		[Test()]
		public void TestFoldIn()
		{
			foreach (Type type in Utils.GetTypes("MyMediaLite.ItemRecommendation"))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IFoldInItemRecommender") != null)
				{
					var recommender = SetUpRecommender(type);

					var items_accessed_by_user = new int[] { 0, 1 };
					var items_to_score = new int[] { 2 };
					var scored_items = ((IFoldInItemRecommender) recommender).ScoreItems(items_accessed_by_user, items_to_score);
					Assert.AreEqual(1, scored_items.Count);
				}
		}
		
		[Test()]
		public void TestSaveLoad()
		{
			foreach (Type type in Utils.GetTypes("MyMediaLite.ItemRecommendation"))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IRecommender") != null)
				{
					if (type.Name == "Random" || type.Name == "ExternalItemRecommender")
						continue;
					if (type.Name == "MostPopularByAttributes" || type.Name == "ItemAttributeSVM")
						continue;
					if (type.Name == "ItemAttributeKNN" || type.Name == "UserAttributeKNN" || type.Name == "BPRLinear" || type.Name == "BPRSLIM")
						continue;

					try
					{
						var recommender = SetUpRecommender(type);

						var results = new float[5];
						for (int i = 0; i < results.Length; i++)
							results[i] = recommender.Predict(0, i);
					
						
						recommender.SaveModel("tmp.model");
						recommender.LoadModel("tmp.model");
						for (int i = 0; i < results.Length; i++)
							Assert.AreEqual(results[i], recommender.Predict(0, i), 0.0001);
					}
					catch (Exception e)
					{
						Assert.Fail("Exception while testing recommender {0}: {1}\n{2}", type.Name, e.Message, e.StackTrace);
					}
				}
		}
		
		private static ItemRecommender SetUpRecommender(Type type)
		{
			var recommender = (ItemRecommender) type.CreateItemRecommender();
			recommender.Feedback = TestUtils.CreatePosOnlyFeedback();
			if (type.GetInterface("IUserAttributeAwareRecommender") != null)
				((IUserAttributeAwareRecommender) recommender).UserAttributes = new SparseBooleanMatrix();
			if (type.GetInterface("IItemAttributeAwareRecommender") != null)
				((IItemAttributeAwareRecommender) recommender).ItemAttributes = new SparseBooleanMatrix();

			recommender.Train();
			return recommender;
		}
	}
}

