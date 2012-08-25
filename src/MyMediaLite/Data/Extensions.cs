// Copyright (C) 2011, 2012 Zeno Gantner
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
using System.Globalization;
using System.Linq;
using MyMediaLite.DataType;

namespace MyMediaLite.Data
{
	/// <summary>Extension methods for dataset statistics</summary>
	public static class Extensions
	{
		/// <summary>Display dataset statistics</summary>
		/// <param name="train">the training data</param>
		/// <param name="test">the test data</param>
		/// <param name="user_attributes">the user attributes</param>
		/// <param name="item_attributes">the item attributes</param>
		/// <param name="display_overlap">if set true, display the user/item overlap between train and test</param>
		public static string Statistics(
			this IRatings train, IRatings test = null,
			IBooleanMatrix user_attributes = null, IBooleanMatrix item_attributes = null,
			bool display_overlap = false)
		{
			// training data stats
			int num_users = train.AllUsers.Count;
			int num_items = train.AllItems.Count;
			long matrix_size = (long) num_users * num_items;
			long empty_size  = (long) matrix_size - train.Count;
			double sparsity = (double) 100L * empty_size / matrix_size;
			string s = string.Format(CultureInfo.InvariantCulture, "training data: {0} users, {1} items, {2} ratings, sparsity {3,0:0.#####}\n", num_users, num_items, train.Count, sparsity);
			if (train is ITimedRatings)
			{
				var time_train = train as ITimedRatings;
				s += string.Format(CultureInfo.InvariantCulture, "rating period: {0} to {1}\n", time_train.EarliestTime, time_train.LatestTime);
			}

			// test data stats
			if (test != null)
			{
				num_users = test.AllUsers.Count;
				num_items = test.AllItems.Count;
				matrix_size = (long) num_users * num_items;
				empty_size  = (long) matrix_size - test.Count; // TODO depends on the eval scheme whether this is correct
				sparsity = (double) 100L * empty_size / matrix_size;
				s += string.Format(CultureInfo.InvariantCulture, "test data:     {0} users, {1} items, {2} ratings, sparsity {3,0:0.#####}\n", num_users, num_items, test.Count, sparsity);
				if (test is ITimedRatings)
				{
					var time_test = test as ITimedRatings;
					s += string.Format(CultureInfo.InvariantCulture, "rating period: {0} to {1}\n", time_test.EarliestTime, time_test.LatestTime);
				}
			}

			// count and display the overlap between train and test
			if (display_overlap && test != null)
			{
				int num_new_users = 0;
				int num_new_items = 0;
				TimeSpan seconds = Wrap.MeasureTime(delegate() {
					num_new_users = test.AllUsers.Except(train.AllUsers).Count();
					num_new_items = test.AllItems.Except(train.AllItems).Count();
				});
				s += string.Format("{0} new users, {1} new items ({2} seconds)\n", num_new_users, num_new_items, seconds);
			}

			return s + Statistics(user_attributes, item_attributes);
		}

		/// <summary>Display data statistics for item recommendation datasets</summary>
		/// <param name="training_data">the training dataset</param>
		/// <param name="test_data">the test dataset</param>
		/// <param name="user_attributes">the user attributes</param>
		/// <param name="item_attributes">the item attributes</param>
		public static string Statistics(
			this IPosOnlyFeedback training_data, IPosOnlyFeedback test_data = null,
			IBooleanMatrix user_attributes = null, IBooleanMatrix item_attributes = null)
		{
			// training data stats
			int num_users = training_data.AllUsers.Count;
			int num_items = training_data.AllItems.Count;
			long matrix_size = (long) num_users * num_items;
			long empty_size  = (long) matrix_size - training_data.Count;
			double sparsity = (double) 100L * empty_size / matrix_size;
			string s = string.Format(CultureInfo.InvariantCulture, "training data: {0} users, {1} items, {2} events, sparsity {3,0:0.#####}\n", num_users, num_items, training_data.Count, sparsity);

			// test data stats
			if (test_data != null)
			{
				num_users = test_data.AllUsers.Count;
				num_items = test_data.AllItems.Count;
				matrix_size = (long) num_users * num_items;
				empty_size  = (long) matrix_size - test_data.Count;
				sparsity = (double) 100L * empty_size / matrix_size; // TODO depends on the eval scheme whether this is correct
				s += string.Format(CultureInfo.InvariantCulture, "test data:     {0} users, {1} items, {2} events, sparsity {3,0:0.#####}\n", num_users, num_items, test_data.Count, sparsity);
			}

			return s + Statistics(user_attributes, item_attributes);
		}

		/// <summary>Display statistics for user and item attributes</summary>
		/// <param name="user_attributes">the user attributes</param>
		/// <param name="item_attributes">the item attributes</param>
		public static string Statistics(IBooleanMatrix user_attributes, IBooleanMatrix item_attributes)
		{
			string s = string.Empty;
			if (user_attributes != null)
			{
				s += string.Format(
					"{0} user attributes for {1} users, {2} assignments, {3} users with attribute assignments\n",
					user_attributes.NumberOfColumns, user_attributes.NumberOfRows,
					user_attributes.NumberOfEntries, user_attributes.NonEmptyRowIDs.Count);
			}
			if (item_attributes != null)
				s += string.Format(
					"{0} item attributes for {1} items, {2} assignments, {3} items with attribute assignments\n",
					item_attributes.NonEmptyColumnIDs.Count, item_attributes.NumberOfRows,
					item_attributes.NumberOfEntries, item_attributes.NonEmptyRowIDs.Count);
			return s;
		}
	}
}
