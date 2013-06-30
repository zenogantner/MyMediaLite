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
using System.Threading;

namespace MyMediaLite.Data
{
	// Temporary class -- will never be part of a release.
	public class MemoryInteractions : IInteractions
	{
		///
		public int Count { get { return dataset.Count; } }

		///
		public int MaxUserID { get; private set; }

		///
		public int MaxItemID { get; private set; }

		///
		public DateTime EarliestDateTime
		{
			get {
				if (dataset is ITimedDataSet)
					return ((ITimedDataSet) dataset).EarliestTime;
				else
					throw new NotSupportedException("Data set does not contain time information.");
			}
		}

		///
		public DateTime LatestDateTime
		{
			get {
				if (dataset is ITimedDataSet)
					return ((ITimedDataSet) dataset).LatestTime;
				else
					throw new NotSupportedException("Data set does not contain time information.");
			}
		}

		///
		public IInteractionReader Random
		{
			get {
				return new IndexedMemoryReader(dataset, dataset.RandomIndex);
			}
		}

		///
		public IInteractionReader Sequential
		{
			get {
				return new IndexedMemoryReader(dataset, Enumerable.Range(0, dataset.Count).ToArray());
			}
		}

		///
		public IList<int> Users { get { return dataset.AllUsers; } }
		///
		public IList<int> Items { get { return dataset.AllItems; } }

		///
		public RatingScale RatingScale
		{
			get {
				var ratings = dataset as IRatings;
				if (ratings == null)
					throw new NotSupportedException();
				return ratings.Scale;
			}
		}

		///
		public IDataSet dataset; // TODO make private again

		///
		public MemoryInteractions(IDataSet dataset)
		{
			this.dataset = dataset;
			MaxUserID = dataset.MaxUserID;
			MaxItemID = dataset.MaxItemID;
		}

		private readonly object syncLock = new object();

		///
		public IInteractionReader ByUser(int user_id)
		{
			lock(syncLock)
			{
				if (user_id >= dataset.ByUser.Count)
					throw new ArgumentOutOfRangeException("user_id", user_id, string.Format("should not be >= {0}, dataset.MaxUserID is {1}", dataset.ByUser.Count, dataset.MaxUserID));
				return new IndexedMemoryReader(dataset, dataset.ByUser[user_id]);
			}
		}

		///
		public IInteractionReader ByItem(int item_id)
		{
			lock(syncLock)
			{
				if (item_id >= dataset.ByItem.Count)
					throw new ArgumentOutOfRangeException("item_id", item_id, string.Format("should not be >= {0}, dataset.MaxItemID is {1}", dataset.ByItem.Count, dataset.MaxItemID));
				return new IndexedMemoryReader(dataset, dataset.ByItem[item_id]);
			}
		}

		public bool HasRatings
		{
			get {
				return dataset is IRatings;
			}
		}

		public bool HasDateTimes
		{
			get {
				return dataset is ITimedDataSet;
			}
		}
	}
}