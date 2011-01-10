// Copyright (C) 2010, 2011 Zeno Gantner
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
using MyMediaLite.Util;


namespace MyMediaLite.DataType
{
    /// <summary>Class for storing sparse matrices</summary>
    /// <remarks>
    /// The data is stored in row-major mode.
    /// Indexes are zero-based.
    /// </remarks>
    /// <typeparam name="T">the matrix element type, must have a default constructor/value</typeparam>
    public class SparseMatrix<T> : IMatrix<T> where T:new()
    {
		// TODO create unit tests for this class

		private List<Dictionary<int, T>> row_list = new List<Dictionary<int, T>>();

		/// <inheritdoc/>
		public int NumberOfRows { get { return row_list.Count; } }

		/// <inheritdoc/>
		public int NumberOfColumns {
			get {
				int max_col_id = 0;
				foreach (var row in row_list)
					max_col_id = Math.Max(max_col_id, row.Keys.Max());
				return max_col_id + 1;
			}
		}

		/// <summary>Create a sparse matrix with a given number of rows</summary>
		/// <param name="num_rows">the number of rows</param>
		public SparseMatrix(int num_rows)
		{
			for (int i = 0; i < num_rows; i++)
				row_list.Add( new Dictionary<int, T>() );
		}

		/// <inheritdoc/>
		public IMatrix<T> CreateMatrix(int num_rows, int num_columns)
		{
			return new SparseMatrix<T>(num_rows);
		}

		/// <summary>Get a row of the matrix</summary>
		/// <param name="x">the row ID</param>
		public Dictionary<int, T> this [int x]
		{
			get {
	            if (x >= row_list.Count)
	                return new Dictionary<int, T>();
	            else return row_list[x];
			}
		}

		/// <summary>Access the elements of the sparse matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public T this [int x, int y]
		{
			get	{
				T result;
				if (x < row_list.Count && row_list[x].TryGetValue(y, out result))
					return result;
				else
					return new T();
			}
			set {
				if (x >= row_list.Count)
					for (int i = row_list.Count; i <= x; i++)
						row_list.Add( new Dictionary<int, T>() );

				row_list[x][y] = value;
			}
		}

		/// <summary>
		/// The non-empty rows of the matrix (the ones that contain at least one non-zero entry),
		/// with their IDs
		/// </summary>
		public IList<KeyValuePair<int, Dictionary<int, T>>> NonEmptyRows
		{
			get	{
				var return_list = new List<KeyValuePair<int, Dictionary<int, T>>>();
				for (int i = 0; i < row_list.Count; i++)
					if (row_list[i].Count > 0)
						return_list.Add(new KeyValuePair<int, Dictionary<int, T>>(i, row_list[i]));
				return return_list;
			}
		}

		/// <summary>The row and column IDs of non-empty entries in the matrix</summary>
		public IList<Pair<int, int>> NonEmptyEntryIDs
		{
			get	{
				var return_list = new List<Pair<int, int>>();
				foreach (var id_row in this.NonEmptyRows)
					foreach (var col_id in id_row.Value.Keys)
						return_list.Add(new Pair<int, int>(id_row.Key, col_id));
				return return_list;
			}
		}
	}
}