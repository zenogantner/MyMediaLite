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
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.RatingPrediction;

namespace Tests.RatingPrediction
{
	[TestFixture()]
	public class RatingPredictorsTest
	{
		[Test()]
		public void TestToString()
		{
			foreach (Type type in Utils.GetTypes("MyMediaLite.RatingPrediction"))
			{
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IRecommender") != null)
				{
					var recommender = type.CreateRatingPredictor();
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
		}

		[Test()]
		public void TestFoldIn()
		{
			foreach (Type type in Utils.GetTypes("MyMediaLite.RatingPrediction"))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IFoldInRatingPredictor") != null)
				{
					if (type.Name == "SigmoidUserAsymmetricFactorModel" || type.Name == "UserAttributeKNN" || type.Name == "GSVDPlusPlus")
						continue;

					try
					{
						var recommender = SetUpRecommender(type);

						var items_rated_by_user = new Tuple<int, float>[] { Tuple.Create(1, 1.0f), Tuple.Create(2, 1.5f) };
						var items_to_rate = new int[] { 2 };
						var rated_items = ((IFoldInRatingPredictor) recommender).ScoreItems(items_rated_by_user, items_to_rate);
						Assert.AreEqual(1, rated_items.Count);
					}
					catch (Exception e)
					{
						Assert.Fail("Exception while testing recommender {0}: {1}\n{2}", type.Name, e.Message, e.StackTrace);
					}
				}
		}

		[Test()]
		public void TestSaveLoad()
		{
			foreach (Type type in Utils.GetTypes("MyMediaLite.RatingPrediction"))
			{
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IRecommender") != null)
				{
					if (type.Name == "Random" || type.Name == "ExternalRatingPredictor")
						continue;
					if (type.Name == "LatentFeatureLogLinearModel" || type.Name == "NaiveBayes")
						continue;
					if (type.Name == "TimeAwareBaseline" || type.Name == "TimeAwareBaselineWithFrequencies")
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
		}

		private static RatingPredictor SetUpRecommender(Type type)
		{
			var recommender = type.CreateRatingPredictor();
			if (recommender is ITimeAwareRatingPredictor)
				recommender.Ratings = TestUtils.CreateRandomTimedRatings(5, 5, 10);
			else
				recommender.Ratings = TestUtils.CreateRandomRatings(5, 5, 10);
			if (type.GetInterface("IUserAttributeAwareRecommender") != null)
				((IUserAttributeAwareRecommender) recommender).UserAttributes = new SparseBooleanMatrix();
			if (type.GetInterface("IItemAttributeAwareRecommender") != null)
				((IItemAttributeAwareRecommender) recommender).ItemAttributes = new SparseBooleanMatrix();

			recommender.Train();

			return recommender;
		}
	}
}

