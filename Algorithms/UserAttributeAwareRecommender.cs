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

using System;
using MyMediaLite.data_type;

namespace MyMediaLite
{
	/// <summary>Interface for recommender engines that take binary user attributes into account</summary>
	public interface UserAttributeAwareRecommender : RecommenderEngine
	{
		/// <summary>Number of binary user attributes</summary>
	    int NumUserAttributes { get; set; }

		/// <summary>Set binary user attributes</summary>
		/// <param name="matrix">
		/// A <see cref="SparseBooleanMatrix"/> containing the binary attributes, rows are users, columns are attributes
		/// </param>
		/// <param name="num_attr">number of attributes</param>
		void SetUserAttributeData(SparseBooleanMatrix matrix, int num_attr);
	}
}
