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
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Eval
{
	/// <summary>Cross-validation for rating prediction</summary>
	public static class RatingsCrossValidation
	{
		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="split">a rating dataset split</param>
		/// <param name="show_results">set to true to print results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> Evaluate(RatingPredictor recommender, ISplit<IRatings> split, bool show_results = false)
		{
			var avg_results = new Dictionary<string, double>();

			Parallel.For(0, (int) split.NumberOfFolds, i =>
			{
				var split_recommender = (RatingPredictor) recommender.Clone(); // to avoid changes in recommender
				split_recommender.Ratings = split.Train[i];
				split_recommender.Train();
				var fold_results = Ratings.Evaluate(split_recommender, split.Test[i]);

				foreach (var key in fold_results.Keys)
					if (avg_results.ContainsKey(key))
						avg_results[key] += fold_results[key];
					else
						avg_results[key] = fold_results[key];

				if (show_results)
					Console.Error.WriteLine("fold {0} {1}", i, Ratings.FormatResults(fold_results));
			});

			foreach (var key in Ratings.Measures)
				avg_results[key] /= split.NumberOfFolds;

			return avg_results;
		}

		/// <summary>Evaluate an iterative recommender on the folds of a dataset split, display results on STDOUT</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="split">a rating dataset split</param>
		/// <param name="max_iter">the maximum number of iterations</param>
		/// <param name="find_iter">the report interval</param>
		static public void EvaluateIterative(RatingPredictor recommender, ISplit<IRatings> split, int max_iter, int find_iter = 1)
		{
			if (!(recommender is IIterativeModel))
				throw new ArithmeticException("recommender must be of type IIterativeModel");

			var split_recommenders     = new RatingPredictor[split.NumberOfFolds];
			var iterative_recommenders = new IIterativeModel[split.NumberOfFolds];

			// initial training and evaluation
			Parallel.For(0, (int) split.NumberOfFolds, i =>
			{
				split_recommenders[i] = (RatingPredictor) recommender.Clone(); // to avoid changes in recommender
				split_recommenders[i].Ratings = split.Train[i];
				split_recommenders[i].Train();
				iterative_recommenders[i] = (IIterativeModel) split_recommenders[i];
				var fold_results = Ratings.Evaluate(split_recommenders[i], split.Test[i]);
				Console.WriteLine("fold {0} {1} iteration {2}", i, Ratings.FormatResults(fold_results), iterative_recommenders[i].NumIter);
			});

			// iterative training and evaluation
			for (int it = (int) iterative_recommenders[0].NumIter + 1; it <= max_iter; it++)
				Parallel.For(0, (int) split.NumberOfFolds, i =>
				{
					iterative_recommenders[i].Iterate();

					if (it % find_iter == 0)
					{
						var fold_results = Ratings.Evaluate(split_recommenders[i], split.Test[i]);
						Console.WriteLine("fold {0} {1} iteration {2}", i, Ratings.FormatResults(fold_results), it);
					}
				});
		}
	}
}

