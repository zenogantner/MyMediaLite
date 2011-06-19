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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using MyMediaLite.Data;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Util
{
	/// <summary>Class containing utility functions</summary>
	public static class Utils
	{
		// TODO add memory constraints and a replacement strategy
		/// <summary>Memoize a function</summary>
		/// <param name="f">The function to memoize</param>
		/// <returns>a version of the function that remembers past function results</returns>
		public static Func<A, R> Memoize<A, R>(this Func<A, R> f)
		{
  			var map = new Dictionary<A, R>();
  			return a =>
			{
	  			R value;
	  			if (map.TryGetValue(a, out value))
				return value;
	  			value = f(a);
	  			map.Add(a, value);
	  			return value;
			};
		}

		/// <summary>Delegate definition necessary to define MeasureTime</summary>
		public delegate void task();

		/// <summary>Measure how long an action takes</summary>
		/// <param name="t">A <see cref="task"/> defining the action to be measured</param>
		/// <returns>The <see cref="TimeSpan"/> it takes to perform the action</returns>
		public static TimeSpan MeasureTime(task t)
		{
			DateTime startTime = DateTime.Now;
			t();
			return DateTime.Now - startTime;
		}

		/// <summary>Read a list of integers from a StreamReader</summary>
		/// <param name="reader">the <see cref="StreamReader"/> to be read from</param>
		/// <returns>a list of integers</returns>
		public static IList<int> ReadIntegers(StreamReader reader)
		{
			var numbers = new List<int>();

			while (!reader.EndOfStream)
				numbers.Add(int.Parse( reader.ReadLine() ));

			return numbers;
		}

		/// <summary>Read a list of integers from a file</summary>
		/// <param name="filename">the name of the file to be read from</param>
		/// <returns>a list of integers</returns>
		public static IList<int> ReadIntegers(string filename)
		{
			using ( var reader = new StreamReader(filename) )
				return ReadIntegers(reader);
		}

		/// <summary>Shuffle a list in-place</summary>
		/// <remarks>
		/// Fisher-Yates shuffle, see
		/// http://en.wikipedia.org/wiki/Fisher–Yates_shuffle
		/// </remarks>
		public static void Shuffle<T>(IList<T> list)
		{
			Random random = MyMediaLite.Util.Random.GetInstance();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				int r = random.Next(0, i + 1);

				// swap position i with position r
				T tmp = list[i];
				list[i] = list[r];
				list[r] = tmp;
			}
		}

		/// <summary>Get all types of a namespace</summary>
		/// <param name="name_space">a string describing the namespace</param>
		/// <returns>an array of Type objects</returns>
		public static Type[] GetTypesInNamespace(string name_space)
		{
			var types = new List<Type>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				types.AddRange( assembly.GetTypes().Where(t => string.Equals(t.Namespace, name_space, StringComparison.Ordinal)) );

			return types.ToArray();
		}

		/// <summary>Display dataset statistics</summary>
		/// <param name="train">the training data</param>
		/// <param name="test">the test data</param>
		/// <param name="recommender">the recommender (to get attribute information)</param>
		public static void DisplayDataStats(IRatings train, IRatings test, RatingPredictor recommender)
		{
			DisplayDataStats(train, test, recommender, false);
		}

		// TODO get rid of recommender argument
		/// <summary>Display dataset statistics</summary>
		/// <param name="train">the training data</param>
		/// <param name="test">the test data</param>
		/// <param name="recommender">the recommender (to get attribute information)</param>
		/// <param name="display_overlap">if set true, display the user/item overlap between train and test</param>
		public static void DisplayDataStats(IRatings train, IRatings test, RatingPredictor recommender, bool display_overlap)
		{
			// training data stats
			int num_users = train.AllUsers.Count;
			int num_items = train.AllItems.Count;
			long matrix_size = (long) num_users * num_items;
			long empty_size  = (long) matrix_size - train.Count;
			double sparsity = (double) 100L * empty_size / matrix_size;
			Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "training data: {0} users, {1} items, {2} ratings, sparsity {3,0:0.#####}", num_users, num_items, train.Count, sparsity));

			// test data stats
			if (test != null)
			{
				num_users = test.AllUsers.Count;
				num_items = test.AllItems.Count;
				matrix_size = (long) num_users * num_items;
				empty_size  = (long) matrix_size - test.Count;
				sparsity = (double) 100L * empty_size / matrix_size;
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "test data:     {0} users, {1} items, {2} ratings, sparsity {3,0:0.#####}", num_users, num_items, test.Count, sparsity));
			}

			// count and display the overlap between train and test
			if (display_overlap && test != null)
			{
				int num_new_users = 0;
				int num_new_items = 0;
				TimeSpan seconds = Utils.MeasureTime(delegate() {
							num_new_users = test.AllUsers.Except(train.AllUsers).Count();
							num_new_items = test.AllItems.Except(train.AllItems).Count();
				});
				Console.WriteLine("{0} new users, {1} new items ({2} seconds)", num_new_users, num_new_items, seconds);
			}

			// attribute stats
			if (recommender != null)
			{
				if (recommender is IUserAttributeAwareRecommender)
					Console.WriteLine("{0} user attributes", ((IUserAttributeAwareRecommender) recommender).NumUserAttributes);
				if (recommender is IItemAttributeAwareRecommender)
					Console.WriteLine("{0} item attributes", ((IItemAttributeAwareRecommender) recommender).NumItemAttributes);
			}
		}

		/// <summary>Display data statistics for item recommendation datasets</summary>
		/// <param name="training_data">the training dataset</param>
		/// <param name="test_data">the test dataset</param>
		/// <param name="recommender">the recommender that will be used</param>
		public static void DisplayDataStats(IPosOnlyFeedback training_data, IPosOnlyFeedback test_data, IItemRecommender recommender)
		{
			// training data stats
			int num_users = training_data.AllUsers.Count;
			int num_items = training_data.AllItems.Count;
			long matrix_size = (long) num_users * num_items;
			long empty_size  = (long) matrix_size - training_data.Count;
			double sparsity = (double) 100L * empty_size / matrix_size;
			Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "training data: {0} users, {1} items, {2} events, sparsity {3,0:0.#####}", num_users, num_items, training_data.Count, sparsity));

			// test data stats
			if (test_data != null)
			{
				num_users = test_data.AllUsers.Count;
				num_items = test_data.AllItems.Count;
				matrix_size = (long) num_users * num_items;
				empty_size  = (long) matrix_size - test_data.Count;
				sparsity = (double) 100L * empty_size / matrix_size;
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "test data:     {0} users, {1} items, {2} events, sparsity {3,0:0.#####}", num_users, num_items, test_data.Count, sparsity));
			}

			// attribute stats
			if (recommender is IUserAttributeAwareRecommender)
				Console.WriteLine("{0} user attributes for {1} users",
				                  ((IUserAttributeAwareRecommender)recommender).NumUserAttributes,
				                  ((IUserAttributeAwareRecommender)recommender).UserAttributes.NumberOfRows);
			if (recommender is IItemAttributeAwareRecommender)
				Console.WriteLine("{0} item attributes for {1} items",
				                  ((IItemAttributeAwareRecommender)recommender).NumItemAttributes,
				                  ((IItemAttributeAwareRecommender)recommender).ItemAttributes.NumberOfRows);
		}
	}
}
