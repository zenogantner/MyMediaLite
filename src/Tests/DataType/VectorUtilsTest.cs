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

using System;
using System.Collections;
using System.Collections.Generic;
using MyMediaLite.DataType;
using NUnit.Framework;

namespace MyMediaLiteTest
{
	/// <summary>Testing the VectorUtils class</summary>
	[TestFixture()]
	public class VectorUtilsTest
	{
		[Test()] public void TestEuclideanNorm()
		{
			var testVector = new List<double>();
			testVector.Add(2);
			testVector.Add(5);
			testVector.Add(3);
			testVector.Add(7);
			testVector.Add(5);
			testVector.Add(3);
			double result = 11;
			Assert.AreEqual(result, VectorUtils.EuclideanNorm(testVector));
		}
	}
}