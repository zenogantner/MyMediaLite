// Copyright (C) 2010, 2011 Zeno Gantner
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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>simple split for rating prediction</summary>
	/// <remarks>
	///   <para>
	///     Please note that simple splits are not the best/most realistic way of evaluating
	///     recommender system algorithms.
	///     In particular, chronological splits (<see cref="RatingsChronologicalSplit"/>) are more realistic.
	///   </para>
	///
	///   <para>
	///     The dataset must not be modified after the split - this would lead to undefined behavior.
	///   </para>
	/// </remarks>
	public class RatingsSimpleSplit : ISplit<IRatings>
	{
		///
		public uint NumberOfFolds { get { return 1; } }

		///
		public IList<IRatings> Train { get; private set; }

		///
		public IList<IRatings> Test { get; private set; }

		/// <summary>Create a simple split of rating prediction data</summary>
		/// <param name="ratings">the dataset</param>
		/// <param name="ratio">the ratio of ratings to use for validation</param>
		public RatingsSimpleSplit(IRatings ratings, double ratio)
		{
			if (ratio <= 0 && ratio >= 1)
				throw new ArgumentOutOfRangeException("ratio must be between 0 and 1");

			var random_index = ratings.RandomIndex;

			int num_test_ratings = (int) Math.Round(ratings.Count * ratio);

			// assign indices to training part
			var train_indices = new int[ratings.Count - num_test_ratings];
			for (int i = 0; i < train_indices.Length; i++)
				train_indices[i] = random_index[i];

			// assign indices to test part
			var test_indices  = new int[num_test_ratings];
			for (int i = 0; i < test_indices.Length; i++)
				test_indices[i] = random_index[i + train_indices.Length];

			// create split data structures
			if (ratings is ITimedRatings)
			{
				Train = new IRatings[] { new TimedRatingsProxy((ITimedRatings) ratings, train_indices) };
				Test  = new IRatings[] { new TimedRatingsProxy((ITimedRatings) ratings, test_indices)  };
			}
			else
			{
				Train = new IRatings[] { new RatingsProxy(ratings, train_indices) };
				Test  = new IRatings[] { new RatingsProxy(ratings, test_indices)  };
			}
		}
	}
}
