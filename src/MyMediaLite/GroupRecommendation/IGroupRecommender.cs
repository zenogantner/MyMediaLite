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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

/*! \namespace MyMediaLite.GroupRecommendations
 *  \brief This namespace contains recommenders that make recommendations to groups of users.
 */
namespace MyMediaLite.GroupRecommendation
{
	/// <summary>Interface for group recommenders</summary>
	public interface IGroupRecommender
	{
		/// <summary>Rank items for a given group of users</summary>
		/// <param name="users">the users</param>
		/// <param name="items">the items to be ranked</param>
		/// <returns>a ranked list of items, highest-ranking item comes first</returns>
		IList<int> RankItems(ICollection<int> users, ICollection<int> items);
	}
}
