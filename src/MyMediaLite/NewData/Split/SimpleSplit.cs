// Copyright (C) 2010, 2011, 2013 Zeno Gantner
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
using MyMediaLite.DataType;

namespace MyMediaLite.Data.Split
{
	/// <summary>simple split for rating prediction</summary>
	/// <remarks>
	///   <para>
	///     Please note that simple splits are not the best/most realistic way of evaluating
	///     recommender system algorithms.
	///     In particular, chronological splits (<see cref="RatingsChronologicalSplit"/>) are more realistic.
	///   </para>
	///
	///   <para>
	///     The dataset must not be modified after the split - this would lead to undefined behavior.
	///   </para>
	/// </remarks>
	public class SimpleSplit : ISplit<IInteractions>
	{
		///
		public uint NumberOfFolds { get { return 1; } }

		///
		public IList<IInteractions> Train { get; private set; }

		///
		public IList<IInteractions> Test { get; private set; }

		/// <summary>Create a simple split of rating prediction data</summary>
		/// <param name="interactions">the dataset</param>
		/// <param name="ratio">the ratio of ratings to use for validation</param>
		public SimpleSplit(IInteractions interactions, double ratio)
		{
			if (ratio <= 0 && ratio >= 1)
				throw new ArgumentOutOfRangeException("ratio must be between 0 and 1");
			
			var ia = interactions as Interactions;
			if (ia == null)
				throw new NotImplementedException("");
			var random_interaction_list = ia.RandomInteractionList;

			int num_test_entries = (int) Math.Round(interactions.Count * ratio);
			int num_train_entries = interactions.Count - num_test_entries;

			// assign indices to training part
			var train_indices = Enumerable.Range(0, num_train_entries).ToArray();
			var test_indices  = Enumerable.Range(num_train_entries, num_test_entries).ToArray();

			// create split data structures
			Train = new IInteractions[] { new Interactions(new ListProxy<IInteraction>(random_interaction_list, train_indices)) };
			Test  = new IInteractions[] { new Interactions(new ListProxy<IInteraction>(random_interaction_list, test_indices))  };
		}
	}
}
