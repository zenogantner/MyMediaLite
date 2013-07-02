// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using MyMediaLite.Data.Split;

namespace Tests.Data
{
	[TestFixture()]
	public class SimpleSplitTest
	{
		[Test()]
		public void TestConstructor()
		{
			var interactions = TestUtils.CreateFeedback( new Tuple<int, int>[] {
				Tuple.Create(0, 0),
				Tuple.Create(0, 1),
				Tuple.Create(1, 0),
				Tuple.Create(1, 1),
			});

			var split1 = new SimpleSplit(interactions, 0.25);
			Assert.AreEqual(3, split1.Train[0].Count);
			Assert.AreEqual(1, split1.Test[0].Count);

			var split2 = new SimpleSplit(interactions, 0.5);
			Assert.AreEqual(2, split2.Train[0].Count);
			Assert.AreEqual(2, split2.Test[0].Count);
		}
	}
}

