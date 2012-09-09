// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Reflection;
using MyMediaLite.GroupRecommendation;

namespace MyMediaLite.GroupRecommendation
{
	/// <summary>Class containing utility functions for group recommenders</summary>
	public static class Extensions
	{
		/// <summary>Create a group recommender from the type name</summary>
		/// <param name="typename">a string containing the type name</param>
		/// <param name="recommender">the underlying recommender</param>
		/// <returns>a group recommender object of type typename if the recommender type is found, null otherwise</returns>
		public static GroupRecommender CreateGroupRecommender(this string typename, IRecommender recommender)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType("MyMediaLite.GroupRecommendation." + typename, false, true);
				if (type != null)
					return CreateGroupRecommender(type, recommender);
			}
			return null;
		}

		/// <summary>Create a group recommender from a type object</summary>
		/// <param name="type">the type object</param>
		/// <param name="recommender">the underlying recommender</param>
		/// <returns>a group recommender object of type type</returns>
		public static GroupRecommender CreateGroupRecommender(this Type type, IRecommender recommender)
		{
			if (type.IsAbstract)
				return null;
			if (type.IsGenericType)
				return null;

			if (recommender == null)
				throw new ArgumentNullException("recommender");
			if (type == null)
				throw new ArgumentNullException("type");
			
			if (type.IsSubclassOf(typeof(GroupRecommender)))
				return (GroupRecommender) type.GetConstructor(new Type[] { } ).Invoke( new object[] { recommender });
			else
				throw new Exception(type.Name + " is not a subclass of MyMediaLite.GroupRecommendation.GroupRecommender");
		}
	}
}
