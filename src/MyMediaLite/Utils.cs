// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite
{
	/// <summary>Class containing utility functions</summary>
	public static class Utils
	{
		/// <summary>Memoize a function</summary>
		/// <param name="f">The function to memoize</param>
		/// <returns>a version of the function that remembers past function results</returns>
		public static Func<A, R> Memoize<A, R>(this Func<A, R> f)
		{
			var map = new Dictionary<A, R>();
			return a =>
			{
				R value;
				if (map.TryGetValue(a, out value))
				return value;
				value = f(a);
				map.Add(a, value);
				return value;
			};
		}

		/// <summary>Shuffle a list in-place</summary>
		/// <remarks>
		/// Fisher-Yates shuffle, see
		/// http://en.wikipedia.org/wiki/Fisher–Yates_shuffle
		/// </remarks>
		public static void Shuffle<T>(this IList<T> list)
		{
			Random random = MyMediaLite.Random.GetInstance();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				int r = random.Next(i + 1);

				// swap position i with position r
				T tmp = list[i];
				list[i] = list[r];
				list[r] = tmp;
			}
		}

		/// <summary>Get all types in a namespace</summary>
		/// <param name="name_space">a string describing the namespace</param>
		/// <returns>a list of Type objects</returns>
		public static IList<Type> GetTypes(string name_space)
		{
			var types = new List<Type>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				types.AddRange( assembly.GetTypes().Where(t => string.Equals(t.Namespace, name_space, StringComparison.Ordinal)) );

			return types;
		}
	}
}
