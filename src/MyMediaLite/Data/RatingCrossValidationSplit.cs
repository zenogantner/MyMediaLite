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
	public class RatingCrossValidationSplit : ISplit<Ratings>
	{
		/// <inheritdoc/>
		public int NumberOfFolds { get; private set; }

		/// <inheritdoc/>
		public List<Ratings> Train { get; private set; }

		/// <inheritdoc/>
		public List<Ratings> Test { get; private set; }

		/// <summary>Create a k-fold split of rating prediction data</summary>
		/// <param name="rating_data">the dataset</param>
		/// <param name="num_folds">the number of folds</param>
		public RatingCrossValidationSplit(Ratings rating_data, int num_folds)
		{
			NumberOfFolds = num_folds;
			Train = new List<Ratings>(num_folds);
			Test  = new List<Ratings>(num_folds);

			// create data structures
			for (int i = 0; i < num_folds; i++)
			{
				Train.Add(new RatingData());
				Test.Add(new RatingData());
			}

			// randomize
			rating_data.Shuffle();

			// assign ratings to folds
			for (int i = 0; i < rating_data.All.Count; i++)
				for (int j = 0; j < num_folds; j++)
					if (j == i % num_folds)
						Test[j].AddRating(rating_data.All[i]);
					else
						Train[j].AddRating(rating_data.All[i]);
		}
	}
}
