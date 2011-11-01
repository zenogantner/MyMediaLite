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
	/// <remarks>
	/// <para>
	///   Chronological splits (splits according to the time of the rating) treat all ratings before
	///   a certain time as training ratings, and the ones after that time as test/validation ratings.
	///   This kind of split is the most realistic kind of split, because in a real application
	///   you also can only use past data to make predictions for the future.
	/// </para>
	///
	/// <para>
	///   The dataset must not be modified after the split - this would lead to undefined behavior.
	/// </para>
	/// </remarks>
	public class RatingsChronologicalSplit : ISplit<ITimedRatings>
	{
		///
		public uint NumberOfFolds { get { return 1; } }

		///
		public IList<ITimedRatings> Train { get; private set; }

		///
		public IList<ITimedRatings> Test { get; private set; }

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
			if (ratio <= 0 && ratio >= 1)
				throw new ArgumentOutOfRangeException("ratio must be between 0 and 1");

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

		/// <summary>Create a chronological split of rating prediction data</summary>
		/// <param name="ratings">the dataset</param>
		/// <param name="split_time">
		/// the point in time to use for splitting the data set;
		/// everything from that point on will be used for validation
		/// </param>
		public RatingsChronologicalSplit(ITimedRatings ratings, DateTime split_time)
		{
			if (split_time < ratings.EarliestTime)
				throw new ArgumentOutOfRangeException("split_time must be after the earliest event in the data set");
			if (split_time > ratings.LatestTime)
				throw new ArgumentOutOfRangeException("split_time must be before the latest event in the data set");

			// create indices
			var train_indices = new List<int>();
			var test_indices  = new List<int>();

			// assign ratings to where they belong
			for (int i = 0; i < ratings.Count; i++)
				if (ratings.Times[i] < split_time)
					train_indices.Add(i);
				else
					test_indices.Add(i);

			// create split data structures
			Train = new ITimedRatings[] { new TimedRatingsProxy(ratings, train_indices) };
			Test  = new ITimedRatings[] { new TimedRatingsProxy(ratings, test_indices)  };
		}

	}
}
