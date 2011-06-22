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

namespace MyMediaLite.DataType
{
	/// <summary>Tools for vector-like data</summary>
	public class VectorUtils
	{
		/// <summary>Write a collection of doubles to a streamwriter</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="vector">a collection of double values</param>
		static public void WriteVector(StreamWriter writer, ICollection<double> vector)
		{
			writer.WriteLine(vector.Count);
			foreach (var v in vector)
			   	writer.WriteLine(v.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>Read a collection of doubles from a TextReader object</summary>
		/// <param name="reader">the <see cref="TextReader"/> to read from</param>
		/// <returns>a list of double values</returns>
		static public IList<double> ReadVector(TextReader reader)
		{
			int dim = int.Parse(reader.ReadLine());

			var vector = new double[dim];

			for (int i = 0; i < vector.Length; i++)
				vector[i] = double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);

			return vector;
		}

		/// <summary>Compute the Euclidean norm of a collection of doubles</summary>
		/// <param name="vector">the vector to compute the norm for</param>
		/// <returns>the Euclidean norm of the vector</returns>
		public static double EuclideanNorm(ICollection<double> vector)
		{
			double sum = 0;
			foreach (double v in vector)
				sum += Math.Pow(v, 2);
			return Math.Sqrt(sum);
		}

		/// <summary>Compute the L1 norm of a collection of doubles</summary>
		/// <param name="vector">the vector to compute the norm for</param>
		/// <returns>the L1 norm of the vector</returns>
		public static double L1Norm(ICollection<double> vector)
		{
			double sum = 0;
			foreach (double v in vector)
				sum += Math.Abs(v);
			return sum;
		}

		/// <summary>Initialize a collection of doubles with values from a normal distribution</summary>
		/// <param name="vector">the vector to initialize</param>
		/// <param name="mean">the mean of the normal distribution</param>
		/// <param name="stdev">the standard deviation of the normal distribution</param>
		static public void InitNormal(IList<double> vector, double mean, double stdev)
		{
			var rnd = MyMediaLite.Util.Random.GetInstance();
			for (int i = 0; i < vector.Count; i++)
				vector[i] = rnd.NextNormal(mean, stdev);
		}
	}
}