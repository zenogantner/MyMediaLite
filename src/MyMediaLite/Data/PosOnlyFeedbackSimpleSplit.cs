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
	/// <summary>simple split for item prediction from implicit feedback</summary>
	/// <remarks>the dataset must not be modified after the split - this would lead to undefined behavior</remarks>
	public class PosOnlyFeedbackSimpleSplit<T> : ISplit<IPosOnlyFeedback> where T : IPosOnlyFeedback, new()
	{
		///
		public int NumberOfFolds { get { return 1; } }

		///
		public List<IPosOnlyFeedback> Train { get; private set; }

		///
		public List<IPosOnlyFeedback> Test { get; private set; }

		/// <summary>Create a simple split of rating prediction data</summary>
		/// <param name="feedback">the dataset</param>
		/// <param name="ratio">the ratio of ratings to use for validation</param>
		public PosOnlyFeedbackSimpleSplit(IPosOnlyFeedback feedback, double ratio)
		{
			if (ratio <= 0)
				throw new ArgumentException();

			// create train/test data structures
			var train = new T();
			var test  = new T();

			// assign indices to training or validation part
			Random random = new Random();
			foreach (int user_id in feedback.AllUsers)
				foreach (int item_id in feedback.UserMatrix[user_id])
					if (random.NextDouble() < ratio)
						test.Add(user_id, item_id);
					else
						train.Add(user_id, item_id);

			// create split data structures
			Train = new List<IPosOnlyFeedback>(NumberOfFolds);
			Test  = new List<IPosOnlyFeedback>(NumberOfFolds);
			Train.Add(train);
			Test.Add(test);
		}
	}
}
