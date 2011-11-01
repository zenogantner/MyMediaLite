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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using MyMediaLite.DataType;

namespace Tests.DataType
{
	[TestFixture()]
	public class ListProxyTest
	{
		private IList<int> CreateSequence()
		{
			return new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		}

		private IList<int> CreateOddSequence()
		{
			return new int[] { 1, 3, 5, 7, 9 };
		}

		private IList<int> CreateEvenSequence()
		{
			return new int[] { 2, 4, 6, 8, 10 };
		}

		[Test()] public void TestIndex()
		{
			var list_proxy = new ListProxy<int>(CreateSequence(), CreateOddSequence());

			for (int i = 0; i < list_proxy.Count; i++)
				Assert.AreEqual(i * 2 + 2, list_proxy[i]);
		}

		[Test()] public void TestCount()
		{
			var list_proxy = new ListProxy<int>(CreateSequence(), CreateOddSequence());

			Assert.AreEqual(CreateOddSequence().Count, list_proxy.Count);
		}

		[Test()] public void TestIsReadOnly()
		{
			var list_proxy = new ListProxy<int>(CreateSequence(), CreateOddSequence());

			Assert.IsTrue(list_proxy.IsReadOnly);
		}

		[Test()] public void TestContains()
		{
			var list_proxy = new ListProxy<int>(CreateSequence(), CreateOddSequence());

			foreach (int num in CreateEvenSequence())
				Assert.IsTrue(list_proxy.Contains(num));

			foreach (int num in CreateOddSequence())
				Assert.IsFalse(list_proxy.Contains(num));
		}
		
		[Test()] public void TestGetEnumerator()
		{
			IEnumerable<int> list_proxy = new ListProxy<int>(CreateSequence(), CreateOddSequence());
			
			var enumerator = list_proxy.GetEnumerator();
			foreach (int e in CreateEvenSequence())
			{
				enumerator.MoveNext();
				Assert.AreEqual(e, enumerator.Current);
			}
		}
	}
}

