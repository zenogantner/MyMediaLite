// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.Data;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>
	/// Base class for item recommenders that support incremental updates
	/// </summary>
	/// <remarks>
	/// </remarks>
	public abstract class IncrementalItemRecommender : ItemRecommender, IIncrementalItemRecommender
	{
		///
		public bool UpdateUsers { get; set; }

		///
		public bool UpdateItems { get; set; }

		///
		public virtual void AddFeedback(ICollection<Tuple<int, int>> feedback)
		{
			foreach (var tuple in feedback)
			{
				int user_id = tuple.Item1;
				int item_id = tuple.Item2;
			
				if (user_id > MaxUserID)
					AddUser(user_id);
				if (item_id > MaxItemID)
					AddItem(item_id);

				Feedback.Add(user_id, item_id);
			}
		}

		///
		public virtual void RemoveFeedback(ICollection<Tuple<int, int>> feedback)
		{
			foreach (var tuple in feedback)
			{
				int user_id = tuple.Item1;
				int item_id = tuple.Item2;

				if (user_id > MaxUserID)
					throw new ArgumentException("Unknown user " + user_id);
				if (item_id > MaxItemID)
					throw new ArgumentException("Unknown item " + item_id);
	
				Feedback.Remove(user_id, item_id);
			}
		}

		///
		protected virtual void AddUser(int user_id)
		{
			if (user_id > MaxUserID)
				MaxUserID = user_id;
		}

		///
		protected virtual void AddItem(int item_id)
		{
			if (item_id > MaxItemID)
				MaxItemID = item_id;
		}

		///
		public virtual void RemoveUser(int user_id)
		{
			Feedback.RemoveUser(user_id);

			if (user_id == MaxUserID)
				MaxUserID--;
		}

		///
		public virtual void RemoveItem(int item_id)
		{
			Feedback.RemoveItem(item_id);

			if (item_id == MaxItemID)
				MaxItemID--;
		}
	}
}