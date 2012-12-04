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
	public class PosOnlyFeedbackAllButOneSplit<T> : ISplit<IPosOnlyFeedback> where T : IPosOnlyFeedback, new()
	{
		///
		public uint NumberOfFolds { get { return 1; } }

		///
		public IList<IPosOnlyFeedback> Train { get; private set; }

		///
		public IList<IPosOnlyFeedback> Test { get; private set; }
		
		///
		public IList<IList<int>> Hidden { get; private set; }
		
		///
		public Random rand;

		/// <summary>Create a simple split of positive-only item prediction data</summary>
		/// <param name="feedback">the dataset</param>
		/// <param name="ratio">the ratio of positive events to use for validation</param>
		public PosOnlyFeedbackAllButOneSplit(IPosOnlyFeedback feedback, double ratio, bool keep_order, int random_seed)
		{
			rand = new Random(seed);
			if (ratio <= 0)
				throw new ArgumentException("ratio must be greater than 0");

			// create train/test data structures
			var Train = new T();
			var Test  = new T();
			var Hidden = new List<IList<int>>();
			
			if(keep_order) {
				var split = new PosOnlyFeedbackKeepOrderSplit<PosOnlyFeedback<SparseBooleanMatrix>>(feedback, ratio);
			} else {
				var split = new PosOnlyFeedbackSimpleSplit<PosOnlyFeedback<SparseBooleanMatrix>>(feedback, ratio);
			}
			
			this.Train = split.Train;
			this.Test = split.Test;

			foreach(T fold in this.Test) {
				var by_user = fold.ByUser;
				var hidden_set = new List<int>(by_user.Count);
				int cnt, random_index;
				foreach(List<int> items in by_user) {
					cnt = items.Count;
					random_index = rand.Next(0, cnt - 1);
					hidden_set.Add(items[random_index]);
				}
				this.Hidden.Add(hidden_set);
			}	
		}
	}
}
