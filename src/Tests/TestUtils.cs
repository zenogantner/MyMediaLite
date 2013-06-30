// Copyright (C) 2012, 2013 Zeno Gantner
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
//
using System;
using System.Collections.Generic;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace Tests
{
	public static class TestUtils
	{
		public static IInteractions CreateRandomRatings(int num_users, int num_items, int num_ratings)
		{
			var random = MyMediaLite.Random.GetInstance();

			var ratings = new Ratings();
			for (int i = 0; i < num_ratings; i++)
			{
				int user_id = random.Next(num_users);
				int item_id = random.Next(num_items);
				int rating_value = 1 + random.Next(5);
				ratings.Add(user_id, item_id, rating_value);
			}
			return new MemoryInteractions(ratings);
		}

		public static IInteractions CreateRandomTimedRatings(int num_users, int num_items, int num_ratings)
		{
			var random = MyMediaLite.Random.GetInstance();

			var ratings = new TimedRatings();
			for (int i = 0; i < num_ratings; i++)
			{
				int user_id = random.Next(num_users);
				int item_id = random.Next(num_items);
				int rating_value = 1 + random.Next(5);
				ratings.Add(user_id, item_id, rating_value, DateTime.Now);
			}
			return new MemoryInteractions(ratings);
		}

		public static IInteractions CreateRatings()
		{
			var ratings = new Ratings();
			ratings.Add(0, 0, 0.0f);
			return new MemoryInteractions(ratings);
		}
		
		public static IInteractions CreateFeedback(IList<Tuple<int, int>> interaction_pairs)
		{
			var interaction_list = new List<IInteraction>();
			foreach (var pair in interaction_pairs)
				interaction_list.Add(new SimpleInteraction(pair.Item1, pair.Item2));
			return new Interactions(interaction_list);
		}

		public static IInteractions CreateFeedback()
		{
			Tuple<int, int>[] interaction_pairs = new Tuple<int, int>[] {
				Tuple.Create(0, 0),
				Tuple.Create(0, 1),
				Tuple.Create(1, 0),
				Tuple.Create(1, 2),
			};
			return CreateFeedback(interaction_pairs);
		}
	}
}

