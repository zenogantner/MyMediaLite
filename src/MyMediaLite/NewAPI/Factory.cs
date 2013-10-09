// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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
using System.Reflection;

namespace MyMediaLite
{
	public class Factory
	{
		public IFactory this[string methodName]
		{
			get {
				return CreateFactory(methodName);
			}
		}

		private static IFactory CreateFactory(string typename)
		{
			if (! typename.StartsWith("MyMediaLite"))
				typename = "MyMediaLite." + typename;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(typename, false, true);
				if (type != null)
					return CreateFactory(type);
			}
			return null;
		}

		public static IFactory CreateFactory(Type type)
		{
			if (type.IsAbstract)
				return null;
			if (type.IsGenericType)
				return null;

			if (type.IsSubclassOf(typeof(IFactory)))
				return (IFactory) type.GetConstructor(new Type[] { } ).Invoke( new object[] { });
			else
				throw new Exception(type.Name + " is not an implementation of MyMediaLite.IFactory");
		}

	}
}

