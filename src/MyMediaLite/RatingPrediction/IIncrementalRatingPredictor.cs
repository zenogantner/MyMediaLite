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

using System.Collections.Generic;
using MyMediaLite.Data;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Interface for rating predictors which support incremental training</summary>
	/// <remarks>
	/// By incremental training we mean that after each update, the recommender does not
	/// perform a complete re-training using all data, but only a brief update procedure
	/// taking into account the update and only a subset of the existing training data.
	///
	/// This interface does not prevent you from doing a complete re-training when implementing
	/// a new class. This makes sense e.g. for simple average-based models.
	///
	/// This interface assumes that every user can rate every item only once.
	/// </remarks>
	public interface IIncrementalRatingPredictor : IRatingPredictor, IIncrementalRecommender
	{
		/// <summary>Add new ratings and perform incremental training</summary>
		/// <param name='ratings'>the ratings</param>
		void AddRatings(IRatings ratings);

		/// <summary>Update existing ratings and perform incremental training</summary>
		/// <param name='ratings'>the ratings</param>
		void UpdateRatings(IRatings ratings);

		/// <summary>Remove existing ratings and perform "incremental" training</summary>
		/// <param name='ratings'>the user and item IDs of the ratings to be removed</param>
		void RemoveRatings(IDataSet ratings);
	}
}
