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
	/// <remarks>the dataset must not be modified after the split - this would lead to undefined behavior</remarks>
	public class RatingsSimpleSplit : ISplit<IRatings>
	{
		///
		public int NumberOfFolds { get { return 1; } }

		///
		public List<IRatings> Train { get; private set; }

		///
		public List<IRatings> Test { get; private set; }

		/// <summary>Create a simple split of rating prediction data</summary>
		/// <param name="ratings">the dataset</param>
		/// <param name="ratio">the ratio of ratings to use for validation</param>
		public RatingsSimpleSplit(IRatings ratings, double ratio)
		{
			if (ratio <= 0)
				throw new ArgumentException("ratio");

			// create index lists
			var train_indices = new List<int>();
			var test_indices  = new List<int>();

			// assign indices to training or validation part
			Random random = new Random();
			for (int i = 0; i < ratings.Count; i++)
				if (random.NextDouble() < ratio)
					test_indices.Add(i);
				else
					train_indices.Add(i);

			// create split data structures
			Train = new List<IRatings>(NumberOfFolds);
			Test  = new List<IRatings>(NumberOfFolds);
			Train.Add(new RatingsProxy(ratings, train_indices));
			Test.Add(new RatingsProxy(ratings, test_indices));
		}
	}
}
