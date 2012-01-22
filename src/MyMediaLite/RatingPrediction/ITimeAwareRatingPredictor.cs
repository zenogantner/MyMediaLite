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

using System;
using MyMediaLite.Data;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Interface for time-aware rating predictors</summary>
	/// <remarks>
	/// Time-aware rating predictors use the information contained in the dates/times
	/// of the ratings to build more accurate models.
	///
	/// They may or may not use time information at prediction (as opposed to training) time.
	/// </remarks>
	public interface ITimeAwareRatingPredictor : IRatingPredictor
	{
		/// <summary>training data that also contains the time information</summary>
		ITimedRatings TimedRatings { get; set; }

		/// <summary>predict rating at a certain point in time</summary>
		/// <param name='user_id'>the user ID</param>
		/// <param name='item_id'>the item ID</param>
		/// <param name='time'>the time of the rating event</param>
		float Predict(int user_id, int item_id, DateTime time);
	}
}
