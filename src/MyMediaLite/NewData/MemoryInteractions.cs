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
using System.Linq;

namespace MyMediaLite.Data
{
	public class MemoryInteractions : IInteractions
	{
		public IInteractionReader Random
		{
			get {
				return new IndexedMemoryReader(dataset, dataset.RandomIndex);
			}
		}

		public IInteractionReader Sequential
		{
			get {
				return new IndexedMemoryReader(dataset, Enumerable.Range(0, dataset.Count).GetEnumerator());
			}
		}

		public IList<int> Users { get { return dataset.AllUsers; } }
		public IList<int> Items { get { return dataset.AllItems; } }
		
		public RatingScale RatingScale
		{
			get {
				var ratings = dataset as IRatings;
				if (ratings == null)
					throw new NotSupportedException();
				return ratings.Scale;
			}
		}
		
		private IDataSet dataset;

		public MemoryInteractions(IDataSet dataset)
		{
			this.dataset = dataset;
		}

		public IInteractionReader ByUser(int user_id)
		{
			return new IndexedMemoryReader(dataset, dataset.ByUser[user_id]);
		}
		
		public IInteractionReader ByItem(int item_id)
		{
			return new IndexedMemoryReader(dataset, dataset.ByItem[item_id]);
		}

	}
}

