// Copyright (C) 2011, 2012, 2013 Zeno Gantner
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
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Weighted matrix factorization method proposed by Hu et al. and Pan et al.</summary>
	/// <remarks>
	///   <para>
	///     We use the fast learning method proposed by Hu et al. (alternating least squares, ALS),
	///     and we use a global parameter to give observed values higher weights.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Y. Hu, Y. Koren, C. Volinsky: Collaborative filtering for implicit feedback datasets.
	///         ICDM 2008.
	///         http://research.yahoo.net/files/HuKorenVolinsky-ICDM08.pdf
	///       </description></item>
	///       <item><description>
	///         R. Pan, Y. Zhou, B. Cao, N. N. Liu, R. M. Lukose, M. Scholz, Q. Yang:
	///         One-class collaborative filtering,
	///         ICDM 2008.
	///         http://www.hpl.hp.com/techreports/2008/HPL-2008-48R1.pdf
	///       </description></item>
	///     </list>
	///   </para>
	///   <para>
	///     This recommender supports incremental updates.
	///   </para>
	/// </remarks>
	public class WRMF : MF
	{
		/// <summary>parameter for the weight/confidence that is put on positive observations</summary>
		public double Alpha { get { return alpha; } set { alpha = value; } }
		double alpha = 1;

		/// <summary>Regularization parameter</summary>
		public double Regularization { get { return regularization; } set { regularization = value; } }
		double regularization = 0.015;

		///
		public WRMF()
		{
			NumIter = 15;
		}

		///
		public override void Iterate()
		{
			// perform alternating parameter fitting
			Optimize(EntityType.USER);
			Optimize(EntityType.ITEM);
		}

		/// <summary>Optimizes the specified data</summary>
		/// <param name="target">the optimization target, USER or ITEM</param>
		protected virtual void Optimize(EntityType target)
		{
			if (target != EntityType.USER && target != EntityType.ITEM)
				throw new ArgumentException();

			int num_entities = target == EntityType.USER ? user_factors.dim1 : item_factors.dim1;
			
			// (1) create HH in O(f^2|Items|)
			Matrix<float> H = target == EntityType.USER ? item_factors : user_factors;
			var HH = ComputeSquareMatrix(H);
			// (2) optimize all U
			Parallel.For(
				0,
				num_entities,
				u => { Optimize(u, target, HH); }
			);
		}

		private Matrix<double> ComputeSquareMatrix(Matrix<float> m)
		{
			var mm = new Matrix<double>(m.dim2, m.dim2);
			// mm is symmetric
			for (int f_1 = 0; f_1 < m.dim2; f_1++)
				for (int f_2 = f_1; f_2 < m.dim2; f_2++)
				{
					double d = 0;
					for (int i = 0; i < m.dim1; i++)
						d += m[i, f_1] * m[i, f_2];
					mm[f_1, f_2] = d;
					mm[f_2, f_1] = d;
				}
			return mm;
		}

		private void Optimize(int u, EntityType target, Matrix<double> HH)
		{
			if (target != EntityType.USER && target != EntityType.ITEM)
				throw new ArgumentException();
			
			Matrix<float> W = target == EntityType.USER ? user_factors : item_factors;
			Matrix<float> H = target == EntityType.USER ? item_factors : user_factors;

			//ICollection<int> row = target == EntityType.USER ? Interactions.ByUser(u).Items : Interactions.ByItem(u).Users;
			ICollection<int> row = target == EntityType.USER ? Feedback.UserMatrix.GetEntriesByRow(u) : Feedback.ItemMatrix.GetEntriesByRow(u);
			
			// comments are in terms of computing the user factors
			// ... works the same with users and items exchanged

			// HC_minus_IH is symmetric
			// create HC_minus_IH in O(f^2|S_u|)
			var HC_minus_IH = new Matrix<double>(num_factors, num_factors);
			for (int f_1 = 0; f_1 < num_factors; f_1++)
				for (int f_2 = f_1; f_2 < num_factors; f_2++)
				{
					double d = 0;
					foreach (int i in row)
						d += H[i, f_1] * H[i, f_2];
					HC_minus_IH[f_1, f_2] = d * alpha;
					HC_minus_IH[f_2, f_1] = d * alpha;
				}
			// create HCp in O(f|S_u|)
			var HCp = new double[num_factors];
			for (int f = 0; f < num_factors; f++)
			{
				double d = 0;
				foreach (int i in row)
					d += H[i, f];
				HCp[f] = d * (1 + alpha);
			}
			// create m = HH + HC_minus_IH + reg*I
			// m is symmetric
			// the inverse m_inv is symmetric
			var m = new DenseMatrix(num_factors, num_factors);
			for (int f_1 = 0; f_1 < num_factors; f_1++)
				for (int f_2 = f_1; f_2 < num_factors; f_2++)
				{
					double d = HH[f_1, f_2] + HC_minus_IH[f_1, f_2];
					if (f_1 == f_2)
						d += regularization;
					m[f_1, f_2] = d;
					m[f_2, f_1] = d;
				}
			var m_inv = m.Inverse();
			// write back optimal W
			for (int f = 0; f < num_factors; f++)
			{
				double d = 0;
				for (int f_2 = 0; f_2 < num_factors; f_2++)
					d += m_inv[f, f_2] * HCp[f_2];
				W[u, f] = (float) d;
			}
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
				"WRMF num_factors={0} regularization={1} alpha={2} num_iter={3}",
				NumFactors, Regularization, Alpha, NumIter);
		}
	}
}