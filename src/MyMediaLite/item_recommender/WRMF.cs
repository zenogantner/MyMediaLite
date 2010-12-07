// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.DataType;


namespace MyMediaLite.ItemRecommender
{
    /// <summary>
    /// Weighted matrix factorization method proposed by Hu et al. and Pan et al.
    /// </summary>
    /// <remarks>
    /// <inproceedings>
    ///   <author>Y. Hu</author> <author>Y. Koren</author> <author>C. Volinsky</author>
    ///   <title>Collaborative filtering for implicit feedback datasets</title>
    ///   <booktitle>IEEE International Conference on Data Mining (ICDM 2008)</booktitle>
    ///   <year>2008</year>
    /// </inproceedings>
    /// 
    /// <inproceedings>
    ///   <author>R. Pan</author>
    ///   <author>Y. Zhou</author>
    ///   <author>B. Cao</author>
    ///   <author>N. N. Liu</author>
    ///   <author>R. M. Lukose</author>
    ///   <author>M. Scholz</author>
    ///   <author>Q. Yang</author>
    ///   <title>One-class collaborative filtering</title>
    ///   <booktitle>IEEE International Conference on Data Mining (ICDM 2008)</booktitle>
    ///   <year>2008</year>
    /// </inproceedings>
    /// We use the fast computation method proposed by Hu et al. and we allow a global
    /// weight to penalize observed/unobserved values.
    ///
    /// This engine does not support online updates.
	/// </remarks>
    public class WRMF : MF
    {
        /// <summary>C position: the weight/confidence that is put on positive observations</summary>
		public double CPos { get { return c_pos; } set { c_pos = value;	} }
        double c_pos = 1;

        /// <summary>Regularization parameter</summary>
		public double Regularization { get { return regularization;	} set {	regularization = value;	} }
        double regularization = 0.015;

		/// <inheritdoc/>
		public override void Iterate()
		{
			// perform alternating parameter fitting
        	optimize(data_user, user_factors, item_factors);
            optimize(data_item, item_factors, user_factors);
		}

        /// <summary>
        /// Optimizes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="W">The W.</param>
        /// <param name="H">The H.</param>
        protected void optimize(SparseBooleanMatrix data, Matrix<double> W, Matrix<double> H)
        {
            Matrix<double> HH   = new Matrix<double>(num_factors, num_factors);
            Matrix<double> HCIH = new Matrix<double>(num_factors, num_factors);
            double[] HCp = new double[num_factors];

            MathNet.Numerics.LinearAlgebra.Matrix m = new MathNet.Numerics.LinearAlgebra.Matrix(num_factors, num_factors);
            MathNet.Numerics.LinearAlgebra.Matrix m_inv;

            // (1) create HH in O(f^2|I|)
            // HH is symmetric
            for (int f_1 = 0; f_1 < num_factors; f_1++)
                for (int f_2 = 0; f_2 < num_factors; f_2++)
                {
                    double d = 0;
                    for (int i = 0; i < H.dim1; i++)
                        d += H[i, f_1] * H[i, f_2];
                    HH[f_1, f_2] = d;
                }
            // (2) optimize all U
            // HCIH is symmetric
            for (int u = 0; u < W.dim1; u++)
            {
                HashSet<int> row = data[u];
                // create HCIH in O(f^2|S_u|)
                for (int f_1 = 0; f_1 < num_factors; f_1++)
                    for (int f_2 = 0; f_2 < num_factors; f_2++)
                    {
                        double d = 0;
                        foreach (int i in row)
                            d += H[i, f_1] * H[i, f_2] * (c_pos - 1);
                        HCIH[f_1, f_2] = d;
                    }
                // create HCp in O(f|S_u|)
                for (int f = 0; f < num_factors; f++)
                {
                    double d = 0;
                    foreach (int i in row)
                        d += H[i, f] * c_pos * 1;
                    HCp[f] = d;
                }
                // create m = HH + HCp + gamma*I
                // m is symmetric
                // the inverse m_inv is symmetric
                for (int f_1 = 0; f_1 < num_factors; f_1++)
                    for (int f_2 = 0; f_2 < num_factors; f_2++)
                    {
                        double d = HH[f_1, f_2] + HCIH[f_1, f_2];
                        if (f_1 == f_2)
                            d += regularization;
                        m[f_1, f_2] = d;
                    }
                m_inv = m.Inverse();
                // write back optimal W
                for (int f = 0; f < num_factors; f++)
                {
                    double d = 0;
                    for (int f_2 = 0; f_2 < num_factors; f_2++)
                        d += m_inv[f, f_2] * HCp[f_2];
                    W[u, f] = d;
                }
            }
        }

		/// <inheritdoc/>
		public override double ComputeFit()
		{
			return -1; // TODO implement
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni, "WR-MF num_factors={0} regularization={1} c_pos={2} num_iter={3} init_mean={4} init_stdev={5}",
				                 NumFactors, Regularization, CPos, NumIter, InitMean, InitStdev);
		}
    }
}