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
using System.Collections;
using System.Collections.Generic;

namespace MyMediaLite.DataType
{
	public class ListProxy<T> : IList<T>
	{
		IList<T> list;
		IList<int> indices;

		public ListProxy(IList<T> list, IList<int> indices)
		{
			this.list = list;
			this.indices = indices;
		}

		public T this[int index]
		{
			get {
				return list[indices[index]];
			}
			set {
				list[indices[index]] = value;
			}
		}

		public int Count { get { return list.Count; } }

		public bool IsReadOnly { get { return true; } }


		public void Add(T item) { throw new NotSupportedException(); }

		public void Clear() { throw new NotSupportedException(); }

		public bool Contains(T item) { return list.Contains(item); }

		public void CopyTo(T[] array, int i) { throw new NotImplementedException(); }

		public void Insert(int index, T item) { throw new NotSupportedException(); }

		public int IndexOf(T item) { throw new NotSupportedException(); }

		public bool Remove(T item) { throw new NotSupportedException(); }

		public void RemoveAt(int index) { throw new NotSupportedException(); }

		IEnumerator IEnumerable.GetEnumerator() { return list.GetEnumerator(); }

		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return list.GetEnumerator(); }
	}
}