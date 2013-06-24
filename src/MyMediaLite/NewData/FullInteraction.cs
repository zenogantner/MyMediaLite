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

namespace MyMediaLite.Data
{
	public struct FullInteraction : IInteraction
	{
		public int User { get { return user; } }
		private readonly int user;

		public int Item { get { return item; } }
		private readonly int item;

		public float Rating { get { return rating; } }
		private readonly float rating;

		public DateTime DateTime { get { return date_time; } }
		private readonly DateTime date_time;

		public bool HasRatings { get { return true; } }
		public bool HasDateTimes { get { return true; } }

		public FullInteraction(int user, int item, float rating, DateTime date_time)
		{
			this.user = user;
			this.item = item;
			this.rating = rating;
			this.date_time = date_time;
		}
	}
}

