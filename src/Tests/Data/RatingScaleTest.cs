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
using MyMediaLite.Data;

namespace Tests.Data
{
	[TestFixture()]
	public class RatingScaleTest
	{
		IList<float> CreateRatings()
		{
			return new float[] { 0.3f, 0.2f, 0.2f, 0.6f, 0.4f, 0.2f, 0.3f };
		}

		[Test()]
		public void TestConstructor1()
		{
			var levels = new float[] {0f, 1f};
			var scale = new RatingScale(levels.ToList());
			Assert.AreEqual(0f, scale.Min);
			Assert.AreEqual(1f, scale.Max);
			Assert.AreEqual(2, scale.Levels.Count);
			Assert.AreEqual(0, scale.LevelID[0f]);
			Assert.AreEqual(1, scale.LevelID[1f]);
		}

		public void TestConstructor2()
		{
			var scale = new RatingScale(CreateRatings());
			Assert.AreEqual(0.2f, scale.Min);
			Assert.AreEqual(0.6f, scale.Max);
			Assert.AreEqual(4, scale.Levels.Count);
			Assert.AreEqual(0, scale.LevelID[0.2f]);
			Assert.AreEqual(1, scale.LevelID[0.3f]);
			Assert.AreEqual(2, scale.LevelID[0.4f]);
			Assert.AreEqual(3, scale.LevelID[0.6f]);
		}

		public void TestConstructor3_1()
		{
			var levels = (new float[] {0f, 1f}).ToList();
			var scale = new RatingScale(new RatingScale(levels), new RatingScale(levels));
			Assert.AreEqual(0f, scale.Min);
			Assert.AreEqual(1f, scale.Max);
			Assert.AreEqual(2, scale.Levels.Count);
			Assert.AreEqual(0, scale.LevelID[0f]);
			Assert.AreEqual(1, scale.LevelID[1f]);
		}

		public void TestConstructor3_2()
		{
			var levels = (new float[] {0f, 1f}).ToList();
			var scale = new RatingScale(new RatingScale(levels), new RatingScale(CreateRatings()));
			Assert.AreEqual(0f, scale.Min);
			Assert.AreEqual(1f, scale.Max);
			Assert.AreEqual(6, scale.Levels.Count);
			Assert.AreEqual(0, scale.LevelID[0f]);
			Assert.AreEqual(1, scale.LevelID[0.2f]);
			Assert.AreEqual(2, scale.LevelID[0.3f]);
			Assert.AreEqual(3, scale.LevelID[0.4f]);
			Assert.AreEqual(4, scale.LevelID[0.6f]);
			Assert.AreEqual(5, scale.LevelID[1f]);
		}
	}
}

