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

using System.Collections.Generic;

namespace MyMediaLite.Data
{
	/// <summary>k-fold split for rating prediction</summary>
	/// <remarks>the dataset must not be modified after the split - this would lead to undefined behavior</remarks>
	public class RatingCrossValidationSplit : ISplit<IRatings>
	{
		///
		public int NumberOfFolds { get; private set; }

		///
		public List<IRatings> Train { get; private set; }

		///
		public List<IRatings> Test { get; private set; }

		/// <summary>Create a k-fold split of rating prediction data</summary>
		/// <param name="ratings">the dataset</param>
		/// <param name="num_folds">the number of folds</param>
		public RatingCrossValidationSplit(IRatings ratings, int num_folds)
		{
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
			Train = new List<IRatings>(num_folds);
			Test  = new List<IRatings>(num_folds);
			for (int i = 0; i < num_folds; i++)
			{
				Train.Add(new RatingsProxy(ratings, train_indices[i]));
				Test.Add(new RatingsProxy(ratings, test_indices[i]));
			}
		}
	}
}
