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
	/// <summary>a symmetric sparse matrix; consumes less memory</summary>
	public class SymmetricSparseMatrix<T> : SparseMatrix<T> where T:new()
	{
		/// <summary>Access the elements of the sparse matrix</summary>
		/// <param name="x">the row ID</param>
		/// <param name="y">the column ID</param>
		public override T this [int x, int y]
		{
			get	{
				// ensure x < y
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
				// ensure x < y
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
		
		/// <summary>Create a symmetric sparse matrix with a given number of rows</summary>
		/// <param name="num_rows">the number of rows</param>		
		public SymmetricSparseMatrix(int num_rows) : base(num_rows, num_rows) { }
	}
}