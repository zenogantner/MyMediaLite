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
using System.IO;

namespace MyMediaLite.util
{
	/// <summary>
	/// Helper class for storing and loading engine models
	/// </summary>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class EngineStorage
	{
		/// <summary>
		/// Does not save if file is an empty string
		/// </summary>
		/// <param name="engine">the engine to store</param>
		/// <param name="data_dir">data directory prefix</param>
		/// <param name="file">the filename (may include relative paths)</param>
		public static void SaveModel(RecommenderEngine engine, string data_dir, string file)
		{
			if (file.Equals(String.Empty))
				return;

			string filename = Path.Combine(data_dir, file);
			Console.Error.WriteLine("Save model to {0}", filename);
			engine.SaveModel(filename);
		}

		public static void SaveModel(RecommenderEngine engine, string data_dir, string file, int iteration)
		{
			if (file.Equals(String.Empty))
				return;

			SaveModel(engine, data_dir, file + "-it-" + iteration);
		}

		public static void LoadModel(RecommenderEngine engine, string data_dir, string file)
		{
			if (file.Equals(String.Empty))
				return;

			string filename = Path.Combine(data_dir, file);
			Console.Error.WriteLine("Load model from {0}", filename);
			engine.LoadModel(filename);
		}

		public static StreamReader GetReader(string filePath, System.Type engine_type)
		{
            StreamReader reader = new StreamReader(filePath);

			if (reader.EndOfStream)
				throw new IOException("Unexpected end of file " + filePath);

			string type_name = reader.ReadLine();
			if (!type_name.Equals(engine_type.ToString()))
				Console.Error.WriteLine("WARNING: No correct type name: {0}, expected: {1}", type_name, engine_type);
			return reader;
		}

		public static StreamWriter GetWriter(string filePath, System.Type engine_type)
		{
			StreamWriter writer = new StreamWriter(filePath);
			writer.WriteLine(engine_type);
			return writer;
		}
	}
}