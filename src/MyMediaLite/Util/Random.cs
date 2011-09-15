// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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

namespace MyMediaLite.Util
{
	/// <summary>Random number generator singleton class</summary>
	public class Random : System.Random
	{
		private static Random instance = null;

		/// <summary>Default constructor</summary>
		public Random() : base() { }

		/// <summary>Creates a Random object initialized with a seed</summary>
		/// <param name="seed">An integer for initializing the random number generator</param>
		public Random(int seed) : base(seed) { }

		/// <summary>Initializes the instance with a given random seed</summary>
		/// <param name="seed">a seed value</param>
		public static void InitInstance(int seed)
		{
			Console.Error.WriteLine("Set random seed to {0}.", seed);
			instance = new Random(seed);
		}

		/// <summary>Gets the instance. If it does not exist yet, it will be created.</summary>
		/// <returns>the singleton instance</returns>
		public static Random GetInstance()
		{
			if (instance == null)
				instance = new Random();
			return instance;
		}
	}
}
