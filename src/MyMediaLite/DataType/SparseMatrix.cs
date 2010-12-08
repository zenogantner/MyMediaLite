// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite.Util;


namespace MyMediaLite.DataType
{
    /// <summary>
    /// Class for storing sparse matrices.
    /// The data is stored in row-major mode.
    /// Indexes are zero-based.
    /// </summary>
    /// <typeparam name="T"></typeparam>	
    public class SparseMatrix<T> where T:new()
    {
		private Dictionary<int, Dictionary<int, T>> data = new Dictionary<int, Dictionary<int, T>>();
		
		/// <summary>
		/// Get a row of the matrix
		/// </summary>
		/// <param name="x">the row ID</param>
		public Dictionary<int, T> this [int x] {
			get {
	            Dictionary<int, T> result;
	            if (!data.TryGetValue(x, out result))
				{
	                result = new Dictionary<int, T>();
	                data.Add(x, result);
	            }
	            return result;
			}
		}
		
		/// <summary>
		/// Access the elements of the sparse matrix
		/// </summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public T this [int x, int y] {
			get {
				T result;
	            if (this[x].TryGetValue(y, out result))
					return result;
				else
					return new T();
			}
			set {
				this[x][y] = value;
			}
		}
		
		/// <summary>
		/// The non-empty rows of the matrix (the ones that contain at least one non-zero entry),
		/// with their IDs
		/// </summary>
		public IList<KeyValuePair<int, Dictionary<int, T>>> NonEmptyRows
		{
			get
			{
				var return_list = new List<KeyValuePair<int, Dictionary<int, T>>>();
				for (int i = 0; i < data.Count; i++)
					if (data[i].Count > 0)
						return_list.Add(new KeyValuePair<int, Dictionary<int, T>>(i, data[i]));
				return return_list;
			}
		}
		
		/// <summary>
		/// The row and column IDs of non-empty entries in the matrix.
		/// </summary>
		public IList<Pair<int, int>> NonEmptyEntryIDs
		{
			get
			{
				var return_list = new List<Pair<int, int>>();
				foreach (var id_row in this.NonEmptyRows)
					foreach (var col_id in id_row.Value.Keys)
						return_list.Add(new Pair<int, int>(id_row.Key, col_id));
				return return_list;
			}
		}		
		
	}
}

