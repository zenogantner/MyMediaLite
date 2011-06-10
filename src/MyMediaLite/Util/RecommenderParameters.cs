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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyMediaLite.Util
{
	/// <summary>Class for key-value pair string processing</summary>
	[Serializable]
	public class RecommenderParameters : Dictionary<string, string>
	{
		private NumberFormatInfo ni = new NumberFormatInfo();

		/// <summary>Create a CommandLineParameters object</summary>
		/// <param name="arg_string">a string that contains the command line parameters</param>
		public RecommenderParameters(string arg_string)
		{
			IList<string> args = Regex.Split(arg_string, "\\s");

			for (int i = 0; i < args.Count; i++)
			{
				if (args[i].Equals(string.Empty))
					continue;

				string[] pair = args[i].Split('=');
				if (pair.Length != 2)
					throw new ArgumentException("Too many '=' in argument '" + args[i] + "'.");

				string arg_name  = pair[0];
				string arg_value = pair[1];

				if (this.ContainsKey(arg_name))
					throw new ArgumentException(arg_name + " is used twice as an argument.");

				if (arg_value.Length == 0)
					throw new ArgumentException(arg_name + " has an empty value.");

				this.Add(arg_name, arg_value);
			}
		}

		/// <summary>Create a CommandLineParameters object</summary>
		/// <param name="args">a list of strings that contains the command line parameters</param>
		/// <param name="start">ignore all parameters before this position</param>
		public RecommenderParameters(IList<string> args, int start)
		{
			for (int i = start; i < args.Count; i++)
			{
				if (args[i].Equals(string.Empty))
					continue;

				string[] pair = args[i].Split('=');
				if (pair.Length != 2)
					throw new ArgumentException("Too many '=' in argument '" + args[i] + "'.");

				string arg_name  = pair[0];
				string arg_value = pair[1];

				if (this.ContainsKey(arg_name))
					throw new ArgumentException(arg_name + " is used twice as an argument.");

				if (arg_value.Length == 0)
					throw new ArgumentException(arg_name + " has an empty value.");

				this.Add(arg_name, arg_value);
			}
		}

		/// <summary>Constructor</summary>
		/// <param name="parameters">a dictionary containing the parameters as key-value pairs</param>
		public RecommenderParameters(Dictionary<string,string> parameters) : base(parameters)
		{
			this.ni.NumberDecimalDigits = '.';
		}

		/// <summary>Check for parameters that have not been processed yet</summary>
		/// <returns>true if there are leftovers</returns>
		public bool CheckForLeftovers()
		{
			if (this.Count != 0)
			{
				Console.WriteLine("Unknown argument '{0}'", this.Keys.First());
				return true;
			}
			return false;
		}

		/// <summary>Get the value of an integer parameter from the collection and remove the corresponding key-value pair</summary>
		/// <param name="key">the name of the parameter</param>
		/// <returns>the value of the parameter if it exists, 0 otherwise</returns>
		public int GetRemoveInt32(string key)
		{
			return GetRemoveInt32(key, 0);
		}

		/// <summary>Get the value of an integer parameter from the collection and remove the corresponding key-value pair</summary>
		/// <param name="key">the name of the parameter</param>
		/// <param name="dvalue">the default value of the parameter</param>
		/// <returns>the value of the parameter if it exists, the default otherwise</returns>
		public int GetRemoveInt32(string key, int dvalue)
		{
			if (this.ContainsKey(key))
			{
				int value = int.Parse(this[key]);
				this.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		/// <summary>Get the values of an integer list parameter from the collection and remove the corresponding key-value pair</summary>
		/// <param name="key">the name of the parameter</param>
		/// <returns>the values of the parameter if it exists, an empty list otherwise</returns>
		public IList<int> GetRemoveInt32List(string key)
		{
			return GetRemoveInt32List(key, ' ');
		}

		/// <summary>Get the values of an integer list parameter from the collection and remove the corresponding key-value pair</summary>
		/// <param name="key">the name of the parameter</param>
		/// <param name="sep">the separator character used to split the string representation of the list</param>
		/// <returns>the values of the parameter if it exists, the default otherwise</returns>
		public IList<int> GetRemoveInt32List(string key, char sep)
		{
			var result_list = new List<int>();
			if (this.ContainsKey(key))
			{
				string[] numbers = this[key].Split(sep);
				this.Remove(key);
				foreach (string s in numbers)
					result_list.Add(int.Parse(s));
			}
			return result_list;
		}

		/// <summary>Get and remove an unsigned integer</summary>
		/// <param name="key">the parameter name</param>
		/// <returns>the value of the unsigned integer parameter, zero if it is not found</returns>
		public uint GetRemoveUInt32(string key)
		{
			return GetRemoveUInt32(key, 0);
		}

		/// <summary>Get and remove an unsigned integer</summary>
		/// <param name="key">the parameter name</param>
		/// <param name="dvalue">the default value of the parameter</param>
		/// <returns>the value of the unsigned integer parameter, dvalue if it is not found</returns>
		public uint GetRemoveUInt32(string key, uint dvalue)
		{
			if (this.ContainsKey(key))
			{
				uint value = uint.Parse(this[key]);
				this.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		/// <summary>Get a double value from the parameters</summary>
		/// <param name="key">the parameter name</param>
		/// <returns>the value of the parameter, 0 if no parameter of the given name found</returns>
		public double GetRemoveDouble(string key)
		{
			return GetRemoveDouble(key, 0.0);
		}

		/// <summary>Get a double value from the parameters</summary>
		/// <param name="key">the parameter name</param>
		/// <param name="dvalue">the default value if parameter of the given name is not found</param>
		/// <returns>the value of the parameter if it is found, the default value otherwise</returns>
		public double GetRemoveDouble(string key, double dvalue)
		{
			ni.NumberDecimalDigits = '.';

			if (this.ContainsKey(key))
				try
				{
					double value = double.Parse(this[key], ni);
					this.Remove(key);
					return value;
				}
				catch (FormatException)
				{
					Console.Error.WriteLine("'{0}'", this[key]);
					throw;
				}
			else
				return dvalue;
		}

		/// <summary>Get a float value from the parameters</summary>
		/// <param name="key">the parameter name</param>
		/// <returns>the value of the parameter, 0 if no parameter of the given name found</returns>
		public float GetRemoveFloat(string key)
		{
			return GetRemoveFloat(key, 0.0f);
		}

		/// <summary>Get a float value from the parameters</summary>
		/// <param name="key">the parameter name</param>
		/// <param name="dvalue">the default value if parameter of the given name is not found</param>
		/// <returns>the value of the parameter if it is found, the default value otherwise</returns>
		public float GetRemoveFloat(string key, float dvalue)
		{
			ni.NumberDecimalDigits = '.';

			if (this.ContainsKey(key))
				try
				{
					float value = float.Parse(this[key], ni);
					this.Remove(key);
					return value;
				}
				catch (FormatException)
				{
					Console.Error.WriteLine("'{0}'", this[key]);
					throw;
				}
			else
				return dvalue;
		}

		/// <summary>Get a string parameter</summary>
		/// <param name="key">the name of the parameter</param>
		/// <returns>the parameter value related to key, an empty string if it does not exist</returns>
		public string GetRemoveString(string key)
		{
			return GetRemoveString(key, string.Empty);
		}

		/// <summary>Get a string parameter</summary>
		/// <param name="key">the name of the parameter</param>
		/// <param name="dvalue">the default value</param>
		/// <returns>the parameter value related to key, the default value if it does not exist</returns>
		public string GetRemoveString(string key, string dvalue)
		{
			if (this.ContainsKey(key))
			{
				string value = this[key];
				this.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}

		/// <summary>Get the value of a boolean parameter from the collection and remove the corresponding key-value pair</summary>
		/// <param name="key">the name of the parameter</param>
		/// <returns>the value of the parameter if it exists, false otherwise</returns>
		public bool GetRemoveBool(string key)
		{
			return GetRemoveBool(key, false);
		}

		/// <summary>Get the value of a boolean parameter from the collection and remove the corresponding key-value pair</summary>
		/// <param name="key">the name of the parameter</param>
		/// <param name="dvalue">the default value of the parameter</param>
		/// <returns>the value of the parameter if it exists, the default otherwise</returns>
		public bool GetRemoveBool(string key, bool dvalue)
		{
			if (this.ContainsKey(key))
			{
				bool value = bool.Parse(this[key]);
				this.Remove(key);
				return value;
			}
			else
			{
				return dvalue;
			}
		}
	}
}