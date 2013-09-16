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
	public class Help : Command
	{
		public override string Description
		{
			get {
				return "Display help";
			}
		}

		public override string Usage
		{
			get {
				return "Help for help ...";
			}
		}

		public override void Run()
		{
			Console.WriteLine("usage: mymedialite [--version] [--datadir] <command> [<args>]");
			Console.WriteLine();
			Console.WriteLine("Available commands:");
			ListCommands();
		}

		public override void Configure(string[] args)
		{
		}

		void ListCommands()
		{
			foreach (var command in GetCommands())
				Console.WriteLine(command.ToString());
		}

		IList<Command> GetCommands()
		{
			var list = new List<Command>();
			foreach (var type in Utils.GetTypes("MyMediaLite.Program"))
			{
				if (!type.IsAbstract && type.IsClass && !type.IsGenericType && type.IsSubclassOf(typeof(Command)))
				{
					var command = (Command) type.GetConstructor(new Type[] { } ).Invoke( new object[] { });
					list.Add(command);
				}
			}
			return list;
		}
	}
}
