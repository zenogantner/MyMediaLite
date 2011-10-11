// Copyright (C) 2011 Zeno Gantner
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
using System.IO;
using System.Text;

namespace MovieDemo
{
	public sealed class IMDBAkaTitles
	{
		static public Dictionary<int, string> Read(string filename, string language, Dictionary<string, int> imdb_key_to_id)
		{
			using ( var reader = new StreamReader(filename, Encoding.GetEncoding("ISO-8859-1")) )
				return Read(reader, language, imdb_key_to_id);
		}

		static public Dictionary<int, string> Read(StreamReader reader, string language, Dictionary<string, int> imdb_key_to_id)
		{
			var aka_titles = new Dictionary<int, string>();

			string line;

			while (!reader.EndOfStream)
			{
				line = reader.ReadLine();

				if (line == "AKA TITLES LIST " + language)
				{
					line = reader.ReadLine();
					break;
				}
			}

			while (!reader.EndOfStream)
			{
			   	line = reader.ReadLine().Trim();

				// ignore empty lines
				if (line == string.Empty)
					continue;

				// ignore second (or more) aka title
				if (line.StartsWith("(aka "))
					continue;

				if (line.StartsWith("--------"))
					break;

				string imdb_key = line;

				line = reader.ReadLine().Trim();
				if (line.StartsWith("(aka "))
				{
					string[] parts = line.Split('\t');
					line = parts[0];

					string aka_title = line.Substring(5, line.Length - 6);

					int id;
					if (imdb_key_to_id.TryGetValue(imdb_key, out id))
					{
					    aka_titles[id] = aka_title;
						//Console.Error.WriteLine("{0} => {1}", line, aka_title);
					}
				}
				else
				{
					throw new IOException("aka titles should start with '(aka': " + line + " IMDB key: " + imdb_key);
				}
			}

			return aka_titles;
		}
	}
}