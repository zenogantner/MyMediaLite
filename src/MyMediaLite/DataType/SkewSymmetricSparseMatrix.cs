// Copyright (C) 2011 Zeno Gantner
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
	// TODO default should be double, create a specific FloatSkewSymmetricSparseMatrix
	
	/// <summary>a skew symmetric (anti-symmetric) sparse matrix; consumes less memory</summary>
	/// <remarks>
	/// Be careful when accessing the matrix via the NonEmptyEntryIDs and
	/// NonEmptyRows properties: these contain only the entries with x &gt; y,
	/// but not their antisymmetric counterparts.
	/// </remarks>
	public class SkewSymmetricSparseMatrix : SymmetricSparseMatrix<float>
	{
		/// <summary>Access the elements of the sparse matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public override float this [int x, int y]
		{
			get	{
				float result = 0f;
				
				if (x < y)
				{
					if (x < row_list.Count && row_list[x].TryGetValue(y, out result))
						return result;
				}
				else if (x > y)
				{
					if (y < row_list.Count && row_list[y].TryGetValue(x, out result))
						return -result; // minus for anti-symmetry
				}

				return result;
			}
			set {
				if (x < y)
				{
					if (x >= row_list.Count)
						for (int i = row_list.Count; i <= x; i++)
							row_list.Add( new Dictionary<int, float>() );
					
					row_list[x][y] = value;					
				}
				else if (x > y)
				{
					if (y >= row_list.Count)
						for (int i = row_list.Count; i <= y; i++)
							row_list.Add( new Dictionary<int, float>() );
					
					row_list[y][x] = -value;
				}
				else
				{
					// all elements on the diagonal must be zero
					if (value != 0)
						throw new ArgumentException("Elements of the diagonal of a skew symmetric matrix must equal 0");
				}
			}
		}

		/// <summary>Only true if all entries are zero</summary>
		/// <value>Only true if all entries are zero</value>
		public override bool IsSymmetric
		{
			get {
				for (int i = 0; i < row_list.Count; i++)
					foreach (var j in row_list[i].Keys)
						if (this[i, j] != 0)
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