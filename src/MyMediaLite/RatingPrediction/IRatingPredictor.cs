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

using MyMediaLite.Data;

/*! \namespace MyMediaLite.RatingPrediction
 *  \brief This namespace contains rating predictors and some helper classes for rating prediction.
 */
namespace MyMediaLite.RatingPrediction
{
	/// <summary>Interface for rating predictors</summary>
	/// <remarks>
	/// Rating prediction is used in systems that let users rate items (e.g. movies, books, songs, etc.)
	/// on a certain scale, e.g. from 1 to 5 stars, where 1 star means the user does not like the item at all,
	/// and 5 stars mean the user likes the item very much.
	///
	/// Given an (incomplete) set of ratings for several items by several users (and maybe additional information),
	/// the task is to predict (some of the) missing ratings.
	///
	/// See also http://recsyswiki.com/wiki/Rating_prediction
	/// </remarks>
	public interface IRatingPredictor : IRecommender
	{
		/// <summary>the ratings to learn from</summary>
		IRatings Ratings { get; set; }

		/// <summary>Gets or sets the maximum rating.</summary>
		/// <value>The maximally possible rating</value>
		float MaxRating { get; set; }

		/// <summary>Gets or sets the minimum rating.</summary>
		/// <value>The minimally possible rating</value>
		float MinRating { get; set; }
	}
}
