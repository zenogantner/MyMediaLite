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


namespace MyMediaLite.data_type
{
	/// <summary>Tools for vector-like data</summary>
	public class VectorUtils
	{
		static public void WriteVector(StreamWriter writer, ICollection<double> vector)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

        	writer.WriteLine(vector.Count);
        	foreach (var v in vector)
               	writer.WriteLine(v.ToString(ni));
			
			writer.WriteLine();
		}

		static public ICollection<double> ReadVector(TextReader reader)
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';			
			
        	int dim = int.Parse(reader.ReadLine());

			var vector = new double[dim];

			string[] numbers;
			int i = 0;
        	while ((numbers = reader.ReadLine().Split(' ')).Length == 1 && numbers[0].Length != 0)
        	{
            	double v = System.Double.Parse(numbers[0], ni);

            	if (i >= dim)
                    throw new IOException("i = " + i + " >= " + dim);

                vector[i++] = v;
	        }

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

		/// <summary>
		/// Initialize a collection of doubles with values from a normal distribution
		/// </summary>
		/// <param name="vector">
		/// the vector to initialize
		/// </param>
		/// <param name="mean">the mean of the normal distribution</param>
		/// <param name="stdev">the standard deviation of the normal distribution</param>
        static public void InitNormal(IList<double> vector, double mean, double stdev)
        {
            var rnd = MyMediaLite.util.Random.GetInstance();
            for (int i = 0; i < vector.Count; i++)
            	vector[i] = rnd.NextNormal(mean, stdev);
        }
	}
}