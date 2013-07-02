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

namespace MyMediaLite.Data.Split
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
	public class ChronologicalSplit : SimpleSplit
	{
		/// <summary>Create a chronological split of interaction data</summary>
		/// <remarks>
		/// If interactions have exactly the same date and time, and they are close to the threshold between
		/// train and test, there is no guaranteed order between them (ties are broken according to how the
		/// sorting procedure sorts the interactions).
		/// </remarks>
		/// <param name="interactions">the dataset</param>
		/// <param name="ratio">the ratio of ratings to use for validation</param>
		public ChronologicalSplit(IInteractions interactions, double ratio)
		{
			var ia = interactions as Interactions;
			if (ia == null)
				throw new NotImplementedException();
			Init(ia.ChronologicalInteractionList, ratio);
		}

		/// <summary>Create a chronological split of an interaction dataset</summary>
		/// <param name="interactions">the dataset</param>
		/// <param name="split_time">
		/// the point in time to use for splitting the data set;
		/// everything from that point on will be used for validation
		/// </param>
		public ChronologicalSplit(IInteractions interactions, DateTime split_time)
		{
			if (split_time < interactions.EarliestDateTime)
				throw new ArgumentOutOfRangeException(
					string.Format("split_time {0} must be after the earliest event {1} in the data set.", split_time, interactions.EarliestDateTime));
			if (split_time > interactions.LatestDateTime)
				throw new ArgumentOutOfRangeException(
					string.Format("split_time {0} must be before the latest event {1} in the data set.", split_time, interactions.LatestDateTime));

			var train_list = new List<IInteraction>();
			var test_list  = new List<IInteraction>();
			
			var ia = interactions as Interactions;
			if (ia == null)
				throw new NotImplementedException();
			// assign interactions to where they belong
			foreach (var interaction in ia.InteractionList)
				if (interaction.DateTime < split_time)
					train_list.Add(interaction);
				else
					test_list.Add(interaction);

			// create split data structures
			Train = new IInteractions[] { new Interactions(train_list) };
			Test  = new IInteractions[] { new Interactions(test_list)  };
		}

	}
}
