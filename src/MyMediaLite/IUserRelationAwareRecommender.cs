// Copyright (C) 2010 Zeno Gantner
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

using MyMediaLite.DataType;

namespace MyMediaLite
{
	/// <summary>Interface for recommenders that take a binary relation over users into account</summary>
	/// <remarks></remarks>
	public interface IUserRelationAwareRecommender : IRecommender
	{
		/// <value>The binary user relation</value>
		/// <remarks></remarks>
		SparseBooleanMatrix UserRelation { get; set; }

		/// <value>Number of users</value>
		/// <remarks></remarks>
		int NumUsers { get; } // TODO think about getting rid of this again
	}
}
