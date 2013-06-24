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
	public struct SimpleInteraction : IInteraction
	{
		public int User { get { return user; } }
		private readonly int user;

		public int Item { get { return item; } }
		private readonly int item;

		public float Rating { get { throw new NotSupportedException(); } }

		public DateTime DateTime { get { throw new NotSupportedException(); } }

		public bool HasRatings { get { return false; } }
		public bool HasDateTimes { get { return false; } }

		public SimpleInteraction(int user, int item)
		{
			this.user = user;
			this.item = item;
		}
	}
}

