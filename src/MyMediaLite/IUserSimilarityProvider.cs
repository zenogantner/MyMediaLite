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
	/// <summary>Interface for classes that provide user similarities</summary>
	public interface IUserSimilarityProvider
	{
		/// <summary>get the similarity between two users</summary>
		/// <returns>the user similarity; higher means more similar</returns>
		/// <param name='user_id1'>the ID of the first user</param>
		/// <param name='user_id2'>the ID of the second user</param>
		float GetUserSimilarity(int user_id1, int user_id2);
		/// <summary>get the most similar users</summary>
		/// <returns>the users most similar to a given user</returns>
		/// <param name='user_id'>the ID of the user</param>
		/// <param name='n'>the number of similar users to return</param>
		IList<int> GetMostSimilarUsers(int user_id, uint n = 10);
	}
}

