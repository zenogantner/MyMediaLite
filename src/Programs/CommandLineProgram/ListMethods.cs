// Copyright (C) 2013 Zeno Gantner
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
using System.Linq;
using MyMediaLite;

namespace MyMediaLite.Program
{
	public class ListMethods : Command
	{
		public override string Description
		{
			get {
				return "List methods available in MyMediaLite";
			}
		}

		public override string Usage
		{
			get {
				return "list-methods";
			}
		}

		public override void Run()
		{
			Console.WriteLine("Available methods:");
			foreach (string method in GetMethodList())
				Console.WriteLine("  " + method);
		}

		public override void Configure(string[] args)
		{
		}

		public static IList<string> GetMethodList()
		{
			var result = new List<string>();

			foreach (Type type in Utils.GetTypes("MyMediaLite"))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IMethod") != null)
				{
					string description = type.Name;
					result.Add(description);
				}

			return result;
		}

	}
}
