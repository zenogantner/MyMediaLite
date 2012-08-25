// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Interface for item recommenders</summary>
	/// <remarks>
	///   <para>
	///     Item prediction or item recommendation is the task of predicting items (movies, books, products, videos, jokes)
	///     that a user may like, based on past user behavior (and possibly other information).
	///   </para>
	///   <para>
	///     See also http://recsyswiki/wiki/Item_prediction
	///   </para>
	/// </remarks>
	public interface IIncrementalItemRecommender : IIncrementalRecommender
	{
		/// <summary>Add positive feedback events and perform incremental training</summary>
		/// <param name='feedback'>collection of user id - item id tuples</param>
		void AddFeedback(ICollection<Tuple<int, int>> feedback);

		/// <summary>Remove all feedback events by the given user-item combinations</summary>
		/// <param name='feedback'>collection of user id - item id tuples</param>
		void RemoveFeedback(ICollection<Tuple<int, int>> feedback);
	}
}