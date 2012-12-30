// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Tina Lichtenthäler, Zeno Gantner
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
using System.Collections.Generic;
using MyMediaLite.DataType;
using NUnit.Framework;

namespace Tests.DataType
{
	[TestFixture()]
	public class VectorExtensionsTest
	{
		[Test()] public void TestEuclideanNorm()
		{
			var test_vector = new List<float>() { 2, 5, 3, 7, 5, 3 };
			Assert.AreEqual(11, test_vector.EuclideanNorm());
		}
		
		[Test()] public void TestInit()
		{
			var test_vector = new float[10];
			test_vector.Init(1.3f);
			for (int i = 0; i < test_vector.Length; i++)
				Assert.AreEqual(1.3f, test_vector[i]);
		}
	}
}