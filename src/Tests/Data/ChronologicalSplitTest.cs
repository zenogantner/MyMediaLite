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
using System.Collections.Generic;
using NUnit.Framework;
using MyMediaLite.Data;
using MyMediaLite.Data.Split;

namespace Tests.Data
{
	[TestFixture()]
	public class ChronologicalSplitTest
	{
		[Test()]
		public void TestRatioSplit()
		{
			var interaction_list = new List<IInteraction>();
			interaction_list.Add(new FullInteraction(0, 0, 5.0f, new DateTime(2011, 10, 31)));
			interaction_list.Add(new FullInteraction(0, 1, 4.5f, new DateTime(2011, 11, 1)));
			interaction_list.Add(new FullInteraction(1, 0, 1.0f, new DateTime(2011, 10, 31)));
			interaction_list.Add(new FullInteraction(1, 1, 2.5f, new DateTime(2011, 11, 2)));

			var split1 = new ChronologicalSplit(new Interactions(interaction_list), 0.25);
			Assert.AreEqual(3, split1.Train[0].Count);
			Assert.AreEqual(1, split1.Test[0].Count);
			Assert.AreEqual(2, split1.Train[0].ByUser(0).Count);
			Assert.AreEqual(1, split1.Train[0].ByUser(1).Count);
			Assert.AreEqual(0, split1.Test[0].ByUser(0).Count);
			Assert.AreEqual(1, split1.Test[0].ByUser(1).Count);
			Assert.AreEqual(new DateTime(2011, 10, 31), split1.Train[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 1),  split1.Train[0].LatestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 2), split1.Test[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 2), split1.Test[0].LatestDateTime);


			var split2 = new ChronologicalSplit(new Interactions(interaction_list), 0.5);
			Assert.AreEqual(2, split2.Train[0].Count);
			Assert.AreEqual(2, split2.Test[0].Count);
			Assert.AreEqual(1, split2.Train[0].ByUser(0).Count);
			Assert.AreEqual(1, split2.Train[0].ByUser(1).Count);
			Assert.AreEqual(1, split2.Test[0].ByUser(0).Count);
			Assert.AreEqual(1, split2.Test[0].ByUser(1).Count);
			Assert.AreEqual(new DateTime(2011, 10, 31), split2.Train[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 10, 31), split2.Train[0].LatestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 1), split2.Test[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 2), split2.Test[0].LatestDateTime);
		}

		[Test()]
		public void TestTimeSplit()
		{
			var interaction_list = new List<IInteraction>();
			interaction_list.Add(new FullInteraction(0, 0, 5.0f, new DateTime(2011, 10, 31)));
			interaction_list.Add(new FullInteraction(0, 1, 4.5f, new DateTime(2011, 11, 1)));
			interaction_list.Add(new FullInteraction(1, 0, 1.0f, new DateTime(2011, 10, 31)));
			interaction_list.Add(new FullInteraction(1, 1, 2.5f, new DateTime(2011, 11, 2)));

			var split1 = new ChronologicalSplit(new Interactions(interaction_list), new DateTime(2011, 11, 2));
			Assert.AreEqual(3, split1.Train[0].Count);
			Assert.AreEqual(1, split1.Test[0].Count);
			Assert.AreEqual(2, split1.Train[0].ByUser(0).Count);
			Assert.AreEqual(1, split1.Train[0].ByUser(1).Count);
			Assert.AreEqual(0, split1.Test[0].ByUser(0).Count);
			Assert.AreEqual(1, split1.Test[0].ByUser(1).Count);
			Assert.AreEqual(new DateTime(2011, 10, 31), split1.Train[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 1),  split1.Train[0].LatestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 2), split1.Test[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 2), split1.Test[0].LatestDateTime);

			var split2 = new ChronologicalSplit(new Interactions(interaction_list), new DateTime(2011, 11, 1));
			Assert.AreEqual(2, split2.Train[0].Count);
			Assert.AreEqual(2, split2.Test[0].Count);
			Assert.AreEqual(1, split2.Train[0].ByUser(0).Count);
			Assert.AreEqual(1, split2.Train[0].ByUser(1).Count);
			Assert.AreEqual(1, split2.Test[0].ByUser(0).Count);
			Assert.AreEqual(1, split2.Test[0].ByUser(1).Count);
			Assert.AreEqual(new DateTime(2011, 10, 31), split2.Train[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 10, 31), split2.Train[0].LatestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 1), split2.Test[0].EarliestDateTime);
			Assert.AreEqual(new DateTime(2011, 11, 2), split2.Test[0].LatestDateTime);
		}

	}
}

