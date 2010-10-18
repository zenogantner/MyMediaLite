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
//using System.IO;


namespace MyMediaLite.data_type
{
    /// <summary>
    /// Sparse representation of a boolean matrix.
    /// Fast row-wise access is possible.
    /// Indexes are zero-based.
    /// </summary>
    public class SparseBooleanMatrix
    {
		private List<HashSet<int>> rows = new List<HashSet<int>>();

		/// <summary>
		/// Indexer to access the elements of the matrix
		/// </summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public bool this [int x, int y]
		{
			get
			{
	            if (x < rows.Count)
	                return rows[x].Contains(y);
				else
					return false;
			}
			set
			{
				if (value)
            		this[x].Add(y);
				else
            		this[x].Remove(y);
			}
		}

		/// <summary>
		/// Indexer to access the rows of the matrix
		/// </summary>
		/// <param name="x">
		/// the row ID
		/// </param>
		public HashSet<int> this [int x]
		{
			get
			{
	            if (x >= rows.Count)
					for (int i = rows.Count; i <= x; i++)
        	        	rows.Add(new HashSet<int>());
				return rows[x];
			}
			set
			{
				rows[x] = value;
			}
		}

		/// <summary>
		/// The rows of the matrix, with their IDs
		/// </summary>
		public IList<KeyValuePair<int, HashSet<int>>> Rows
		{
			get
			{
				var return_list = new List<KeyValuePair<int, HashSet<int>>>();
				for (int i = 0; i < rows.Count; i++)
				{
					return_list.Add(new KeyValuePair<int, HashSet<int>>(i, rows[i]));
				}
				return return_list;
			}
		}

		/// <summary>
		/// The non-empty rows of the matrix (the ones that contain at least one true entry),
		/// with their IDs
		/// </summary>
		public IList<KeyValuePair<int, HashSet<int>>> NonEmptyRows
		{
			get
			{
				var return_list = new List<KeyValuePair<int, HashSet<int>>>();
				for (int i = 0; i < rows.Count; i++)
				{
					if (rows[i].Count > 0)
						return_list.Add(new KeyValuePair<int, HashSet<int>>(i, rows[i]));
				}
				return return_list;
			}
		}

		/// <summary>
		/// The IDs of the non-empty rows in the matrix (the ones that contain at least one true entry)
		/// </summary>
		public ICollection<int> NonEmptyRowIDs
		{
			get
			{
				HashSet<int> row_ids = new HashSet<int>();

				for (int i = 0; i < rows.Count; i++)
					if (rows[i].Count > 0)
						row_ids.Add(i);

				return row_ids;
			}
		}

		/// <summary>
		/// The number of rows in the matrix
		/// </summary>
		public int NumberOfRows	{ get { return rows.Count; } }

		/// <summary>The number of (true) entries</summary>
		public int NumberOfEntries
		{
			get
			{
				int n = 0;
				foreach (var row in rows)
					n += row.Count;
	
				return n;
			}
		}		
		
		/// <summary>
		/// Removes a column, and fills the gap by decrementing all occurrences of higher column IDs by one.
		/// </summary>
		/// <param name="y">the column ID</param>
		public void RemoveColumn(int y)
		{
			foreach (var row in rows)
				foreach (int col_id in row)
				{
					if (col_id >= y)
						row.Remove(y);
					if (col_id > y)
						row.Add(col_id - 1);
				}
		}

		/// <summary>
		/// Removes several columns, and fills the gap by decrementing all occurrences of higher column IDs.
		/// </summary>
		/// <param name="delete_columns">an array with column IDs</param>
		public void RemoveColumn(int[] delete_columns)
		{
			foreach (var row in rows)
			{
				foreach (int col_id in row)
				{
					int decrease_by = 0;
					foreach (int y in delete_columns)
					{
						if (col_id == y)
						{
							row.Remove(y);
							goto NEXT_COL; // poor man's labeled continue
						}
						if (col_id > y)
							decrease_by++;
					}

					// decrement column ID
					row.Remove(col_id);
					row.Add(col_id - decrease_by);

					NEXT_COL:;
				} //COL
			} //ROW
		}

		/// <summary>
		/// Get the transpose of the matrix, i.e. a matrix where rows and columns are interchanged
		/// </summary>
		/// <returns>
		/// the transpose of the matrix
		/// </returns>
		public SparseBooleanMatrix Transpose()
		{
			SparseBooleanMatrix transpose = new SparseBooleanMatrix();
			for (int i = 0; i < rows.Count; i++)
			{
				foreach (int j in this[i])
					transpose[j, i] = true;
			}
			return transpose;
		}

		/// <summary>
		/// Get the overlap of two matrices, i.e. the number of true entries where they agree
		/// </summary>
		/// <param name="s">
		/// the <see cref="SparseBooleanMatrix"/> to compare to
		/// </param>
		/// <returns>
		/// the number of entries that are true in both matrices
		/// </returns>
		public int Overlap(SparseBooleanMatrix s)
		{
			int c = 0;

			for (int i = 0; i < rows.Count; i++)
				foreach (int j in rows[i])
					if (s[i, j])
						c++;

			return c;
		}
   }
}