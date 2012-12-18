// Copyright (C) 2012 Zeno Gantner
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
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Rating predictor that allows folding in new users</summary>
	/// <remarks>
	///   <para>
	///     The process of folding in is computing a predictive model for a new user based on their ratings
	///     and the existing recommender, without modifying the parameters of the existing recommender.
	///   </para>
	///   <para>
	///     Literature:
	///     <list type="bullet">
	///       <item><description>
	///         Badrul Sarwar and George Karypis, Joseph Konstan, John Riedl:
	///         Incremental singular value decomposition algorithms for highly scalable recommender systems.
	///         Fifth International Conference on Computer and Information Science, 2002.
	///         http://grouplens.org/papers/pdf/sarwar_SVD.pdf
	///       </description></item>
	///       </list>
	///   </para>
	/// </remarks>
	public interface IFoldInRatingPredictor : IRatingPredictor
	{
		/// <summary>Rate a list of items given a list of ratings that represent a new user</summary>
		/// <returns>a list of int and float pairs, representing item IDs and predicted ratings</returns>
		/// <param name='rated_items'>the ratings (item IDs and rating values) representing the new user</param>
		/// <param name='candidate_items'>the items to be rated</param>
		IList<Tuple<int, float>> ScoreItems(IList<Tuple<int, float>> rated_items, IList<int> candidate_items);
	}
}