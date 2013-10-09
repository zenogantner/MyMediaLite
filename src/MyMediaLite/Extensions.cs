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
	}
}