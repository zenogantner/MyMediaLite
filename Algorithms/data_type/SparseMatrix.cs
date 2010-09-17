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
    /// Sparse representation of a matrix.
    /// Fast row-wise access is possible.
    /// </summary>
    /// <author>Zeno Gantner, Steffen Rendle, University of Hildesheim</author>
    [Serializable]
    public class SparseMatrix<T> : Dictionary<int, Dictionary<int, T>>
    {
        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public Dictionary<int, T> GetRow(int x)
        {
            Dictionary<int, T> result;
            if (TryGetValue(x, out result))
            {
                return result;
            }
            else
            {
                result = new Dictionary<int, T>();
                Add(x, result);
                return result;
            }
        }

		/// <summary>
        /// Set a value
        /// </summary>
        /// <param name="x">row ID.</param>
        /// <param name="y">column ID</param>
        /// <param name="val">value</param>
        public void Set(int x, int y, T val)
        {
			GetRow(x)[y] = val;
        }

		/// <summary>
		/// Get a value
		/// </summary>
		/// <param name="x">
		/// row ID
		/// </param>
		/// <param name="y">
		/// column ID
		/// </param>
		/// <returns>
		/// the value of element (row ID, column ID)
		/// </returns>
		public T Get(int x, int y)
		{
			T result;
			Dictionary<int, T> row = GetRow(x);
            row.TryGetValue(y, out result);
            return result;
		}

		//public Dictionary<int, T>.KeyCollection GetKeys()
		//{
		//	return data.Keys;
		//}

		/// <summary>
		/// Returns the number of (true) entries
		/// </summary>
		public int GetNumberOfEntries()
		{
			int n = 0;

			foreach (int x in this.Keys)
			{
				n += GetRow(x).Count;
			}

			return n;
		}
    }
}