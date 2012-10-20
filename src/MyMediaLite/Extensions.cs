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
using System.IO;
using System.Reflection;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite
{
	/// <summary>Helper class with utility methods for handling recommenders</summary>
	/// <remarks>
	/// Contains methods for creating and configuring recommender objects, as well as listing recommender classes.
	/// </remarks>
	public static class Extensions
	{
		static string NormalizeName(string s)
		{
			int underscore_position;
			while ((underscore_position = s.LastIndexOf('_')) != -1)
				s = s.Remove(underscore_position, 1);
			return s.ToUpperInvariant();
		}

		/// <summary>Configure a recommender</summary>
		/// <param name="recommender">the recommender to configure</param>
		/// <param name="parameters">a string containing the parameters as key-value pairs</param>
		/// <param name="report_error">void function that takes a string for error reporting</param>
		/// <returns>the configured recommender</returns>
		public static T Configure<T>(this T recommender, string parameters, Action<string> report_error)
		{
			try
			{
				Configure(recommender, new RecommenderParameters(parameters), report_error);
			}
			catch (ArgumentException e)
			{
				report_error(e.Message + "\n\n" + recommender.ToString() + "\n");
			}
			return recommender;
		}

		/// <summary>Configure a recommender</summary>
		/// <param name="recommender">the recommender to configure</param>
		/// <param name="parameters">a string containing the parameters as key-value pairs</param>
		public static T Configure<T>(this T recommender, string parameters)
		{
			return Configure(recommender, parameters, delegate(string s) { Console.Error.WriteLine(s); });
		}

		/// <summary>Configure a recommender</summary>
		/// <param name="recommender">the recommender to configure</param>
		/// <param name="parameters">a dictionary containing the parameters as key-value pairs</param>
		/// <param name="report_error">void function that takes a string for error reporting</param>
		/// <returns>the configured recommender</returns>
		public static T Configure<T>(T recommender, Dictionary<string, string> parameters, Action<string> report_error)
		{
			try
			{
				foreach (var key in new List<string>(parameters.Keys))
				{
					recommender.SetProperty(key, parameters[key], report_error);
					parameters.Remove(key);
				}
			}
			catch (Exception e)
			{
				report_error(e.Message + "\n\n" + recommender.ToString()  + "\n");
			}
			return recommender;
		}

		/// <summary>Sets a property of a MyMediaLite recommender</summary>
		/// <param name="recommender">An <see cref="IRecommender"/></param>
		/// <param name="key">the name of the property (case insensitive)</param>
		/// <param name="val">the string representation of the value</param>
		public static void SetProperty<T>(this T recommender, string key, string val)
		{
			SetProperty(recommender, key, val, delegate(string s) { Console.Error.WriteLine(s); });
		}

		/// <summary>Sets a property of a MyMediaLite recommender</summary>
		/// <param name="recommender">An <see cref="IRecommender"/></param>
		/// <param name="key">the name of the property (case insensitive)</param>
		/// <param name="val">the string representation of the value</param>
		/// <param name="report_error">delegate to report errors</param>
		public static void SetProperty<T>(this T recommender, string key, string val, Action<string> report_error)
		{
			Type type = recommender.GetType();
			var property_names = new List<string>();
			foreach (var p in type.GetProperties())
				property_names.Add(p.Name);
			property_names.Sort();

			bool property_found = false;

			key = NormalizeName(key);
			foreach (string property_name in property_names)
			{
				if (NormalizeName(property_name).StartsWith(key))
				{
					property_found = true;
					var property = type.GetProperty(property_name);

					if (property.GetSetMethod() == null)
						throw new ArgumentException(string.Format("Property '{0}' is read-only", key));

					if (property.PropertyType.IsEnum)
					{
						property.GetSetMethod().Invoke(recommender, new Object[] { Enum.Parse(property.PropertyType, val) });
						continue;
					}

					switch (property.PropertyType.ToString())
					{
						case "System.Double":
							property.GetSetMethod().Invoke(recommender, new Object[] { double.Parse(val, CultureInfo.InvariantCulture) });
							break;
						case "System.Single":
							property.GetSetMethod().Invoke(recommender, new Object[] { float.Parse(val, CultureInfo.InvariantCulture) });
							break;
						case "System.Int32":
							if (val.Equals("inf"))
								property.GetSetMethod().Invoke(recommender, new Object[] { int.MaxValue });
							else
								property.GetSetMethod().Invoke(recommender, new Object[] { int.Parse(val) });
							break;
						case "System.UInt32":
							if (val.Equals("inf"))
								property.GetSetMethod().Invoke(recommender, new Object[] { uint.MaxValue });
							else
								property.GetSetMethod().Invoke(recommender, new Object[] { uint.Parse(val) });
							break;
						case "System.Boolean":
							property.GetSetMethod().Invoke(recommender, new Object[] { bool.Parse(val) });
							break;
						case "System.String":
							property.GetSetMethod().Invoke(recommender, new Object[] { val });
							break;
						default:
							report_error(string.Format("Parameter '{0}' has unknown type '{1}'", key, property.PropertyType));
							break;
					}
				}
			}

			if (!property_found)
				report_error(string.Format("Recommender {0} does not have a parameter named '{1}'.\n{2}", type.ToString(), key, recommender));
		}

		/// <summary>Create a rating predictor from the type name</summary>
		/// <param name="typename">a string containing the type name</param>
		/// <returns>a rating recommender object of type typename if the recommender type is found, null otherwise</returns>
		public static RatingPredictor CreateRatingPredictor(this string typename)
		{
			if (! typename.StartsWith("MyMediaLite.RatingPrediction."))
				typename = "MyMediaLite.RatingPrediction." + typename;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(typename, false, true);
				if (type != null)
					return type.CreateRatingPredictor();
			}
			return null;
		}

		/// <summary>Create recommender</summary>
		/// <param name='typename'>the type name</param>
		/// <returns>a recommender of the given type name</returns>
		public static Recommender CreateRecommender(this string typename)
		{
			if (typename.StartsWith("MyMediaLite.RatingPrediction."))
				return typename.CreateRatingPredictor();
			else if (typename.StartsWith("MyMediaLite.ItemRecommendation."))
				return typename.CreateItemRecommender();
			else
				throw new IOException(string.Format("Unknown recommender namespace in type name '{0}'", typename));
		}

		/// <summary>Create a rating predictor from a type object</summary>
		/// <param name="type">the type object</param>
		/// <returns>a rating recommender object of type type</returns>
		public static RatingPredictor CreateRatingPredictor(this Type type)
		{
			if (type.IsAbstract)
				return null;
			if (type.IsGenericType)
				return null;

			if (type.IsSubclassOf(typeof(RatingPredictor)))
				return (RatingPredictor) type.GetConstructor(new Type[] { } ).Invoke( new object[] { });
			else
				throw new Exception(type.Name + " is not a subclass of MyMediaLite.RatingPrediction.RatingPredictor");
		}

		/// <summary>Create an item recommender from the type name</summary>
		/// <param name="typename">a string containing the type name</param>
		/// <returns>an item recommender object of type typename if the recommender type is found, null otherwise</returns>
		public static ItemRecommender CreateItemRecommender(this string typename)
		{
			if (! typename.StartsWith("MyMediaLite.ItemRecommendation"))
				typename = "MyMediaLite.ItemRecommendation." + typename;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(typename, false, true);
				if (type != null)
					return type.CreateItemRecommender();
			}
			return null;
		}

		/// <summary>Create an item recommender from a type object</summary>
		/// <param name="type">the type object</param>
		/// <returns>an item recommender object of type type</returns>
		public static ItemRecommender CreateItemRecommender(this Type type)
		{
			if (type.IsAbstract)
				return null;
			if (type.IsGenericType)
				return null;

			if (type.IsSubclassOf(typeof(ItemRecommender)))
				return (ItemRecommender) type.GetConstructor(new Type[] { } ).Invoke( new object[] { });
			else
				throw new Exception(type.Name + " is not a subclass of MyMediaLite.ItemRecommendation.ItemRecommender");
		}

		/// <summary>Describes the kind of data needed by this recommender</summary>
		/// <param name="recommender">a recommender</param>
		/// <returns>a string containing the additional data file arguments needed for training this recommender</returns>
		public static string Needs(this IRecommender recommender)
		{
			// determine necessary data
			var needs = new List<string>();
			if (recommender is IUserRelationAwareRecommender)
				needs.Add("--user-relations=FILE");
			if (recommender is IItemRelationAwareRecommender)
				needs.Add("--item-relations=FILE");
			if (recommender is IUserAttributeAwareRecommender)
				needs.Add("--user-attributes=FILE");
			if (recommender is IItemAttributeAwareRecommender)
				needs.Add("--item-attributes=FILE");

			return string.Join(", ", needs.ToArray());
		}

		/// <summary>Describes the kind of arguments supported by this recommender</summary>
		/// <param name="recommender">a recommender</param>
		/// <returns>a string containing the additional arguments supported by this recommender</returns>
		public static string Supports(this IRecommender recommender)
		{
			// determine necessary data
			var supports = new List<string>();
			/*
			if (recommender is IUserSimilarityProvider)
				needs.Add("");
			if (recommender is IItemSimilarityProvider)
				needs.Add("");
			*/
			if (recommender is IIterativeModel)
				supports.Add("--find-iter=N");
			if (recommender is IIncrementalItemRecommender)
				supports.Add("--online-evaluation");
			if (recommender is IIncrementalRatingPredictor)
				supports.Add("--online-evaluation");

			return string.Join(", ", supports.ToArray());
		}


		/// <summary>List all recommenders in a given namespace</summary>
		/// <param name="prefix">a string representing the namespace</param>
		/// <returns>an array of strings containing the recommender descriptions</returns>
		public static IList<string> ListRecommenders(this string prefix)
		{
			var result = new List<string>();

			foreach (Type type in Utils.GetTypes(prefix))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType && type.GetInterface("IRecommender") != null)
				{
					IRecommender recommender = prefix.Equals("MyMediaLite.RatingPrediction") ? (IRecommender) type.CreateRatingPredictor() : (IRecommender) type.CreateItemRecommender();

					string description = recommender.ToString();
					string needs = recommender.Needs();
					if (needs.Length > 0)
						description += "\n       needs " + needs;
					string supports = recommender.Supports();
					if (supports.Length > 0)
						description += "\n       supports " + supports;
					result.Add(description);
				}

			return result;
		}
	}
}