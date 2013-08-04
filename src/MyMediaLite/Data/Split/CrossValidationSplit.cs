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
	/// <summary>k-fold  cross-validation split</summary>
	/// <remarks>
	///   <para>
	///     Please note that k-fold cross-validation is not the best/most realistic way of evaluating
	///     recommender system algorithms.
	///     In particular, chronological splits (<see cref="ChronologicalSplit"/>) are more realistic.
	///   </para>
	///
	///   <para>
	///     The dataset must not be modified after the split -- this would lead to undefined behavior.
	///   </para>
	/// </remarks>
	public class CrossValidationSplit : ISplit<IInteractions>
	{
		///
		public uint NumberOfFolds { get { return number_of_folds; } }
		private readonly uint number_of_folds;

		///
		public IList<IInteractions> Train { get; private set; }

		///
		public IList<IInteractions> Test { get; private set; }

		/// <summary>Create a k-fold split of rating prediction data</summary>
		/// <param name="interactions">the dataset</param>
		/// <param name="number_of_folds">the number of folds</param>
		public CrossValidationSplit(IInteractions interactions, uint number_of_folds)
		{
			if (number_of_folds < 2)
				throw new ArgumentOutOfRangeException("number_of_folds must be at least 2.");
			this.number_of_folds = number_of_folds;
			
			var ia = interactions as Interactions;
			if (ia == null)
				throw new NotImplementedException("");
			var interaction_list = ia.RandomInteractionList;

			// create index lists
			IList<int>[] train_indices = new List<int>[number_of_folds];
			IList<int>[] test_indices  = new List<int>[number_of_folds];

			for (int i = 0; i < number_of_folds; i++)
			{
				train_indices[i] = new List<int>();
				test_indices[i]  = new List<int>();
			}

			// assign indices to folds
			foreach (int i in Enumerable.Range(0, interaction_list.Count))
				for (int j = 0; j < number_of_folds; j++)
					if (j == i % number_of_folds)
						test_indices[j].Add(i);
					else
						train_indices[j].Add(i);

			// create split data structures
			Train = new List<IInteractions>((int) number_of_folds);
			Test  = new List<IInteractions>((int) number_of_folds);
			for (int i = 0; i < number_of_folds; i++)
			{
				Train.Add(new Interactions(new ListProxy<IInteraction>(interaction_list, train_indices[i])));
				Test.Add(new Interactions(new ListProxy<IInteraction>(interaction_list, test_indices[i])));
			}
		}
	}
}
