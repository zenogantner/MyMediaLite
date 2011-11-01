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
using System.Linq;

namespace MyMediaLite.Data
{
	/// <summary>chronological split for rating prediction</summary>
	/// <remarks>the dataset must not be modified after the split - this would lead to undefined behavior</remarks>
	public class RatingsChronologicalSplit : ISplit<IRatings>
	{
		///
		public uint NumberOfFolds { get { return 1; } }

		///
		public IList<IRatings> Train { get; private set; }

		///
		public IList<IRatings> Test { get; private set; }

		/// <summary>Create a chronological split of rating prediction data</summary>
		/// <remarks>
		/// If ratings have exactly the same date and time, and they are close to the threshold between
		/// train and test, there is no guaranteed order between them (ties are broken according to how the
		/// sorting procedure sorts the ratings).
		/// </remarks>
		/// <param name="ratings">the dataset</param>
		/// <param name="ratio">the ratio of ratings to use for validation</param>
		public RatingsChronologicalSplit(ITimedRatings ratings, double ratio)
		{
			if (ratio <= 0)
				throw new ArgumentException("ratio must be greater than 0");

			var chronological_index = Enumerable.Range(0, ratings.Count).ToList();
			chronological_index.Sort(
				(a, b) => ratings.Times[a].CompareTo(ratings.Times[b])
			);

			int num_test_ratings = (int) Math.Round(ratings.Count * ratio);

			// assign indices to training part
			var train_indices = new int[ratings.Count - num_test_ratings];
			for (int i = 0; i < train_indices.Length; i++)
				train_indices[i] = chronological_index[i];

			// assign indices to test part
			var test_indices  = new int[num_test_ratings];
			for (int i = 0; i < test_indices.Length; i++)
				test_indices[i] = chronological_index[i + train_indices.Length];

			// create split data structures
			Train = new ITimedRatings[] { new TimedRatingsProxy(ratings, train_indices) };
			Test  = new ITimedRatings[] { new TimedRatingsProxy(ratings, test_indices)  };
		}
	}
}
