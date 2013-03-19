// Copyright (C) 2013 Zeno Gantner
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
//
using System;
using System.Collections.Generic;
using System.Data;

namespace MyMediaLite.Data
{
	public class IndexedMemoryReader : IInteractionReader
	{
		public int Count { get { return index.Count; } }

		public ISet<int> Users
		{
			get {
				if (_users == null)
					_users = AllValues(dataset.Users);
				return _users;
			}
		}
		private ISet<int> _users;

		public ISet<int> Items
		{
			get {
				if (_items == null)
					_items = AllValues(dataset.Items);
				return _items;
			}
		}
		private ISet<int> _items;

		private IDataSet dataset;
		private IList<int> index;
		private IEnumerator<int> enumerator;

		public IndexedMemoryReader(IDataSet dataset, IList<int> index)
		{
			this.dataset = dataset;
			this.index = index;
			this.enumerator = index.GetEnumerator();
		}

		public void Reset()
		{
			enumerator.Reset();
		}

		public bool Read()
		{
			return enumerator.MoveNext();
		}

		public void ReadInfinite()
		{
			if (!enumerator.MoveNext())
				enumerator.Reset();
		}

		public int GetUser()
		{
			return dataset.Users[enumerator.Current];
		}

		// TODO move somewhere else?
		ISet<T> AllValues<T>(IList<T> c)
		{
			var result = new HashSet<T>();
			foreach (int pos in index)
				result.Add(c[pos]);
			return result;
		}

		public int GetItem()
		{
			return dataset.Items[enumerator.Current];
		}

		public float GetRating()
		{
			var ratings = dataset as IRatings;
			if (ratings != null)
				return ratings[enumerator.Current];
			else
				throw new NotSupportedException();
		}

		public DateTime GetDateTime()
		{
			throw new NotImplementedException();
		}

		public long GetTimestamp()
		{
			throw new NotImplementedException();
		}
	}
}

