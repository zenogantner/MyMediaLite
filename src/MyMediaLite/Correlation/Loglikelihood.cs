// Copyright (C) 2015 Dimitris Paraschakis
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

namespace MyMediaLite.Correlation
{
	/// <summary>Loglikelihood similarity, based on Mahout implementation</summary>
	public sealed class Loglikelihood : BinaryDataSymmetricCorrelationMatrix
	{

        private float NumColumns = 0;

		/// <summary>Creates an object of type Loglikelihood</summary>
		/// <param name="num_entities">the number of entities</param>
        /// <param name="num_col">the number of columns in data matrix</param>
        public Loglikelihood(int num_entities, int num_col) : base(num_entities) {
            NumColumns = (float) num_col;
        }

		///
        protected override float ComputeCorrelationFromOverlap(float overlap, float count_x, float count_y)
		{
            double logLikelihood = logLikelihoodRatio(overlap, count_y - overlap, count_x - overlap, (NumColumns + 1) - count_x - count_y + overlap);

			if (overlap != 0)
                return (float) (1.0 - 1.0 / (1.0 + logLikelihood));
			else
				return 0.0f;
		}

        private static double logLikelihoodRatio(float k11, float k12, float k21, float k22)
        {
            if (k11 >= 0 && k12 >= 0 && k21 >= 0 && k22 >= 0)
            {
                double rowEntropy = entropy(k11 + k12, k21 + k22);
                double columnEntropy = entropy(k11 + k21, k12 + k22);
                double matrixEntropy = entropy(k11, k12, k21, k22);
                if (rowEntropy + columnEntropy < matrixEntropy)
                {
                    return 0.0;
                }
                return 2.0 * (rowEntropy + columnEntropy - matrixEntropy);
            }
            else
                return 0.0;
        }

        private static double xLogX(float x)
        {
            return x == 0 ? 0.0 : x * Math.Log(x);
        }
        private static double entropy(float a, float b)
        {
            return xLogX(a + b) - xLogX(a) - xLogX(b);
        }
        private static double entropy(float a, float b, float c, float d)
        {
            return xLogX(a + b + c + d) - xLogX(a) - xLogX(b) - xLogX(c) - xLogX(d);
        }
	}
}
