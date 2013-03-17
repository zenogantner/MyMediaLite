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
using System.Data;
using System.Collections.Generic;

namespace MyMediaLite.Data
{
	public class IndexedMemoryReader : IInteractionReader
	{
		private IDataSet dataset;
		private IEnumerator<int> index;

		public IndexedMemoryReader(IDataSet dataset, IEnumerator<int> index)
		{
			this.dataset = dataset;
			this.index = index;
			this.index.Reset();
		}

		public IndexedMemoryReader(IDataSet dataset, IList<int> index)
		{
			this.dataset = dataset;
			this.index = index.GetEnumerator();
			this.index.Reset();
		}
		
		public bool Read()
		{
			return index.MoveNext();
		}
		
		public int GetUser()
		{
			return dataset.Users[index.Current];
		}
		
		public int GetItem()
		{
			return dataset.Items[index.Current];
		}

		public float GetRating()
		{
			var ratings = dataset as IRatings;
			if (ratings != null)
				return ratings[index.Current];
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

