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
//

using System;
using MyMediaLite.Data;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Abstract class for time-aware rating predictors</summary>
	/// <exception cref='ArgumentException'>
	/// Is thrown when an argument passed to a method is invalid.
	/// </exception>
	public abstract class TimeAwareRatingPredictor : RatingPredictor, ITimeAwareRatingPredictor
	{
		/// <summary>the rating data, including time information</summary>
		public virtual ITimedRatings TimedRatings
		{
			get { return timed_ratings; }
			set {
				timed_ratings = value;
				Ratings = value;
			}
		}
		/// <summary>rating data, including time information</summary>
		protected ITimedRatings timed_ratings;

		///
		public override IRatings Ratings
		{
			get { return ratings; }
			set {
				if (!(value is ITimedRatings))
					throw new ArgumentException("Ratings must be of type ITimedRatings.");

				base.Ratings = value;
				timed_ratings = (ITimedRatings) value;
			}
		}

		///
		public abstract float Predict(int user_id, int item_id, DateTime time);
	}
}

