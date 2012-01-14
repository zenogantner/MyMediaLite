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
	public class RatingsPerUserChronologicalSplitTest
	{
		[Test()]
		public void TestRatioSplit()
		{
			var ratings = new TimedRatings();
			ratings.Add(0, 0, 5.0f, new DateTime(2011, 10, 31));
			ratings.Add(0, 1, 4.5f, new DateTime(2011, 11, 1));
			ratings.Add(0, 2, 5.0f, new DateTime(2011, 11, 3));
			ratings.Add(0, 3, 4.5f, new DateTime(2011, 11, 4));
			ratings.Add(1, 0, 1.0f, new DateTime(2011, 10, 31));
			ratings.Add(1, 1, 2.5f, new DateTime(2011, 11, 2));
			ratings.Add(1, 2, 1.0f, new DateTime(2011, 12, 1));
			ratings.Add(1, 3, 2.5f, new DateTime(2011, 12, 4));

			var split1 = new RatingsPerUserChronologicalSplit(ratings, 0.25);
			Assert.AreEqual(6, split1.Train[0].Count);
			Assert.AreEqual(2, split1.Test[0].Count);
			Assert.AreEqual(3, split1.Train[0].ByUser[0].Count);
			Assert.AreEqual(3, split1.Train[0].ByUser[1].Count);
			Assert.AreEqual(1, split1.Test[0].ByUser[0].Count);
			Assert.AreEqual(1, split1.Test[0].ByUser[1].Count);

			var split2 = new RatingsPerUserChronologicalSplit(ratings, 0.5);
			Assert.AreEqual(4, split2.Train[0].Count);
			Assert.AreEqual(4, split2.Test[0].Count);
			Assert.AreEqual(2, split2.Train[0].ByUser[0].Count);
			Assert.AreEqual(2, split2.Train[0].ByUser[1].Count);
			Assert.AreEqual(2, split2.Test[0].ByUser[0].Count);
			Assert.AreEqual(2, split2.Test[0].ByUser[1].Count);
		}

		[Test()]
		public void TestNumberSplit()
		{
			var ratings = new TimedRatings();
			ratings.Add(0, 0, 5.0f, new DateTime(2011, 10, 31));
			ratings.Add(0, 1, 4.5f, new DateTime(2011, 11, 1));
			ratings.Add(0, 2, 5.0f, new DateTime(2011, 11, 3));
			ratings.Add(0, 3, 4.5f, new DateTime(2011, 11, 4));
			ratings.Add(1, 0, 1.0f, new DateTime(2011, 10, 31));
			ratings.Add(1, 1, 2.5f, new DateTime(2011, 11, 2));
			ratings.Add(1, 2, 1.0f, new DateTime(2011, 12, 1));
			ratings.Add(1, 3, 2.5f, new DateTime(2011, 12, 4));

			var split1 = new RatingsPerUserChronologicalSplit(ratings, 1);
			Assert.AreEqual(6, split1.Train[0].Count);
			Assert.AreEqual(2, split1.Test[0].Count);
			Assert.AreEqual(3, split1.Train[0].ByUser[0].Count);
			Assert.AreEqual(3, split1.Train[0].ByUser[1].Count);
			Assert.AreEqual(1, split1.Test[0].ByUser[0].Count);
			Assert.AreEqual(1, split1.Test[0].ByUser[1].Count);

			var split2 = new RatingsPerUserChronologicalSplit(ratings, 2);
			Assert.AreEqual(4, split2.Train[0].Count);
			Assert.AreEqual(4, split2.Test[0].Count);
			Assert.AreEqual(2, split2.Train[0].ByUser[0].Count);
			Assert.AreEqual(2, split2.Train[0].ByUser[1].Count);
			Assert.AreEqual(2, split2.Test[0].ByUser[0].Count);
			Assert.AreEqual(2, split2.Test[0].ByUser[1].Count);

			var split3 = new RatingsPerUserChronologicalSplit(ratings, 3);
			Assert.AreEqual(2, split3.Train[0].Count);
			Assert.AreEqual(6, split3.Test[0].Count);
			Assert.AreEqual(1, split3.Train[0].ByUser[0].Count);
			Assert.AreEqual(1, split3.Train[0].ByUser[1].Count);
			Assert.AreEqual(3, split3.Test[0].ByUser[0].Count);
			Assert.AreEqual(3, split3.Test[0].ByUser[1].Count);

			var split4 = new RatingsPerUserChronologicalSplit(ratings, 4);
			Assert.AreEqual(0, split4.Train[0].Count);
			Assert.AreEqual(8, split4.Test[0].Count);
			Assert.AreEqual(0, split4.Train[0].ByUser[0].Count);
			Assert.AreEqual(0, split4.Train[0].ByUser[1].Count);
			Assert.AreEqual(4, split4.Test[0].ByUser[0].Count);
			Assert.AreEqual(4, split4.Test[0].ByUser[1].Count);

			var split5 = new RatingsPerUserChronologicalSplit(ratings, 5);
			Assert.AreEqual(0, split5.Train[0].Count);
			Assert.AreEqual(8, split5.Test[0].Count);
			Assert.AreEqual(0, split5.Train[0].ByUser[0].Count);
			Assert.AreEqual(0, split5.Train[0].ByUser[1].Count);
			Assert.AreEqual(4, split5.Test[0].ByUser[0].Count);
			Assert.AreEqual(4, split5.Test[0].ByUser[1].Count);
		}

	}
}

