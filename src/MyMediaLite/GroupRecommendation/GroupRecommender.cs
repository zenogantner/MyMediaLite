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

using System;
using System.Collections.Generic;
using MyMediaLite;

namespace MyMediaLite.GroupRecommendation
{
	/// <summary>Base class for group recommenders </summary>
	public abstract class GroupRecommender : IGroupRecommender
	{
		/// <summary>The underlying recommender that produces the user-wise item scores</summary>
		protected IRecommender recommender;

 		/// <summary>Constructor that takes the underlying recommender that will be used</summary>
		/// <param name="recommender">the underlying recommender</param>		
		public GroupRecommender(IRecommender recommender)
		{
			this.recommender = recommender;
		}

		///
		public abstract IList<int> RankItems(ICollection<int> users, ICollection<int> items);
	}
}

