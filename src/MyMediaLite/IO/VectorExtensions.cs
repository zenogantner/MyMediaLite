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

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MyMediaLite.IO
{
	/// <summary>Extensions for vector-like data</summary>
	public static class VectorExtensions
	{
		/// <summary>Write a collection of doubles to a TextWriter</summary>
		/// <param name="writer">a <see cref="TextWriter"/></param>
		/// <param name="vector">a collection of double values</param>
		static public void WriteVector(this TextWriter writer, ICollection<double> vector)
		{
			writer.WriteLine(vector.Count);
			foreach (var v in vector)
				writer.WriteLine(v.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>Write a collection of floats to a TextWriter</summary>
		/// <param name="writer">a <see cref="TextWriter"/></param>
		/// <param name="vector">a collection of float values</param>
		static public void WriteVector(this TextWriter writer, ICollection<float> vector)
		{
			writer.WriteLine(vector.Count);
			foreach (var v in vector)
				writer.WriteLine(v.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>Read a collection of floats from a TextReader object</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <returns>a list of double values</returns>
		static public IList<float> ReadVector(this TextReader reader)
		{
			int dim = int.Parse(reader.ReadLine());

			var vector = new float[dim];

			for (int i = 0; i < vector.Length; i++)
				vector[i] = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

			return vector;
		}

		/// <summary>Write a collection of ints to a streamwriter</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="vector">a collection of int values</param>
		static public void WriteVector(this TextWriter writer, ICollection<int> vector)
		{
			writer.WriteLine(vector.Count);
			foreach (var v in vector)
				writer.WriteLine(v.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>Read a collection of ints from a TextReader object</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <returns>a list of int values</returns>
		static public IList<int> ReadIntVector(this TextReader reader)
		{
			int dim = int.Parse(reader.ReadLine());

			var vector = new int[dim];

			for (int i = 0; i < vector.Length; i++)
				vector[i] = int.Parse(reader.ReadLine());

			return vector;
		}
	}
}