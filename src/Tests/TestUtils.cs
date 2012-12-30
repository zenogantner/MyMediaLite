// Copyright (C) 2012 Zeno Gantner
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
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace Tests
{
	public static class TestUtils
	{
		public static IRatings CreateRandomRatings(int num_users, int num_items, int num_ratings)
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
			return ratings;
		}

		public static IRatings CreateRandomTimedRatings(int num_users, int num_items, int num_ratings)
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
			return ratings;
		}

		public static IRatings CreateRatings()
		{
			var ratings = new Ratings();
			ratings.Add(0, 0, 0.0f);
			return ratings;
		}

		public static IPosOnlyFeedback CreatePosOnlyFeedback()
		{
			var feedback = new PosOnlyFeedback<SparseBooleanMatrix>();
			feedback.Add(0, 0);
			feedback.Add(0, 1);
			feedback.Add(1, 0);
			feedback.Add(1, 2);
			return feedback;
		}
	}
}

