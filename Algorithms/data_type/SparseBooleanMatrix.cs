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
using System.Linq;
using System.Text;
using System.IO;


namespace MyMediaLite.data_type
{
    /// <summary>
    /// Sparse representation of a boolean matrix.
    /// Fast row-wise access is possible.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    [Serializable]
    public class SparseBooleanMatrix
    {
		List<HashSet<int>> rows = new List<HashSet<int>>();

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="MyMediaLite.data_type.SparseBooleanMatrix"/> class.
		/// </summary>
		public SparseBooleanMatrix() {}

		public bool Get(int x, int y)
		{
            if (x < rows.Count)
                return rows[x].Contains(y);
			else
				return false;
		}

        /// <summary>Get a row</summary>
        /// <param name="x">row ID</param>
        /// <returns>the row</returns>
        public HashSet<int> GetRow(int x)
        {
            if (x >= rows.Count)
				for (int i = rows.Count; i <= x; i++)
                	rows.Add(new HashSet<int>());

			return rows[x];
        }

		public IList<KeyValuePair<int, HashSet<int>>> GetRows()
		{
			var return_list = new List<KeyValuePair<int, HashSet<int>>>();
			for (int i = 0; i < rows.Count; i++)
			{
				return_list.Add(new KeyValuePair<int, HashSet<int>>(i, rows[i]));
			}
			return return_list;
		}

		public IList<KeyValuePair<int, HashSet<int>>> GetNonEmptyRows()
		{
			var return_list = new List<KeyValuePair<int, HashSet<int>>>();
			for (int i = 0; i < rows.Count; i++)
			{
				if (rows[i].Count > 0)
					return_list.Add(new KeyValuePair<int, HashSet<int>>(i, rows[i]));
			}
			return return_list;
		}

		public HashSet<int> GetNonEmptyRowIDs()
		{
			HashSet<int> row_ids = new HashSet<int>();

			for (int i = 0; i < rows.Count; i++)
				if (rows[i].Count > 0)
					row_ids.Add(i);

			return row_ids;
		}

		public void SetRow(int x, HashSet<int> row)
		{
			rows[x] = row;
		}

		public int GetNumberOfRows()
		{
			return rows.Count;
		}

		/// <summary>Adds an entry to the matrix</summary>
        /// <param name="x">row ID</param>
        /// <param name="y">column ID</param>
        public void AddEntry(int x, int y)
        {
            GetRow(x).Add(y);
        }

        public void RemoveEntry(int x, int y)
        {
            GetRow(x).Remove(y);
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

		/// <summary>Returns the number of (true) entries</summary>
		public int GetNumberOfEntries()
		{
			int n = 0;
			foreach (var row in rows)
				n += row.Count;

			return n;
		}

		public SparseBooleanMatrix Transpose()
		{
			SparseBooleanMatrix transpose = new SparseBooleanMatrix();
			for (int i = 0; i < rows.Count; i++)
			{
				foreach (int j in this.GetRow(i))
					transpose.AddEntry(j, i);
			}
			return transpose;
		}

		public int Overlap(SparseBooleanMatrix s)
		{
			int c = 0;

			for (int i = 0; i < rows.Count; i++)
				foreach (int j in rows[i])
					if (s.Get(i, j))
						c++;

			return c;
		}
   }
}