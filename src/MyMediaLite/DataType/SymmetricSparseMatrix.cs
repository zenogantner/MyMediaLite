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
using MyMediaLite.Util;

namespace MyMediaLite.DataType
{
	/// <summary>a symmetric sparse matrix; consumes less memory</summary>
	/// <remarks>
	/// Be careful when accessing the matrix via the NonEmptyRows property: this contains
	/// only the entries with x &gt;, but not their symmetric counterparts.
	/// </remarks>
	public class SymmetricSparseMatrix<T> : SparseMatrix<T> where T:new()
	{
		/// <summary>Access the elements of the sparse matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public override T this [int x, int y]
		{
			get	{
				// ensure x <= y
				if (x > y)
				{
					int tmp = x;
					x = y;
					y = tmp;
				}

				T result;
				if (x < row_list.Count && row_list[x].TryGetValue(y, out result))
					return result;
				else
					return new T();
			}
			set {
				// ensure x <= y
				if (x > y)
				{
					int tmp = x;
					x = y;
					y = tmp;
				}

				if (x >= row_list.Count)
					for (int i = row_list.Count; i <= x; i++)
						row_list.Add( new Dictionary<int, T>() );

				row_list[x][y] = value;
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
		public override IList<Pair<int, int>> NonEmptyEntryIDs
		{
			get	{
				var return_list = new List<Pair<int, int>>();
				foreach (var id_row in this.NonEmptyRows)
					foreach (var col_id in id_row.Value.Keys)
					{
						return_list.Add(new Pair<int, int>(id_row.Key, col_id));
						if (id_row.Key != col_id)
							return_list.Add(new Pair<int, int>(col_id, id_row.Key));
					}
				return return_list;
			}
		}
		
		///
		public override int NumberOfNonEmptyEntries
		{
			get	{
				int counter = 0;
				for (int i = 0; i < row_list.Count; i++)
				{
					counter += 2 * row_list[i].Count;
					
					// adjust for diagonal elements
					if (row_list[i].ContainsKey(i))
						counter--;
				}
				
				return counter;
			}
		}		
	}
}