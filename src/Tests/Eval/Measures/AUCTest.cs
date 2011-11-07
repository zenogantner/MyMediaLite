// Copyright (C) 2011 Zeno Gantner
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
using MyMediaLite.Eval.Measures;

namespace Tests.Eval.Measures
{
	[TestFixture()]
	public class AUCTest
	{
		[Test()]
		public void TestCompute()
		{
			var ranking = new int[] { 1, 2, 3, 4 };

			// everything correct
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1 }));
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2 }));
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2, 3 }));

			// everything wrong
			Assert.AreEqual(0.0, AUC.Compute(ranking, new int[] { 4 }));

			// .33
			Assert.AreEqual(0.333, AUC.Compute(ranking, new int[] { 3 }), 0.01);

			// 0.66
			Assert.AreEqual(0.666, AUC.Compute(ranking, new int[] { 2 }), 0.01);

			// .75
			Assert.AreEqual(0.75, AUC.Compute(ranking, new int[] { 1, 3 }));

			// .5
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 1, 2, 3, 4 }));

			// .25
			Assert.AreEqual(0.25, AUC.Compute(ranking, new int[] { 2, 4 }));
		}

		[Test()]
		public void TestComputeWithIgnoreItems()
		{
			var ranking = new int[] { 1, 2, 3, 4 };
			var ignore  = new int[] { 4 };

			// everything correct
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1 }, ignore));
			Assert.AreEqual(1.0, AUC.Compute(ranking, new int[] { 1, 2 }, ignore));

			// everything wrong
			Assert.AreEqual(0.0, AUC.Compute(ranking, new int[] { 3 }, ignore));

			// .5
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 1, 2, 3 }, ignore));
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 1, 2, 3, 4 }, ignore));
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 2 }, ignore));
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 2, 4 }, ignore));
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 4 }, ignore));
			Assert.AreEqual(0.5, AUC.Compute(ranking, new int[] { 1, 3 }, ignore));
		}

	}
}

