// Copyright (C) 2011 Zeno Gantner
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.Util;

namespace MyMediaLite.Eval
{
	// TODO create unit tests

	/// <summary>Class that generates a validation split for track 2 of the KDD Cup 2011</summary>
	public class Track2Validation
	{
		/// <summary>the validation candidate items for each user</summary>
		public Dictionary<int, IList<int>> Candidates { get; private set; }
		/// <summary>the positive items for each user in the validation set</summary>
		public Dictionary<int, IList<int>> Hits { get; private set; }
		/// <summary>the training part of the validation set</summary>
		public IRatings Training { get; private set; }

		// TODO check whether this needs too much memory
		/// <summary>Create a validation split</summary>
		/// <param name="ratings">the rating data</param>
		/// <param name="test_candidates">the test candidates</param>
		public Track2Validation(IRatings ratings, Dictionary<int, IList<int>> test_candidates)
		{
			// initialize the properties
			Candidates = new Dictionary<int, IList<int>>();
			Hits = new Dictionary<int, IList<int>>();

			// initialize random number generator
			var random = new System.Random();

			int counter = 0;

			// for each user in the test set, randomly sample three items scored 80 or higher,
			// and three that were not
			foreach (int user_id in test_candidates.Keys)
			{
				// create a set of all positively rated items by the user
				var user_pos_items = new HashSet<int>();
				foreach (int index in ratings.ByUser[user_id])
					if (ratings[index] >= 80)
						user_pos_items.Add(ratings.Items[index]);

				// abort this user if we do not have enough positive items
				if (user_pos_items.Count < 3)
					continue;

				var user_pos_items_array = user_pos_items.ToArray();

				// sample positive items
				var sampled_pos_items = new HashSet<int>();
				while (sampled_pos_items.Count < 3)
				{
					int random_item = user_pos_items_array[random.Next(0, user_pos_items_array.Length - 1)];
					sampled_pos_items.Add(random_item);
				}

				// sample negative items
				var sampled_neg_items = new HashSet<int>();
				while (sampled_neg_items.Count < 3)
				{
					int random_item = ratings.Items[random.Next(0, ratings.Count - 1)];
					if (!user_pos_items.Contains(random_item))
						sampled_neg_items.Add(random_item);
				}

				// add to data structure
				Hits[user_id] = sampled_pos_items.ToArray();
				Candidates[user_id] = sampled_pos_items.Union(sampled_neg_items).ToArray();

				if (counter % 100 == 0)
					Console.Error.Write(".");

				if (counter++ % 1000 == 0)
					Console.Error.WriteLine("{0}: {1}", user_id, Memory.Usage);
			}
			Console.WriteLine("{0} users in validation set, {1} in test.", Hits.Count, test_candidates.Count);

			var left_out_indices = new int[Hits.Count * 3];
			Console.Error.WriteLine("left_out_indices {0}", Memory.Usage);
			int pos = 0;
			foreach (int user_id in Hits.Keys)
				foreach (int item_id in Hits[user_id])
					left_out_indices[pos++] = ratings.GetIndex(user_id, item_id, ratings.ByUser[user_id]);

			// create training part of the ratings (without the validation items)
			var kept_indices = ratings.RandomIndex.Except(left_out_indices).ToArray();
			Console.Error.WriteLine("kept_indices {0}", Memory.Usage);
			left_out_indices = null;
			Console.Error.WriteLine("rm left_out_indices {0}", Memory.Usage);
			Training = new RatingsProxy(ratings, kept_indices);
			Console.Error.WriteLine("training {0}", Memory.Usage);
		}
	}
}

