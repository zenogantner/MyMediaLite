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
//

using System;
using NUnit.Framework;
using MyMediaLite.Data;

namespace Tests.Data
{
	[TestFixture()]
	public class RatingsSimpleSplitTest
	{
		[Test()]
		public void TestConstructor()
		{
			var ratings = new Ratings();
			ratings.Add(0, 0, 5.0f);
			ratings.Add(0, 1, 4.5f);
			ratings.Add(1, 0, 1.0f);
			ratings.Add(1, 1, 2.5f);

			var split1 = new RatingsSimpleSplit(ratings, 0.25);
			Assert.AreEqual(3, split1.Train[0].Count);
			Assert.AreEqual(1, split1.Test[0].Count);

			var split2 = new RatingsSimpleSplit(ratings, 0.5);
			Assert.AreEqual(2, split2.Train[0].Count);
			Assert.AreEqual(2, split2.Test[0].Count);
		}
	}
}

