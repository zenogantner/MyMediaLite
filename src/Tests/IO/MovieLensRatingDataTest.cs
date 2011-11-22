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
using System.IO;
using NUnit.Framework;
using MyMediaLite.Data;
using MyMediaLite.IO;

namespace Tests.IO
{
	[TestFixture()]
	public class MovieLensRatingDataTest
	{
		[Test()]
		public void TestRead()
		{
			var reader = new StringReader(@"5951::50::5::881250949
5951::223::5::891717742
5951::260::5::878887116
5951::293::5::880606923
5951::356::4::886397596
5951::364::3::884182806
5951::457::3::879270459
");

			IRatings data = MovieLensRatingData.Read(reader, null, null);
			Assert.AreEqual(7, data.Count);
		}

	}
}
