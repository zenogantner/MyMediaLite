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
	/// <summary>Combines two List objects</summary>
	public class CombinedList<T> : IList<T>
	{
		IList<T> first;
		IList<T> second;

		/// <summary>Create a new CombinedList object</summary>
		/// <param name="list1">first list</param>
		/// <param name="list2">second list</param>
		public CombinedList(IList<T> list1, IList<T> list2)
		{
			first = list1;
			second = list2;
		}

		///
		public T this[int index]
		{
			get {
				if (index < first.Count)
					return first[index];
				else
					return second[index - first.Count];
			}
			set {
				throw new NotSupportedException();
			}
		}

		///
		public int Count { get { return first.Count + second.Count; } }

		///
		public bool IsReadOnly { get { return first.IsReadOnly || second.IsReadOnly; } }

		///
		public void Add(T item)
		{
			second.Add(item);
		}

		///
		public void Clear() { throw new NotSupportedException(); }

		///
		public bool Contains(T item) {
			for (int i = 0; i < first.Count; i++)
				if (first[i].Equals(item))
					return true;
			for (int i = first.Count; i < first.Count + second.Count; i++)
				if (second[i - first.Count].Equals(item))
					return true;
			return false;
		}

		///
		public void CopyTo(T[] array, int i) { throw new NotImplementedException(); }

		///
		public void Insert(int index, T item) { throw new NotSupportedException(); }

		///
		public int IndexOf(T item) { throw new NotSupportedException(); }

		///
		public bool Remove(T item) { throw new NotSupportedException(); }

		///
		public void RemoveAt(int index)
		{
			if (index < first.Count)
				first.RemoveAt(index);
			else
				second.RemoveAt(index - first.Count);
		}

		///
		IEnumerator IEnumerable.GetEnumerator()
		{
			var list = new List<T>(first);
			list.AddRange(second);
			return list.GetEnumerator();
		}

		///
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			var list = new List<T>(first);
			list.AddRange(second);
			return list.GetEnumerator();
		}
	}
}