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

namespace MyMediaLite.Data
{
	public class InteractionReader : IInteractionReader
	{
		public ISet<int> Users { get; private set; }
		public ISet<int> Items { get; private set; }
		public int Count { get { return interaction_list.Count; } }

		private IList<IInteraction> interaction_list;
		private int pos;

		public InteractionReader(IList<IInteraction> interaction_list, ISet<int> users, ISet<int> items)
		{
			this.interaction_list = interaction_list;
			Users = users;
			Items = items;

			Reset();
		}

		public void Reset()
		{
			pos = -1;
		}

		public bool Read()
		{
			pos++;
			return pos < Count;
		}

		public void ReadInfinite()
		{
			if (!Read())
				Reset();
		}

		public int GetUser()
		{
			return interaction_list[pos].User;
		}

		public int GetItem()
		{
			return interaction_list[pos].Item;
		}

		public float GetRating()
		{
			return interaction_list[pos].Rating;
		}

		public DateTime GetDateTime()
		{
			return interaction_list[pos].DateTime;
		}

	}
}

