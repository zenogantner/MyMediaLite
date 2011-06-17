// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Util
{
	/// <summary>Helper class with utility methods for handling recommenders</summary>
	/// <remarks>
	/// Contains methods for storing and loading recommender models, and for configuring recommenders.
	/// </remarks>
	public class Recommender
	{
		// TODO move IO stuff to IO namespace

		/// <summary>Save the model parameters of a recommender to a file</summary>
		/// <remarks>
		/// Does not save if filename is an empty string.
		/// </remarks>
		/// <param name="recommender">the recommender to store</param>
		/// <param name="filename">the filename (may include relative paths)</param>
		public static void SaveModel(IRecommender recommender, string filename)
		{
			if (filename == string.Empty)
				return;

			Console.Error.WriteLine("Save model to {0}", filename);
			recommender.SaveModel(filename);
		}

		/// <summary>Save the model parameters of a recommender (in a given iteration of the training) to a file</summary>
		/// <remarks>
		/// Does not save if filename is an empty string.
		/// </remarks>
		/// <param name="recommender">the <see cref="IRecommender"/> to save</param>
		/// <param name="filename">the filename template</param>
		/// <param name="iteration">the iteration (will be appended to the filename)</param>
		public static void SaveModel(IRecommender recommender, string filename, int iteration)
		{
			if (filename == string.Empty)
				return;

			SaveModel(recommender, filename + "-it-" + iteration);
		}

		/// <summary>Load the model parameters of a recommender (in a given iteration of the training) from a file</summary>
		/// <remarks>
		/// Does not load model if filename is an empty string.
		/// </remarks>
		/// <param name="recommender">the <see cref="IRecommender"/> to save</param>
		/// <param name="filename">the filename template</param>
		public static void LoadModel(IRecommender recommender, string filename)
		{
			if (filename  == string.Empty)
				return;

			Console.Error.WriteLine("Load model from {0}", filename);
			recommender.LoadModel(filename);
		}

		/// <summary>Get a reader object to read in model parameters of a recommender</summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="recommender_type">the expected recommender type</param>
		/// <returns>a <see cref="StreamReader"/></returns>
		public static StreamReader GetReader(string filename, Type recommender_type)
		{
			var reader = new StreamReader(filename);

			if (reader.EndOfStream)
				throw new IOException("Unexpected end of file " + filename);

			string type_name = reader.ReadLine();
			if (!type_name.Equals(recommender_type.ToString()))
				Console.Error.WriteLine("WARNING: No correct type name: {0}, expected: {1}", type_name, recommender_type);
			return reader;
		}

		/// <summary>Get a writer object to save the model parameters of a recommender</summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="recommender_type">the recommender type</param>
		/// <returns>a <see cref="StreamWriter"/></returns>
		public static StreamWriter GetWriter(string filename, Type recommender_type)
		{
			var writer = new StreamWriter(filename);
			writer.WriteLine(recommender_type);
			return writer;
		}

		static string NormalizeName(string s)
		{
			int underscore_position;
			while ((underscore_position = s.LastIndexOf('_')) != -1)
				s = s.Remove(underscore_position, 1);
			return s.ToUpperInvariant();
		}

		/// <summary>Delegate definition necessary to define Configure</summary>
		public delegate void takes_string(string s);

		/// <summary>Configure a recommender</summary>
		/// <param name="recommender">the recommender to configure</param>
		/// <param name="parameters">a string containing the parameters as key-value pairs</param>
		/// <param name="report_error">void function that takes a string for error reporting</param>
		/// <returns>the configured recommender</returns>
		public static T Configure<T>(T recommender, string parameters, takes_string report_error)
		{
			var parameters_dictionary = new RecommenderParameters(parameters);
			return Configure(recommender, parameters_dictionary, report_error);
		}

		/// <summary>Configure a recommender</summary>
		/// <param name="recommender">the recommender to configure</param>
		/// <param name="parameters">a string containing the parameters as key-value pairs</param>
		public static T Configure<T>(T recommender, string parameters)
		{
			return Configure(recommender, parameters, delegate(string s) { Console.Error.WriteLine(s); });
		}

		/// <summary>Configure a recommender</summary>
		/// <param name="recommender">the recommender to configure</param>
		/// <param name="parameters">a dictionary containing the parameters as key-value pairs</param>
		/// <param name="report_error">void function that takes a string for error reporting</param>
		/// <returns>the configured recommender</returns>
		public static T Configure<T>(T recommender, Dictionary<string, string> parameters, takes_string report_error)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			Type type = recommender.GetType();
			var property_names = new List<string>();
			foreach (var p in type.GetProperties())
				property_names.Add(p.Name);
			property_names.Sort();

			// TODO consider using SetProperty
			foreach (var key in new List<string>(parameters.Keys))
			{
				string param_name = NormalizeName(key);
				foreach (string property_name in property_names)
				{
					if (NormalizeName(property_name).StartsWith(param_name))
					{
						var property = type.GetProperty(property_name);

						if (!property.CanWrite)
							throw new Exception(string.Format("Property '{0}' is read-only.", property.Name));

						if (property.GetSetMethod() == null)
							goto NEXT_PROPERTY; // poor man's labeled break ...

						switch (property.PropertyType.ToString())
						{
							case "System.Double":
								property.GetSetMethod().Invoke(recommender, new Object[] { double.Parse(parameters[key], ni) });
								break;
							case "System.Single":
								property.GetSetMethod().Invoke(recommender, new Object[] { float.Parse(parameters[key], ni) });
								break;
							case "System.Int32":
								if (parameters[key].Equals("inf"))
									property.GetSetMethod().Invoke(recommender, new Object[] { int.MaxValue });
								else
									property.GetSetMethod().Invoke(recommender, new Object[] { int.Parse(parameters[key]) });
								break;
							case "System.UInt32":
								if (parameters[key].Equals("inf"))
									property.GetSetMethod().Invoke(recommender, new Object[] { uint.MaxValue });
								else
									property.GetSetMethod().Invoke(recommender, new Object[] { uint.Parse(parameters[key]) });
								break;
							case "System.Boolean":
								property.GetSetMethod().Invoke(recommender, new Object[] { bool.Parse(parameters[key]) });
								break;
							case "System.String":
								property.GetSetMethod().Invoke(recommender, new Object[] { parameters[key] });
								break;
							default:
								report_error(string.Format("Parameter '{0}' has unknown type '{1}'", key, property.PropertyType));
								break;
						}
						parameters.Remove(key);
						goto NEXT_KEY; // poor man's labeled break ...
					}

					NEXT_PROPERTY:
					Console.Write(string.Empty); // the C# compiler wants some statement here
				}

				report_error(string.Format("Recommender {0} does not have a parameter named '{1}'.\n{2}", type.ToString(), key, recommender));

				NEXT_KEY:
				Console.Write(string.Empty); // the C# compiler wants some statement here
			}

			return recommender;
		}

		/// <summary>Sets a property of a MyMediaLite recommender</summary>
		/// <param name="recommender">An <see cref="IRecommender"/></param>
		/// <param name="key">the name of the property (case insensitive)</param>
		/// <param name="val">the string representation of the value</param>
		public static void SetProperty(IRecommender recommender, string key, string val)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			Type type = recommender.GetType();
			var property_names = new List<string>();
			foreach (var p in type.GetProperties())
				property_names.Add(p.Name);
			property_names.Sort();

			key = NormalizeName(key);
			foreach (string property_name in property_names)
			{
				if (NormalizeName(property_name).StartsWith(key))
				{
					var property = type.GetProperty(property_name);

					if (property.GetSetMethod() == null)
						throw new ArgumentException(string.Format("Parameter '{0}' has no setter", key));

					switch (property.PropertyType.ToString())
					{
						case "System.Double":
							property.GetSetMethod().Invoke(recommender, new Object[] { double.Parse(val, ni) });
							break;
						case "System.Single":
							property.GetSetMethod().Invoke(recommender, new Object[] { float.Parse(val, ni) });
							break;
						case "System.Int32":
							property.GetSetMethod().Invoke(recommender, new Object[] { int.Parse(val) });
							break;
						case "System.UInt32":
							property.GetSetMethod().Invoke(recommender, new Object[] { uint.Parse(val) });
							break;
						case "System.Boolean":
							property.GetSetMethod().Invoke(recommender, new Object[] { bool.Parse(val) });
							break;
						default:
							throw new ArgumentException(string.Format("Parameter '{0}' has unknown type '{1}'", key, property.PropertyType));
					}
				}
			}
		}

		/// <summary>Create a rating predictor from the type name</summary>
		/// <param name="typename">a string containing the type name</param>
		/// <returns>a rating recommender object of type typename if the recommender type is found, null otherwise</returns>
		public static RatingPrediction.RatingPredictor CreateRatingPredictor(string typename)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType("MyMediaLite.RatingPrediction." + typename, false, true);
				if (type != null)
					return CreateRatingPredictor(type);
			}
			return null;
		}

		/// <summary>Create a rating predictor from a type object</summary>
		/// <param name="type">the type object</param>
		/// <returns>a rating recommender object of type type</returns>
		public static RatingPrediction.RatingPredictor CreateRatingPredictor(Type type)
		{
			if (type.IsAbstract)
				return null;
			if (type.IsGenericType)
				return null;

			if (type.IsSubclassOf(typeof(RatingPrediction.RatingPredictor)))
				return (RatingPrediction.RatingPredictor) type.GetConstructor(new Type[] { } ).Invoke( new object[] { });
			else
				throw new Exception(type.Name + " is not a subclass of MyMediaLite.RatingPrediction.RatingPredictor");
		}

		/// <summary>Create an item recommender from the type name</summary>
		/// <param name="typename">a string containing the type name</param>
		/// <returns>an item recommender object of type typename if the recommender type is found, null otherwise</returns>
		public static ItemRecommendation.ItemRecommender CreateItemRecommender(string typename)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType("MyMediaLite.ItemRecommendation." + typename, false, true);
				if (type != null)
					return CreateItemRecommender(type);
			}
			return null;
		}

		/// <summary>Create an item recommender from a type object</summary>
		/// <param name="type">the type object</param>
		/// <returns>an item recommender object of type type</returns>
		public static ItemRecommendation.ItemRecommender CreateItemRecommender(Type type)
		{
			if (type.IsAbstract)
				return null;
			if (type.IsGenericType)
				return null;

			if (type.IsSubclassOf(typeof(ItemRecommendation.ItemRecommender)))
				return (ItemRecommendation.ItemRecommender) type.GetConstructor(new Type[] { } ).Invoke( new object[] { });
			else
				throw new Exception(type.Name + " is not a subclass of MyMediaLite.ItemRecommendation.ItemRecommender");
		}

		/// <summary>Describes the kind of data needed by this recommender</summary>
		/// <param name="recommender">a recommender</param>
		/// <returns>a string containing the additional datafiles needed for training this recommender</returns>
		public static string Needs(IRecommender recommender)
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

		/// <summary>List all recommenders in a given namespace</summary>
		/// <param name="prefix">a string representing the namespace</param>
		/// <returns>an array of strings containing the recommender descriptions</returns>
		public static string[] List(string prefix)
		{
			var result = new List<string>();

			foreach (Type type in Utils.GetTypesInNamespace(prefix))
				if (!type.IsAbstract && !type.IsInterface && !type.IsEnum && !type.IsGenericType)
				{
					IRecommender recommender = prefix.Equals("MyMediaLite.RatingPrediction") ? (IRecommender) Recommender.CreateRatingPredictor(type) : (IRecommender) Recommender.CreateItemRecommender(type);

					string description = recommender.ToString();
					string needs = Recommender.Needs(recommender);
					if (needs.Length > 0)
						description += " (needs " + needs + ")";
					result.Add(description);
				}

			return result.ToArray();
		}
	}
}