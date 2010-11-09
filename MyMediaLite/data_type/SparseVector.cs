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


namespace MyMediaLite.data_type
{
    /// <summary>
    /// Class for storing sparse vectors.
    /// Indexes are zero-based.
    /// </summary>
    /// <typeparam name="T"></typeparam>	
    public class SparseVector<T> where T:new()
    {
		private Dictionary<int, T> data = new Dictionary<int, T>();
		
		/// <summary>
		/// Access an element of the vector
		/// </summary>
		/// <param name="x">the index</param>
		public T this [int x] {
			get {
	            T result;
	            if (data.TryGetValue(x, out result))
		            return result;
				else
					return new T();
			}
		}
	}
}