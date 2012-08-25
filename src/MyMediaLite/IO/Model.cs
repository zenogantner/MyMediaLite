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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;

namespace MyMediaLite.IO
{
	/// <summary>Class containing static routines for reading and writing recommender models</summary>
	public static class Model
	{
		/// <summary>Save the model parameters of a recommender to a file</summary>
		/// <remarks>
		/// Does not save if filename is an empty string.
		/// </remarks>
		/// <param name="recommender">the recommender to store</param>
		/// <param name="filename">the filename (may include relative paths)</param>
		public static void Save(IRecommender recommender, string filename)
		{
			if (filename == null)
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
		public static void Save(IRecommender recommender, string filename, int iteration)
		{
			if (filename == null)
				return;

			Save(recommender, filename + "-it-" + iteration);
		}

		/// <summary>Load the model parameters of a recommender from a file</summary>
		/// <param name="recommender">the <see cref="IRecommender"/> to load</param>
		/// <param name="filename">the filename template</param>
		public static void Load(IRecommender recommender, string filename)
		{
			Console.Error.WriteLine("Load model from {0}", filename);
			recommender.LoadModel(filename);
		}

		/// <summary>Load a recommender from a file, including object creation</summary>
		/// <param name="filename">the name of the model file</param>
		/// <returns>the recommender loaded from the file</returns>
		public static IRecommender Load(string filename)
		{
			IRecommender recommender;
			string type_name;

			using (var reader = new StreamReader(filename))
			{
				if (reader.EndOfStream)
					throw new IOException("Unexpected end of file " + filename);
				type_name = reader.ReadLine();
			}

			recommender = type_name.CreateRecommender();
			recommender.LoadModel(filename);

			return recommender;
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
			reader.ReadLine(); // read version line, and ignore it for now
			return reader;
		}

		/// <summary>Get a writer object to save the model parameters of a recommender</summary>
		/// <param name="filename">the filename of the model file</param>
		/// <param name="recommender_type">the recommender type</param>
		/// <param name="version">the version string (for backwards compatibility)</param>
		/// <returns>a <see cref="StreamWriter"/></returns>
		public static StreamWriter GetWriter(string filename, Type recommender_type, string version)
		{
			var writer = new StreamWriter(filename);
			writer.WriteLine(recommender_type);
			writer.WriteLine(version);
			return writer;
		}
	}
}