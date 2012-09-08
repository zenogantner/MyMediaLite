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
	/// <summary>a symmetric sparse matrix; consumes less memory</summary>
	public class SymmetricSparseMatrix<T> : SparseMatrix<T> where T:new()
	{
		/// <summary>Access the elements of the sparse matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public override T this [int x, int y]
		{
			get {
				if (x > y)
					return base[y, x];
				else
					return base[x, y];
			}
			set {
				if (x > y)
					base[y, x] = value;
				else
					base[x, y] = value;
			}
		}

		/// <summary>Always true because the data type is symmetric</summary>
		/// <value>Always true because the data type is symmetric</value>
		public override bool IsSymmetric { get { return true; } }

		/// <summary>Create a symmetric sparse matrix with a given dimension</summary>
		/// <param name="dimension">the dimension (number of rows/columns)</param>
		public SymmetricSparseMatrix(int dimension) : base(dimension, dimension) { }

		///
		public override IMatrix<T> CreateMatrix(int num_rows, int num_columns)
		{
			if (num_rows != num_columns)
				throw new ArgumentException("Symmetric matrices must have the same number of rows and columns.");
			return new SymmetricSparseMatrix<T>(num_rows);
		}

		///
		public override IList<Tuple<int, int>> NonEmptyEntryIDs
		{
			get {
				var return_list = new List<Tuple<int, int>>();
				for (int row_id = 0; row_id < index_list.Count; row_id++)
					foreach (var col_id in index_list[row_id])
					{
						return_list.Add(Tuple.Create(row_id, col_id));
						if (row_id != col_id)
							return_list.Add(Tuple.Create(col_id, row_id));
					}
				return return_list;
			}
		}

		///
		public override int NumberOfNonEmptyEntries
		{
			get {
				int counter = 0;
				for (int i = 0; i < index_list.Count; i++)
				{
					counter += 2 * index_list[i].Count;

					// adjust for diagonal elements
					if (index_list[i].Contains(i))
						counter--;
				}

				return counter;
			}
		}

		/// <summary>Resize to the given size</summary>
		/// <param name="size">the size</param>
		public virtual void Resize(int size)
		{
			Resize (size, size);
		}
	}
}