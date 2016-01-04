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
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MyMediaLite.DataType;
using MyMediaLite.Data;
using System.Linq;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Regularized single-element-based non-negative matrix factorization (RSNMF)</summary>
    /// <remarks>
    ///   <para>
    ///     Literature:
    ///     <list type="bullet">
    ///       <item><description>
    ///         Luo et al. (2014): "An efficient non-negative matrix-factorization-based approach to collaborative filtering for recommender systems".
    ///         IEEE Transaction and Industrial Informatics, Vol. 10, No. 2, 2014
    ///       </description></item>
    ///   </para>
    /// </remarks>
	public class RSNMF : MatrixFactorization
	{
        private Matrix<float> P;
        private Matrix<float> Q;

        /// <summary>Regularization parameter</summary>
        public float Lambda { get { return lambda; } set { lambda = value; } }
        float lambda = 0.12f;

		public RSNMF() {}

        public override void Train()
        {            
            InitModelNonNegative();
            P = user_factors;
            Q = (Matrix<float>) item_factors.Transpose();

            Matrix<float> UserUP = new Matrix<float>(P);
            Matrix<float> UserDOWN = new Matrix<float>(P);
            Matrix<float> ItemUP = new Matrix<float>(Q);
            Matrix<float> ItemDOWN = new Matrix<float>(Q);

            for (int iter = 0; iter < NumIter; iter++)
            {
                UserUP.InitZeros();
                UserDOWN.InitZeros();
                ItemUP.InitZeros();
                ItemDOWN.InitZeros();

                Parallel.For(0, Ratings.Count, index =>
                {
                    int u = Ratings.Users[index];
                    int i = Ratings.Items[index];

                    float R_ui = Ratings[index];
                    float R_ui_hat = R_hat(u, i);
                    for (int k = 0; k < NumFactors; k++)
                    {
                        UserUP[u, k] += Q[k, i] * R_ui;
                        UserDOWN[u, k] += Q[k, i] * R_ui_hat;
                        ItemUP[k, i] += P[u, k] * R_ui;
                        ItemDOWN[k, i] += P[u, k] * R_ui_hat;
                    }
                });

                foreach (int u in Ratings.Users.Distinct())
                {
                    int I_u = Ratings.ByUser[u].Count;
                    for (int k = 0; k < NumFactors; k++)
                    {
                        UserDOWN[u, k] += I_u * lambda * P[u, k];
                        P[u, k] *= UserUP[u, k] / (UserDOWN[u, k]);
                    }
                }

                foreach (int i in Ratings.Items.Distinct())
                {
                    int U_i = Ratings.ByItem[i].Count;
                    for (int k = 0; k < NumFactors; k++)
                    {
                        ItemDOWN[k, i] += U_i * lambda * Q[k, i];
                        Q[k, i] *= ItemUP[k, i] / (ItemDOWN[k, i]);
                    }
                }

                /*
                // Evaluate objective function
                float sum_error = 0;

                //Parallel.For(0, Ratings.Count, index =>
                for (int index = 0; index < Ratings.Count; index++)
                {
                    int u = Ratings.Users[index];
                    int i = Ratings.Items[index];

                    float R_ui = Ratings[index];

                    float R_ui_hat = 0;
                    float p_2 = 0;
                    float q_2 = 0;

                    for (int k = 0; k < NumFactors; k++)
                    {
                        R_ui_hat += P[u, k] * Q[k, i];
                        p_2 += (float)Math.Pow(P[u, k], 2);
                        q_2 += (float)Math.Pow(Q[k, i], 2);
                    }
                    sum_error += (float)Math.Pow((R_ui - R_ui_hat), 2) + lambda * p_2 + lambda * q_2;
                }
                Console.WriteLine("Iteration {0}:\t{1}", iter, sum_error);
                */

            }
            user_factors = P;
            item_factors = (Matrix<float>) Q.Transpose();
        }
        
        private float R_hat(int u, int i)
        {
            float result = 0;
            for (int k = 0; k < NumFactors; k++)
            {
                result += P[u, k] * Q[k, i];
            }
            return result;
        }

        public override float Predict(int user_id, int item_id)
        {
            return base.Predict(user_id, item_id);
        }

		///
		public override float ComputeObjective()
		{
			return -1;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"RSNMF num_factors={0} num_iter={1} lambda={2}",
				NumFactors, NumIter, Lambda);
		}
	}
}