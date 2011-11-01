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
	/// <summary>per-user chronological split for rating prediction</summary>
	/// <remarks>
	/// <para>
	///   Chronological splits (splits according to the time of the rating) treat all ratings before
	///   a certain time as training ratings, and the ones after that time as test/validation ratings.
	/// </para>
	/// 
	/// <para>
	///   Here, the split date may differ from user to user.
	///   In the constructor, you can either specify which part (ratio) or how many of a user's rating
	///   are supposed to be used for validation.
	/// </para>
	/// 
	/// <para>
	///   The dataset must not be modified after the split - this would lead to undefined behavior.
	/// </para>
	/// </remarks>
	public class RatingsPerUserChronologicalSplit : ISplit<ITimedRatings>
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
		/// <param name="ratio">the ratio of ratings to use for validation (per user)</param>
		public RatingsPerUserChronologicalSplit(ITimedRatings ratings, double ratio)
		{
			if (ratio <= 0 && ratio >= 1)
				throw new ArgumentOutOfRangeException("ratio must be between 0 and 1");
			
			var train_indices = new List<int>();
			var test_indices  = new List<int>();
			
			// for every user, perform the split and assign the ratings accordingly
			foreach (int u in ratings.AllUsers)
			{
				var chronological_index = ratings.ByUser[u].ToList();
				chronological_index.Sort(
					(a, b) => ratings.Times[a].CompareTo(ratings.Times[b])
				);
	
				int num_test_ratings  = (int) Math.Round(ratings.ByUser[u].Count * ratio);
				int num_train_ratings = ratings.ByUser[u].Count - num_test_ratings;
	
				// assign indices to training part
				for (int i = 0; i < num_train_ratings; i++)
					train_indices.Add(chronological_index[i]);
	
				// assign indices to test part
				for (int i = 0; i < num_test_ratings; i++)
					test_indices.Add(chronological_index[i + num_train_ratings]);
			}

			// create split data structures
			Train = new ITimedRatings[] { new TimedRatingsProxy(ratings, train_indices) };
			Test  = new ITimedRatings[] { new TimedRatingsProxy(ratings, test_indices)  };
		}
		
		/// <summary>Create a chronological split of rating prediction data</summary>
		/// <remarks>
		/// If ratings have exactly the same date and time, and they are close to the threshold between
		/// train and test, there is no guaranteed order between them (ties are broken according to how the
		/// sorting procedure sorts the ratings).
		/// </remarks>
		/// <param name="ratings">the dataset</param>
		/// <param name="num_test_ratings_per_user">the number of test ratings (per user)</param>
		public RatingsPerUserChronologicalSplit(ITimedRatings ratings, int num_test_ratings_per_user)
		{
			var train_indices = new List<int>();
			var test_indices  = new List<int>();
			
			// for every user, perform the split and assign the ratings accordingly
			foreach (int u in ratings.AllUsers)
			{
				var chronological_index = ratings.ByUser[u].ToList();
				chronological_index.Sort(
					(a, b) => ratings.Times[a].CompareTo(ratings.Times[b])
				);
	
				int num_test_ratings  = Math.Min(num_test_ratings_per_user, ratings.ByUser[u].Count);
				int num_train_ratings = ratings.ByUser[u].Count - num_test_ratings;
	
				// assign indices to training part
				for (int i = 0; i < num_train_ratings; i++)
					train_indices.Add(chronological_index[i]);
	
				// assign indices to test part
				for (int i = 0; i < num_test_ratings; i++)
					test_indices.Add(chronological_index[i + num_train_ratings]);
			}

			// create split data structures
			Train = new ITimedRatings[] { new TimedRatingsProxy(ratings, train_indices) };
			Test  = new ITimedRatings[] { new TimedRatingsProxy(ratings, test_indices)  };
		}
	}
}
