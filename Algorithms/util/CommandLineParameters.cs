// Copyright (C) 2010 Zeno Gantner
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
using System.Linq;


namespace MyMediaLite.util
{
	/// <summary>
	/// Class for command line argument processing
	/// </summary>
	public class CommandLineParameters
	{
		/// <summary>
		/// Object to store the key/value pairs
		/// </summary>
		protected Dictionary<string, string> dict;

		private NumberFormatInfo ni = new NumberFormatInfo();

		/// <summary>
		/// Create a CommandLineParameters object
		/// </summary>
		/// <param name="args">
		/// a list of strings that contains the command line parameters
		/// </param>
		/// <param name="start">ignore all parameters before this position</param>
		public CommandLineParameters(string[] args, int start)
		{
			this.dict = new Dictionary<string, string>();
			for (int i = start; i < args.Length; i++)
			{
				if (args[i].Equals(String.Empty))
					continue;

				string[] pair = args[i].Split('=');
				if (pair.Length != 2)
					throw new ArgumentException("Too many '=' in argument '" + args[i] + "'.");

				string arg_name  = pair[0];
				string arg_value = pair[1];

				if (dict.ContainsKey(arg_name))
					throw new ArgumentException(arg_name + " is used twice as an argument.");

				if (arg_value.Equals(String.Empty))
				    throw new ArgumentException(arg_name + " has an empty value.");

				if (arg_name.Equals("option_file"))
					AddArgumentsFromFile(dict, arg_value);
				else
					dict.Add(arg_name, arg_value);
			}
		}

		public CommandLineParameters(Dictionary<string,string> parameters)
		{
			this.dict = parameters;
			this.ni.NumberDecimalDigits = '.';
		}

		public int Count {
			get { return dict.Count; }
		}

		/// <summary>
		/// Check for parameters that have not been processed yet
		/// </summary>
		/// <returns>
		/// true if there are leftovers
		/// </returns>
		public bool CheckForLeftovers()
		{
			if (dict.Count != 0)
			{
				Console.WriteLine("Unknown argument '{0}'", dict.Keys.First());
				return true;
			}
			return false;
		}

		public int GetRemoveInt32(string key)
		{
			return GetRemoveInt32(key, 0);
		}

		public int GetRemoveInt32(string key, int dvalue)
		{
			if (dict.ContainsKey(key))
			{
				int value = System.Int32.Parse(dict[key]);
				dict.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		public IList<int> GetRemoveInt32List(string key)
		{
			return GetRemoveInt32List(key, ' ');
		}

		public IList<int> GetRemoveInt32List(string key, char sep)
		{
			List<int> result_list = new List<int>();
			if (dict.ContainsKey(key))
			{
				string[] numbers = dict[key].Split(sep);
				dict.Remove(key);
				foreach (string s in numbers)
					result_list.Add(System.Int32.Parse(s));
			}
			return result_list;
		}

		public uint GetRemoveUInt32(string key)
		{
			return GetRemoveUInt32(key, 0);
		}

		public uint GetRemoveUInt32(string key, uint dvalue)
		{
			if (dict.ContainsKey(key))
			{
				uint value = System.UInt32.Parse(dict[key]);
				dict.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		public double GetRemoveDouble(string key)
		{
			return GetRemoveDouble(key, 0.0);
		}

		public double GetRemoveDouble(string key, double dvalue)
		{
			ni.NumberDecimalDigits = '.';

			if (dict.ContainsKey(key))
			{
				try
				{
					double value = System.Double.Parse(dict[key], ni);
					dict.Remove(key);
					return value;
				}
				catch (FormatException)
				{
					Console.Error.WriteLine("'{0}'", dict[key]);
					throw;
				}
			}
			else
			{
				return dvalue;
			}
		}

		public string GetRemoveString(string key)
		{
			return GetRemoveString(key, String.Empty);
		}

		public string GetRemoveString(string key, string dvalue)
		{
			if (dict.ContainsKey(key))
			{
				string value = dict[key];
				dict.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		public bool GetRemoveBool(string key)
		{
			return GetRemoveBool(key, false);
		}

		public bool GetRemoveBool(string key, bool dvalue)
		{
			if (dict.ContainsKey(key))
			{
				bool value = System.Boolean.Parse(dict[key]);
				dict.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		protected void AddArgumentsFromFile(Dictionary<string, string> dict, string filename)
		{
            using ( StreamReader reader = new StreamReader(filename) )
			{
				while (!reader.EndOfStream)
				{
		           	string line = reader.ReadLine();
					if (line.Trim().Equals(String.Empty))
						continue;

		            string[] tokens = line.Split(':');
					if (tokens.Length != 2)
						throw new IOException("Expected format: 'KEY: VALUE':" + line);

					string arg_name  = tokens[0].Trim();
					string arg_value = tokens[1].Trim();

					if (arg_value.Equals(String.Empty))
					    throw new ArgumentException(arg_name + " has an empty value.");

					if (!dict.ContainsKey(arg_name)) // command line overrides argument file
						dict.Add(arg_name, arg_value);
				}
			}
		}
	}
}