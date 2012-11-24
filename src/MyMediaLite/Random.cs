// Copyright (C) 2010, 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle
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

namespace MyMediaLite
{
	/// <summary>Random number generator singleton class</summary>
	public class Random : System.Random
	{
		[ThreadStatic]
		private static Random instance;

		private static Nullable<int> seed;

		/// <summary>Default constructor</summary>
		public Random() : base() { }

		/// <summary>Creates a Random object initialized with a seed</summary>
		/// <param name="seed">An integer for initializing the random number generator</param>
		public Random(int seed) : base(seed) { }

		/// <summary>the random seed</summary>
		public static int Seed
		{
			set {
				Console.Error.WriteLine("Set random seed to {0}.", value);
				seed = value;
				instance = new Random(seed.Value);
			}
		}

		/// <summary>Gets the instance. If it does not exist yet, it will be created.</summary>
		/// <returns>the singleton instance</returns>
		public static Random GetInstance()
		{
			if (instance == null)
			{
				if (seed == null)
					instance = new Random();
				else
					instance = new Random(seed.Value);
			}
			return instance;
		}
	}
}
