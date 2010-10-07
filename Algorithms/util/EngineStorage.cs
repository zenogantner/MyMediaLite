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

		/// <summary>
		/// Save the model parameters of a recommender engine (in a given iteration of the training) to a file
		/// </summary>
		/// <param name="engine">the <see cref="RecommenderEngine"/> to save</param>
		/// <param name="data_dir">the directory where the file will  be stored</param>
		/// <param name="filename">the filename template</param>
		/// <param name="iteration">the iteration (will be appended to the filename)</param>
		public static void SaveModel(RecommenderEngine engine, string data_dir, string filename, int iteration)
		{
			if (filename.Equals(String.Empty))
				return;

			SaveModel(engine, data_dir, filename + "-it-" + iteration);
		}

		/// <summary>
		/// Save the model parameters of a recommender engine (in a given iteration of the training) to a file
		/// </summary>
		/// <param name="engine">the <see cref="RecommenderEngine"/> to save</param>
		/// <param name="data_dir">the directory where the file will  be stored</param>
		/// <param name="filename">the filename template</param>
		public static void LoadModel(RecommenderEngine engine, string data_dir, string filename)
		{
			if (filename.Equals(String.Empty))
				return;

			filename = Path.Combine(data_dir, filename);
			Console.Error.WriteLine("Load model from {0}", filename);
			engine.LoadModel(filename);
		}

		/// <summary>
		/// Get a reader object to read in model parameters of a recommender engine
		/// </summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="engine_type">the expected engine type</param>
		/// <returns>a <see cref="StreamReader"/></returns>
		public static StreamReader GetReader(string filename, System.Type engine_type)
		{
            StreamReader reader = new StreamReader(filename);

			if (reader.EndOfStream)
				throw new IOException("Unexpected end of file " + filename);

			string type_name = reader.ReadLine();
			if (!type_name.Equals(engine_type.ToString()))
				Console.Error.WriteLine("WARNING: No correct type name: {0}, expected: {1}", type_name, engine_type);
			return reader;
		}

		/// <summary>
		/// Get a writer object to save the model parameters of a recommender engine
		/// </summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="engine_type">the engine type</param>
		/// <returns>a <see cref="StreamWriter"/></returns>
		public static StreamWriter GetWriter(string filename, System.Type engine_type)
		{
			StreamWriter writer = new StreamWriter(filename);
			writer.WriteLine(engine_type);
			return writer;
		}
	}
}