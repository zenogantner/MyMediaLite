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
using System.Linq;
using System.Text;
using MyMediaLite.data_type;
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
    /// <summary>
    /// Weighted matrix factorization method proposed by Hu et al. and Pan et al.:
    /// 
    /// Y. Hu, Y. Koren, and C. Volinsky:
    /// Collaborative filtering for implicit feedback datasets.
    /// In IEEE International Conference on Data Mining (ICDM 2008), pages 263--272, 2008.
    /// 
    /// R. Pan, Y.Zhou, B. Cao, N. N. Liu, R. M. Lukose, M. Scholz, and Q. Yang:
    /// One-class collaborative filtering.
    /// In IEEE International Conference on Data Mining (ICDM 2008), pages 502--511, 2008.
    /// 
    /// We use the fast computation method proposed by Hu et al. and we allow a global 
    /// weight to penalize observed/unobserved values.
    /// 
    /// This engine does not support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class WRMF : MF
    {
        /// <summary>
        /// Regularization parameter
        /// </summary>
        public double regularization = 0.015;
        /// <summary>
        /// C position: the weight/confidence that is put on positive observations
        /// </summary>
        public double c_pos = 1;

		/// <inheritdoc />
		public override void Iterate()
		{
        	optimize(data_user, user_feature, item_feature);
            optimize(data_item, item_feature, user_feature);
		}
		
        /// <summary>
        /// Optimizes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="W">The W.</param>
        /// <param name="H">The H.</param>
        protected void optimize(SparseBooleanMatrix data, Matrix<double> W, Matrix<double> H) 
        {
            Matrix<double> HH   = new Matrix<double>(num_features, num_features);
            Matrix<double> HCIH = new Matrix<double>(num_features, num_features);
            double[] HCp = new double[num_features];

            MathNet.Numerics.LinearAlgebra.Matrix m = new MathNet.Numerics.LinearAlgebra.Matrix(num_features, num_features);
            MathNet.Numerics.LinearAlgebra.Matrix m_inv; // = new MathNet.Numerics.LinearAlgebra.Matrix(num_feature);

            // (1) create HH in O(f^2|I|)
            // HH is symmetric
            for (int f_1 = 0; f_1 < num_features; f_1++)
            {
                for (int f_2 = 0; f_2 < num_features; f_2++)
                {
                    double d = 0;
                    for (int i = 0; i < H.dim1; i++)
                    {
                        d += H.Get(i, f_1) * H.Get(i, f_2);
                    }
                    HH.Set(f_1, f_2, d);	
                }

            }
            // (2) optimize all U
            // HCIH is symmetric
            for (int u = 0; u < W.dim1; u++)
            {
                HashSet<int> row = data.GetRow(u);
                // create HCIH in O(f^2|S_u|)
                for (int f_1 = 0; f_1 < num_features; f_1++)
                {
                    for (int f_2 = 0; f_2 < num_features; f_2++)
                    {
                        double d = 0;
                        foreach (int i in row)
                        {
                            d += H.Get(i, f_1) * H.Get(i, f_2) * (c_pos - 1);
                        }
                        HCIH.Set(f_1, f_2, d);
                    }
                }
                // create HCp in O(f|S_u|)
                for (int f = 0; f < num_features; f++)
                {
                    double d = 0;
                    foreach (int i in row)
                    {
                        d += H.Get(i, f) * c_pos * 1;
                    }
                    HCp[f] = d;
                }
                // create m = HH + HCp + gamma*I
                // m is symmetric
                // the inverse m_inv is symmetric
                for (int f_1 = 0; f_1 < num_features; f_1++)
                {
                    for (int f_2 = 0; f_2 < num_features; f_2++)
                    {
                        double d = HH.Get(f_1, f_2) + HCIH.Get(f_1, f_2);
                        if (f_1 == f_2)
                        {
                            d += regularization;
                        }
                        m[f_1, f_2] = d;
                    }
                }
                m_inv = m.Inverse();
                // write back optimal W
                for (int f = 0; f < num_features; f++)
                {
                    double d = 0;
                    for (int f_2 = 0; f_2 < num_features; f_2++)
                    {
                        d += m_inv[f, f_2] * HCp[f_2];
                    }
                    W.Set(u, f, d);
                }
            }
        }
		
		public override double ComputeFit()
		{
			return 0;
		}
		
		/// <summary>
		/// Returns the method name and the current hyperparameters
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString()
		{
			return String.Format("WR-MF num_features={0} regularization={1} c_pos={2} num_iter={3} init_f_mean={4} init_f_stdev={5}",
				                 num_features, regularization, c_pos, num_iter, init_f_mean, init_f_stdev);
		}
    }
}