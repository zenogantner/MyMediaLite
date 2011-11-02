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
	/// <summary>k-fold  cross-validation split for rating prediction</summary>
	/// <remarks>
	///   <para>
	///     Please note that k-fold cross-validation is not the best/most realistic way of evaluating
	///     recommender system algorithms.
	///     In particular, chronological splits (<see cref="RatingsChronologicalSplit"/>) are more realistic.
	///   </para>
	///
	///   <para>
	///     The dataset must not be modified after the split - this would lead to undefined behavior.
	///   </para>
	/// </remarks>
	public class RatingCrossValidationSplit : ISplit<IRatings>
	{
		///
		public uint NumberOfFolds { get; private set; }

		///
		public IList<IRatings> Train { get; private set; }

		///
		public IList<IRatings> Test { get; private set; }

		/// <summary>Create a k-fold split of rating prediction data</summary>
		/// <param name="ratings">the dataset</param>
		/// <param name="num_folds">the number of folds</param>
		public RatingCrossValidationSplit(IRatings ratings, uint num_folds)
		{
			if (num_folds < 2)
				throw new ArgumentOutOfRangeException("num_folds must be at least 2.");
			
			NumberOfFolds = num_folds;

			// randomize
			IList<int> random_indices = ratings.RandomIndex;

			// create index lists
			List<int>[] train_indices = new List<int>[num_folds];
			List<int>[] test_indices  = new List<int>[num_folds];

			for (int i = 0; i < num_folds; i++)
			{
				train_indices[i] = new List<int>();
				test_indices[i]  = new List<int>();
			}

			// assign indices to folds
			foreach (int i in random_indices)
				for (int j = 0; j < num_folds; j++)
					if (j == i % num_folds)
						test_indices[j].Add(i);
					else
						train_indices[j].Add(i);

			// create split data structures
			Train = new List<IRatings>((int) num_folds);
			Test  = new List<IRatings>((int) num_folds);
			for (int i = 0; i < num_folds; i++)
				if (ratings is ITimedRatings)
				{
					Train.Add(new TimedRatingsProxy((ITimedRatings) ratings, train_indices[i]));
					Test.Add(new TimedRatingsProxy((ITimedRatings) ratings, test_indices[i]));
				}
				else
				{
					Train.Add(new RatingsProxy(ratings, train_indices[i]));
					Test.Add(new RatingsProxy(ratings, test_indices[i]));
				}
		}
	}
}
