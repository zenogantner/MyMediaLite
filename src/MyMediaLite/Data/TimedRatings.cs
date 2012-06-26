// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Linq;
using System.Runtime.Serialization;

namespace MyMediaLite.Data
{
	/// <summary>Data structure for storing ratings with time information</summary>
	/// <remarks>
	/// <para>This data structure supports incremental updates.</para>
	/// <para>
	/// Loading the Netflix Prize data set (100,000,000 ratings) into this data structure requires about 3.2 GB of memory.
	/// </para>
	/// </remarks>
	[Serializable()]
	public class TimedRatings : Ratings, ITimedRatings
	{
		///
		public IList<DateTime> Times { get; protected set; }

		///
		public DateTime EarliestTime { get; protected set; }

		///
		public DateTime LatestTime { get; protected set; }

		/// <summary>Default constructor</summary>
		public TimedRatings() : base()
		{
			Times = new List<DateTime>();
			EarliestTime = DateTime.MaxValue;
			LatestTime = DateTime.MinValue;
		}

		///
		public TimedRatings(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Times = (List<DateTime>) info.GetValue("Times", typeof(List<DateTime>));

			EarliestTime = Times.Min();
			LatestTime   = Times.Max();
		}

		///
		public override void Add(int user_id, int item_id, float rating)
		{
			throw new NotSupportedException();
		}

		///
		public virtual void Add(int user_id, int item_id, float rating, DateTime time)
		{
			Users.Add(user_id);
			Items.Add(item_id);
			Values.Add(rating);
			Times.Add(time);

			int pos = Users.Count - 1;

			if (user_id > MaxUserID)
				MaxUserID = user_id;
			if (item_id > MaxItemID)
				MaxItemID = item_id;

			// TODO speed up time stuff
			if (time < EarliestTime)
				EarliestTime = time;
			if (time > LatestTime)
				LatestTime = time;

			// update index data structures if necessary
			if (by_user != null)
			{
				for (int u = by_user.Count; u <= user_id; u++)
					by_user.Add(new List<int>());
				by_user[user_id].Add(pos);
			}
			if (by_item != null)
			{
				for (int i = by_item.Count; i <= item_id; i++)
					by_item.Add(new List<int>());
				by_item[item_id].Add(pos);
			}
		}

		///
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Times", this.Times);
		}
	}
}