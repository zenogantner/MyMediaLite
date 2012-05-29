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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace MyMediaLite
{
	/// <summary>Interface for classes that provide item similarities</summary>
	public interface IItemSimilarityProvider
	{
		/// <summary>get the similarity between two items</summary>
		/// <returns>the item similarity; higher means more similar</returns>
		/// <param name='item_id1'>the ID of the first item</param>
		/// <param name='item_id2'>the ID of the second item</param>
		float GetItemSimilarity(int item_id1, int item_id2);
		/// <summary>get the most similar items</summary>
		/// <returns>the items most similar to a given item</returns>
		/// <param name='item_id'>the ID of the item</param>
		/// <param name='n'>the number of similar items to return</param>
		IList<int> GetMostSimilarItems(int item_id, uint n = 10);
	}
}

