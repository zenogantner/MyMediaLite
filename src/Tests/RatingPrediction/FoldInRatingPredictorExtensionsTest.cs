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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using MyMediaLite.DataType;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

namespace Tests.RatingPrediction
{
	[TestFixture()]
	public class FoldInRatingPredictorExtensionsTest
	{
		IFoldInRatingPredictor CreateRecommender()
		{
			var training_data = RatingData.Read("../../../../data/ml-100k/u.data");
			var recommender = new MatrixFactorization();
			recommender.Ratings = training_data;
			recommender.NumFactors = 4;
			recommender.NumIter = 5;
			recommender.Train();

			return recommender;
		}

		[Test()]
		public void TestTopNWithCandidates()
		{
			var rated_items = new List<Tuple<int, float>>();
			rated_items.Add(Tuple.Create(1, 1.0f));
			rated_items.Add(Tuple.Create(2, 4.0f));
			rated_items.Add(Tuple.Create(3, 4.5f));

			var candidate_items = new int[] { 4, 5, 6, 7, 8 };

			IFoldInRatingPredictor recommender = CreateRecommender();

			var result = recommender.RecommendItems(rated_items, candidate_items, 3);
			Assert.GreaterOrEqual(result[0].Item2, result[1].Item2);
			Assert.GreaterOrEqual(result[0].Item2, result[2].Item2);
			Assert.GreaterOrEqual(result[1].Item2, result[2].Item2);
			Assert.Contains(result[0].Item1, candidate_items);
			Assert.Contains(result[1].Item1, candidate_items);
			Assert.Contains(result[2].Item1, candidate_items);
			Assert.AreEqual(3, result.Count);
		}

		[Test()]
		public void TestTopNWithoutCandidates()
		{
			var rated_items = new List<Tuple<int, float>>();
			rated_items.Add(Tuple.Create(1, 1.0f));
			rated_items.Add(Tuple.Create(2, 4.0f));
			rated_items.Add(Tuple.Create(3, 4.5f));

			IFoldInRatingPredictor recommender = CreateRecommender();

			var result = recommender.RecommendItems(rated_items, 3);
			Assert.GreaterOrEqual(result[0].Item2, result[1].Item2);
			Assert.GreaterOrEqual(result[0].Item2, result[2].Item2);
			Assert.GreaterOrEqual(result[1].Item2, result[2].Item2);
			Assert.AreEqual(3, result.Count);
		}
	}
}

