// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MyMediaLite.DataType
{
	/// <summary>Sparse representation of a boolean matrix</summary>
	/// <remarks>
	/// Fast row-wise access is possible.
	/// Indexes are zero-based.
	/// </remarks>
	public class SparseBooleanMatrix : IMatrix<bool>
	{
		private List<HashSet<int>> row_list = new List<HashSet<int>>();

		/// <summary>Indexer to access the elements of the matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public bool this [int x, int y]
		{
			get	{
				if (x < row_list.Count)
					return row_list[x].Contains(y);
				else
					return false;
			}
			set	{
				if (value)
					this[x].Add(y);
				else
					this[x].Remove(y);
			}
		}

		/// <summary>Indexer to access the rows of the matrix</summary>
		/// <param name="x">the row ID</param>
		public HashSet<int> this [int x]
		{
			get	{
				if (x >= row_list.Count)
					for (int i = row_list.Count; i <= x; i++)
						row_list.Add(new HashSet<int>());
				return row_list[x];
			}
			set
			{
				row_list[x] = value; // TODO get rid of this?
			}
		}

		/// <inheritdoc/>
		public virtual bool IsSymmetric
		{
			get	{
				for (int i = 0; i < row_list.Count; i++)
					foreach (var j in row_list[i])
					{
						if (i > j)
							continue; // check every pair only once

						if (!this[j, i])
							return false;
					}
				return true;
			}
		}

		/// <inheritdoc/>
		public IMatrix<bool> CreateMatrix(int x, int y)
		{
			return new SparseBooleanMatrix();
		}

		/// <summary>The rows of the matrix, with their IDs</summary>
		/// <value>The rows of the matrix, with their IDs</value>
		public IList<KeyValuePair<int, HashSet<int>>> Rows
		{
			get	{
				var return_list = new List<KeyValuePair<int, HashSet<int>>>();
				for (int i = 0; i < row_list.Count; i++)
					return_list.Add(new KeyValuePair<int, HashSet<int>>(i, row_list[i]));
				return return_list;
			}
		}

		/// <summary>The non-empty rows of the matrix (the ones that contain at least one true entry), with their IDs</summary>
		/// <value>The non-empty rows of the matrix (the ones that contain at least one true entry), with their IDs</value>
		public IList<KeyValuePair<int, HashSet<int>>> NonEmptyRows
		{
			get	{
				var return_list = new List<KeyValuePair<int, HashSet<int>>>();
				for (int i = 0; i < row_list.Count; i++)
				{
					if (row_list[i].Count > 0)
						return_list.Add(new KeyValuePair<int, HashSet<int>>(i, row_list[i]));
				}
				return return_list;
			}
		}

		/// <summary>The IDs of the non-empty rows in the matrix (the ones that contain at least one true entry)</summary>
		/// <value>The IDs of the non-empty rows in the matrix (the ones that contain at least one true entry)</value>
		public ICollection<int> NonEmptyRowIDs
		{
			get	{
				var row_ids = new HashSet<int>();

				for (int i = 0; i < row_list.Count; i++)
					if (row_list[i].Count > 0)
						row_ids.Add(i);

				return row_ids;
			}
		}

		// TODO add unit test
		/// <summary>The IDs of the non-empty columns in the matrix (the ones that contain at least one true entry)</summary>
		/// <value>The IDs of the non-empty columns in the matrix (the ones that contain at least one true entry)</value>
		public ICollection<int> NonEmptyColumnIDs
		{
			get	{
				var col_ids = new HashSet<int>();

				// iterate over the complete data structure to find column IDs
				for (int i = 0; i < row_list.Count; i++)
					foreach (int id in row_list[i])
						col_ids.Add(id);

				return col_ids;
			}
		}

		/// <summary>The number of rows in the matrix</summary>
		/// <value>The number of rows in the matrix</value>
		public int NumberOfRows	{ get { return row_list.Count; } }

		/// <summary>The number of columns in the matrix</summary>
		/// <value>The number of columns in the matrix</value>
		public int NumberOfColumns {
			get	{
				int max_column_id = -1;
				foreach (var row in row_list)
					if (row.Count > 0)
						max_column_id = Math.Max(max_column_id, row.Max());

				return max_column_id + 1;
			}
		}

		/// <summary>The number of (true) entries</summary>
		/// <value>The number of (true) entries</value>
		public int NumberOfEntries
		{
			get	{
				int n = 0;
				foreach (var row in row_list)
					n += row.Count;
				return n;
			}
		}

		/// <summary>Removes a column, and fills the gap by decrementing all occurrences of higher column IDs by one.</summary>
		/// <param name="y">the column ID</param>
		public void RemoveColumn(int y)
		{
			for (int row_id = 0; row_id < row_list.Count; row_id++)
			{
				var cols = new List<int>(row_list[row_id]);
				foreach (int col_id in cols)
				{
					if (col_id >= y)
						row_list[row_id].Remove(y);
					if (col_id > y)
						row_list[row_id].Add(col_id - 1);
				}
			}
		}

		/// <summary>
		/// Removes several columns, and fills the gap by decrementing all occurrences of higher column IDs.
		/// </summary>
		/// <param name="delete_columns">an array with column IDs</param>
		public void RemoveColumn(int[] delete_columns)
		{
			for (int row_id = 0; row_id < row_list.Count; row_id++)
			{
				var cols = new List<int>(row_list[row_id]);
				foreach (int col_id in cols)
				{
					int decrease_by = 0;
					foreach (int y in delete_columns)
					{
						if (col_id == y)
						{
							row_list[row_id].Remove(y);
							goto NEXT_COL; // poor man's labeled continue
						}
						if (col_id > y)
							decrease_by++;
					}

					// decrement column ID
					row_list[row_id].Remove(col_id);
					row_list[row_id].Add(col_id - decrease_by);

					NEXT_COL:;
				}
			}
		}

		/// <summary>
		/// Get the transpose of the matrix, i.e. a matrix where rows and columns are interchanged
		/// </summary>
		/// <returns>
		/// the transpose of the matrix
		/// </returns>
		public SparseBooleanMatrix Transpose()
		{
			var transpose = new SparseBooleanMatrix();
			for (int i = 0; i < row_list.Count; i++)
				foreach (int j in this[i])
					transpose[j, i] = true;
			return transpose;
		}

		/// <summary>Get the overlap of two matrices, i.e. the number of true entries where they agree</summary>
		/// <param name="s">the <see cref="SparseBooleanMatrix"/> to compare to</param>
		/// <returns>the number of entries that are true in both matrices</returns>
		public int Overlap(SparseBooleanMatrix s)
		{
			int c = 0;

			for (int i = 0; i < row_list.Count; i++)
				foreach (int j in row_list[i])
					if (s[i, j])
						c++;

			return c;
		}
   }
}