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

namespace MyMediaLite.ItemRecommendation.BPR
{
	public interface IBPRSampler
	{
		void ItemPair(ISet<int> user_items, out int item_id, out int other_item_id);
		void NextTriple(out int u, out int i, out int j);
		bool OtherItem(int user_id, int item_id, out int other_item_id);
		bool OtherItem(ISet<int> user_items, int item_id, out int other_item_id);
		int NextUser();
	}
}
