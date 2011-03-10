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
using System.Collections.Generic;

namespace MyMediaLite
{
	public interface IRatings
	{
		IList<int> Users { get; }
		IList<int> Items { get; }
		IList<double> Values  { get; } // TODO make generic
		double this[int index] { get; }

		int MaxUserID { get; }
		int MaxItemID { get; }

		IList<List<int>> ByUser { get; }
		IList<List<int>> ByItem { get; }
		int[] RandomIndex { get; }

		int Count { get; }
		double Average { get; }

		HashSet<int> GetUsers();
		HashSet<int> GetItems();
		HashSet<int> GetUsers(IList<int> indices);
		HashSet<int> GetItems(IList<int> indices);

		double FindRating(int user_id, int item_id); // TODO think about returning an index ...

		double FindRating(int user_id, int item_id, ICollection<int> indexes);

		void AddRating(int user_id, int item_id, double rating); // TODO think about returning the index of the newly added rating
	}
}

