// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Collections.Generic;

namespace MyMediaLite.DataType
{
	/// <summary>a skew symmetric (anti-symmetric) sparse matrix; consumes less memory</summary>
	public class SkewSymmetricSparseMatrix : SymmetricSparseMatrix<float>
	{
		/// <summary>Access the elements of the sparse matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public override float this [int x, int y]
		{
			get {
				float result = 0f;

				if (x <= y)
					return base[x, y];
				else if (x > y)
					return -base[y, x]; // minus for anti-symmetry

				return result;
			}
			set {
				if (x <= y)
					base[x, y] = value;
				else if (x > y)
					base[y, x] = -value;
			}
		}

		/// <summary>Only true if all entries are zero, except for the diagonal</summary>
		public override bool IsSymmetric
		{
			get {
				for (int i = 0; i < index_list.Count; i++)
					foreach (var j in index_list[i])
						if (i != j && this[i, j] != 0)
							return false;
				return true;
			}
		}

		/// <summary>Create a skew symmetric sparse matrix with a given dimension</summary>
		/// <param name="dimension">the dimension (number of rows/columns)</param>
		public SkewSymmetricSparseMatrix(int dimension) : base(dimension) { }

		///
		public override IMatrix<float> CreateMatrix(int num_rows, int num_columns)
		{
			if (num_rows != num_columns)
				throw new ArgumentException("Skew symmetric matrices must have the same number of rows and columns.");
			return new SkewSymmetricSparseMatrix(num_rows);
		}
	}
}