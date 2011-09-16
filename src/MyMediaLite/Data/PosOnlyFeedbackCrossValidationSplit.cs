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
	/// <summary>K-fold cross-validation split for item prediction from implicit feedback</summary>
	/// <remarks>
	/// Items with less than k events associated are ignored for testing and always assigned to the training set.
	///
	/// The dataset must not be modified after the split - this would lead to undefined behavior.
	/// </remarks>
	public class PosOnlyFeedbackCrossValidationSplit<T> : ISplit<IPosOnlyFeedback> where T : IPosOnlyFeedback, new()
	{
		///
		public uint NumberOfFolds { get; private set; }

		///
		public IList<IPosOnlyFeedback> Train { get; private set; }

		///
		public IList<IPosOnlyFeedback> Test { get; private set; }

		/// <summary>Create a k-fold split of positive-only item prediction data</summary>
		/// <param name="feedback">the dataset</param>
		/// <param name="num_folds">the number of folds</param>
		public PosOnlyFeedbackCrossValidationSplit(IPosOnlyFeedback feedback, uint num_folds)
		{
			if (num_folds < 2)
				throw new ArgumentException("num_folds must be at least 2.");

			NumberOfFolds = num_folds;
			Train = new T[num_folds];
			Test  = new T[num_folds];
			for (int f = 0; f < num_folds; f++)
			{
				Train[f] = new T();
				Test[f]  = new T();
			}

			// assign events to folds
			int pos = 0;
			Console.Error.WriteLine("{0} items", feedback.AllItems.Count);
			foreach (int item_id in feedback.AllItems)
			{
				var item_users = new List<int>(feedback.ItemMatrix[item_id]);
				if (item_users.Count < num_folds)
				{
					foreach (int user_id in item_users)
						for (int f = 0; f < num_folds; f++)
							Train[f].Add(user_id, item_id);
				}
				else
				{
					// shuffle list for randomness
					MyMediaLite.Util.Utils.Shuffle(item_users);

					for (int index = 0; index < item_users.Count; index++)
					{
						int user_id = item_users[index];
						for (int f = 0; f < num_folds; f++)
							if (pos % num_folds == f)
								Test[f].Add(user_id, item_id);
							else
								Train[f].Add(user_id, item_id);
						pos++;
					}
				}
			}
		}
	}
}
