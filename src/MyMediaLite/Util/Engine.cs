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
using MyMediaLite;

namespace MyMediaLite.Util
{
	/// <summary>Helper class with utility methods for recommender engines</summary>
	/// <remarks>
	/// Contains methods for storing and loading engine models, and for configuring engines.
	/// </remarks>
	public class Engine
	{
		/// <summary>Save the model parameters of a recommender engine to a file</summary>
		/// <remarks>
		/// Does not save if file is an empty string
		/// </remarks>
		/// <param name="engine">the engine to store</param>
		/// <param name="filename">the filename (may include relative paths)</param>
		public static void SaveModel(IRecommenderEngine engine, string filename)
		{
			if (filename.Equals(string.Empty))
				return;

			Console.Error.WriteLine("Save model to {0}", filename);
			engine.SaveModel(filename);
		}

		/// <summary>Save the model parameters of a recommender engine (in a given iteration of the training) to a file</summary>
		/// <param name="engine">the <see cref="IRecommenderEngine"/> to save</param>
		/// <param name="filename">the filename template</param>
		/// <param name="iteration">the iteration (will be appended to the filename)</param>
		public static void SaveModel(IRecommenderEngine engine, string filename, int iteration)
		{
			if (filename.Equals(string.Empty))
				return;

			SaveModel(engine, filename + "-it-" + iteration);
		}

		/// <summary>Load the model parameters of a recommender engine (in a given iteration of the training) from a file</summary>
		/// <param name="engine">the <see cref="IRecommenderEngine"/> to save</param>
		/// <param name="filename">the filename template</param>
		public static void LoadModel(IRecommenderEngine engine, string filename)
		{
			if (filename.Equals(string.Empty))
				return;

			Console.Error.WriteLine("Load model from {0}", filename);
			engine.LoadModel(filename);
		}

		/// <summary>Get a reader object to read in model parameters of a recommender engine</summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="engine_type">the expected engine type</param>
		/// <returns>a <see cref="StreamReader"/></returns>
		public static StreamReader GetReader(string filename, System.Type engine_type)
		{
            var reader = new StreamReader(filename);

			if (reader.EndOfStream)
				throw new IOException("Unexpected end of file " + filename);

			string type_name = reader.ReadLine();
			if (!type_name.Equals(engine_type.ToString()))
				Console.Error.WriteLine("WARNING: No correct type name: {0}, expected: {1}", type_name, engine_type);
			return reader;
		}

		/// <summary>Get a writer object to save the model parameters of a recommender engine</summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="engine_type">the engine type</param>
		/// <returns>a <see cref="StreamWriter"/></returns>
		public static StreamWriter GetWriter(string filename, System.Type engine_type)
		{
			var writer = new StreamWriter(filename);
			writer.WriteLine(engine_type);
			return writer;
		}

		static string NormalizeName(string s)
		{
			int underscore_position;
			while ((underscore_position = s.LastIndexOf('_')) != -1)
				s = s.Remove(underscore_position, 1);
			return s.ToUpperInvariant();
		}

		/// <summary>Delegate definition necessary to define ConfigureEngine</summary>
		public delegate void takes_string(string s);

		/// <summary>Configure a recommender engine</summary>
		/// <param name="engine">the recommender engine to configure</param>
		/// <param name="parameters">a dictionary containing the parameters as key-value pairs</param>
		/// <param name="report_error">void function that takes a string for error reporting</param>
		/// <returns>the configured recommender engine</returns>
		public static T Configure<T>(T engine, Dictionary<string, string> parameters, takes_string report_error)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			Type type = engine.GetType();
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

						if (property.GetSetMethod() == null)
							goto NEXT_PROPERTY; // poor man's labeled break ...

						switch (property.PropertyType.ToString())
						{
							case "System.Double":
						    	property.GetSetMethod().Invoke(engine, new Object[] { double.Parse(parameters[key], ni) });
								break;
							case "System.Single":
						    	property.GetSetMethod().Invoke(engine, new Object[] { float.Parse(parameters[key], ni) });
								break;
							case "System.Int32":
								if (parameters[key].Equals("inf"))
									property.GetSetMethod().Invoke(engine, new Object[] { int.MaxValue });
								else
						    		property.GetSetMethod().Invoke(engine, new Object[] { int.Parse(parameters[key]) });
								break;
							case "System.UInt32":
								if (parameters[key].Equals("inf"))
									property.GetSetMethod().Invoke(engine, new Object[] { uint.MaxValue });
								else
						    		property.GetSetMethod().Invoke(engine, new Object[] { uint.Parse(parameters[key]) });
								break;
							case "System.Boolean":
						    	property.GetSetMethod().Invoke(engine, new Object[] { bool.Parse(parameters[key]) });
								break;
							case "System.String":
						    	property.GetSetMethod().Invoke(engine, new Object[] { parameters[key] });
								break;							
							default:
								report_error(string.Format("Parameter '{0}' has unknown type '{1}'", key, property.PropertyType));
								break;
						}
						parameters.Remove(key);
						goto NEXT_KEY; // poor man's labeled break ...
					}

					NEXT_PROPERTY:
					Console.Write(""); // the C# compiler wants some statement here
				}

				report_error(string.Format("Engine {0} does not have a parameter named '{1}'.\n{2}", type.ToString(), key, engine));

				NEXT_KEY:
				Console.Write(""); // the C# compiler wants some statement here
			}

			return engine;
		}

		/// <summary>Sets a property of a MyMediaLite recommender engine</summary>
		/// <param name="engine">An <see cref="IRecommenderEngine"/></param>
		/// <param name="key">the name of the property (case insensitive)</param>
		/// <param name="val">the string representation of the value</param>
		public static void SetProperty(IRecommenderEngine engine, string key, string val)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			Type type = engine.GetType();
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
					    	property.GetSetMethod().Invoke(engine, new Object[] { double.Parse(val, ni) });
							break;
						case "System.Single":
					    	property.GetSetMethod().Invoke(engine, new Object[] { float.Parse(val, ni) });
							break;
						case "System.Int32":
					    	property.GetSetMethod().Invoke(engine, new Object[] { int.Parse(val) });
							break;
						case "System.UInt32":
					    	property.GetSetMethod().Invoke(engine, new Object[] { uint.Parse(val) });
							break;
						case "System.Boolean":
					    	property.GetSetMethod().Invoke(engine, new Object[] { bool.Parse(val) });
							break;
						default:
							throw new ArgumentException(string.Format("Parameter '{0}' has unknown type '{1}'", key, property.PropertyType));
					}
				}
			}
		}
	}
}