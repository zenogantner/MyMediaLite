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

using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;

namespace MyMediaLite.DataType
{
	/// <summary>Extensions for vector-like data</summary>
	public static class VectorExtensions
	{
		/// <summary>Increment a vector by another one</summary>
		/// <param name='vector1'>the vector to be incremented</param>
		/// <param name='vector2'>the vector to be added to the first one</param>
		static public void Inc(this IList<double> vector1, IList<double> vector2)
		{
			for (int i = 0; i < vector1.Count; i++)
				vector1[i] += vector2[i];
		}

		/// <summary>Increment a vector by another one</summary>
		/// <param name='vector1'>the vector to be incremented</param>
		/// <param name='vector2'>the vector to be added to the first one</param>
		static public void Inc(this IList<float> vector1, IList<float> vector2)
		{
			for (int i = 0; i < vector1.Count; i++)
				vector1[i] += vector2[i];
		}

		/// <summary>Increment a vector by another one</summary>
		/// <param name='vector1'>the vector to be incremented</param>
		/// <param name='vector2'>the vector to be added to the first one</param>
		static public void Inc(this IList<double> vector1, IList<float> vector2)
		{
			for (int i = 0; i < vector1.Count; i++)
				vector1[i] += vector2[i];
		}

		/// <summary>Multiply a vector by a scalar</summary>
		/// <param name='vector'>the vector to be multiplied</param>
		/// <param name='x'>the scalar</param>
		static public void Multiply(this IList<double> vector, double x)
		{
			for (int i = 0; i < vector.Count; i++)
				vector[i] *= x;
		}

		/// <summary>Multiply a vector by a scalar</summary>
		/// <param name='vector'>the vector to be multiplied</param>
		/// <param name='x'>the scalar</param>
		static public void Multiply(this IList<float> vector, float x)
		{
			for (int i = 0; i < vector.Count; i++)
				vector[i] *= x;
		}

		/// <summary>Multiply a vector by a scalar</summary>
		/// <param name='vector'>the vector to be multiplied</param>
		/// <param name='x'>the scalar</param>
		static public void Multiply(this IList<float> vector, double x)
		{
			for (int i = 0; i < vector.Count; i++)
				vector[i] = (float) (vector[i] * x);
		}

		/// <summary>Compute scalar product (dot product) of two vectors</summary>
		/// <returns>the scalar product of the arguments</returns>
		/// <param name='v1'>the first vector</param>
		/// <param name='v2'>the second vector</param>
		static public double ScalarProduct(IList<double> v1, IList<double> v2)
		{
			double result = 0;

			for (int i = 0; i < v1.Count; i++)
				result += v1[i] * v2[i];

			return result;
		}

		/// <summary>Compute scalar product (dot product) of two vectors</summary>
		/// <returns>the scalar product of the arguments</returns>
		/// <param name='v1'>the first vector</param>
		/// <param name='v2'>the second vector</param>
		static public float ScalarProduct(IList<float> v1, IList<float> v2)
		{
			double result = 0;

			for (int i = 0; i < v1.Count; i++)
				result += v1[i] * v2[i];

			return (float) result;
		}

		/// <summary>Compute the Euclidean norm of a collection of doubles</summary>
		/// <param name="vector">the vector to compute the norm for</param>
		/// <returns>the Euclidean norm of the vector</returns>
		static public double EuclideanNorm(this ICollection<double> vector)
		{
			double sum = 0;
			foreach (double v in vector)
				sum += Math.Pow(v, 2);
			return Math.Sqrt(sum);
		}

		/// <summary>Compute the Euclidean norm of a collection of floats</summary>
		/// <param name="vector">the vector to compute the norm for</param>
		/// <returns>the Euclidean norm of the vector</returns>
		static public double EuclideanNorm(this ICollection<float> vector)
		{
			double sum = 0;
			foreach (double v in vector)
				sum += Math.Pow(v, 2);
			return Math.Sqrt(sum);
		}

		/// <summary>Compute the L1 norm of a collection of doubles</summary>
		/// <param name="vector">the vector to compute the norm for</param>
		/// <returns>the L1 norm of the vector</returns>
		static public double L1Norm(this ICollection<double> vector)
		{
			double sum = 0;
			foreach (double v in vector)
				sum += Math.Abs(v);
			return sum;
		}

		/// <summary>Initialize a collection of doubles with values from a normal distribution</summary>
		/// <param name="vector">the vector to initialize</param>
		/// <param name="mean">the mean of the normal distribution</param>
		/// <param name="stddev">the standard deviation of the normal distribution</param>
		static public void InitNormal(this IList<double> vector, double mean, double stddev)
		{
			var nd = new Normal(mean, stddev);
			nd.RandomSource = Util.Random.GetInstance();

			for (int i = 0; i < vector.Count; i++)
				vector[i] = nd.Sample();
		}

		/// <summary>Initialize a collection of floats with values from a normal distribution</summary>
		/// <param name="vector">the vector to initialize</param>
		/// <param name="mean">the mean of the normal distribution</param>
		/// <param name="stddev">the standard deviation of the normal distribution</param>
		static public void InitNormal(this IList<float> vector, double mean, double stddev)
		{
			var nd = new Normal(mean, stddev);
			nd.RandomSource = Util.Random.GetInstance();

			for (int i = 0; i < vector.Count; i++)
				vector[i] = (float) nd.Sample();
		}

		/// <summary>Initialize a collection of floats with one value</summary>
		/// <param name="vector">the vector to initialize</param>
		/// <param name="val">the value to set each element to</param>
		static public void Init(this IList<float> vector, float val)
		{
			for (int i = 0; i < vector.Count; i++)
				vector[i] = val;
		}

	}
}