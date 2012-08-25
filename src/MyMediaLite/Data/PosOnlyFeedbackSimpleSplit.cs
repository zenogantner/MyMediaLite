// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
	/// <remarks>
	/// The dataset must not be modified after the split - this would lead to undefined behavior.
	/// </remarks>
	public class PosOnlyFeedbackSimpleSplit<T> : ISplit<IPosOnlyFeedback> where T : IPosOnlyFeedback, new()
	{
		///
		public uint NumberOfFolds { get { return 1; } }

		///
		public IList<IPosOnlyFeedback> Train { get; private set; }

		///
		public IList<IPosOnlyFeedback> Test { get; private set; }

		/// <summary>Create a simple split of positive-only item prediction data</summary>
		/// <param name="feedback">the dataset</param>
		/// <param name="ratio">the ratio of positive events to use for validation</param>
		public PosOnlyFeedbackSimpleSplit(IPosOnlyFeedback feedback, double ratio)
		{
			if (ratio <= 0)
				throw new ArgumentException("ratio must be greater than 0");

			// create train/test data structures
			var Train = new T();
			var Test  = new T();

			// assign indices to training or validation part
			Random random = MyMediaLite.Random.GetInstance();
			foreach (int index in feedback.RandomIndex)
				if (random.NextDouble() < ratio)
					Test.Add(feedback.Users[index], feedback.Items[index]);
				else
					Train.Add(feedback.Users[index], feedback.Items[index]);
			
			this.Train = new IPosOnlyFeedback[] { Train };
			this.Test  = new IPosOnlyFeedback[] { Test  };
		}
	}
}
