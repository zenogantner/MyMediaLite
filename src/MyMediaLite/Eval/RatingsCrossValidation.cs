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
		/// <param name="num_folds">the number of folds</param>
		/// <param name="compute_fit">if set to true measure fit on the training data as well</param>
		/// <param name="show_fold_results">if set to true to print per-fold results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public RatingPredictionEvaluationResults DoCrossValidation(
			this RatingPredictor recommender,
			uint num_folds = 5,
			bool compute_fit = false,
			bool show_fold_results = false)
		{
			var split = new RatingCrossValidationSplit(recommender.Ratings, num_folds);
			return recommender.DoCrossValidation(split, compute_fit, show_fold_results);
		}

		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="split">a rating dataset split</param>
		/// <param name="compute_fit">if set to true measure fit on the training data as well</param>
		/// <param name="show_fold_results">set to true to print per-fold results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public RatingPredictionEvaluationResults DoCrossValidation(
			this RatingPredictor recommender,
			ISplit<IRatings> split,
			bool compute_fit = false,
			bool show_fold_results = false)
		{
			var fold_results = new RatingPredictionEvaluationResults[split.NumberOfFolds];

			Parallel.For(0, (int) split.NumberOfFolds, i =>
			{
				try
				{
					var split_recommender = (RatingPredictor) recommender.Clone(); // to avoid changes in recommender
					split_recommender.Ratings = split.Train[i];
					if (recommender is ITransductiveRatingPredictor)
						((ITransductiveRatingPredictor) split_recommender).AdditionalFeedback = split.Test[i];
					split_recommender.Train();
					fold_results[i] = Ratings.Evaluate(split_recommender, split.Test[i]);
					if (compute_fit)
						fold_results[i]["fit"] = (float) split_recommender.ComputeFit();

					if (show_fold_results)
						Console.Error.WriteLine("fold {0} {1}", i, fold_results[i]);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw;
				}
			});

			return new RatingPredictionEvaluationResults(fold_results);
		}

		/// <summary>Evaluate an iterative recommender on the folds of a dataset split, display results on STDOUT</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="num_folds">the number of folds</param>
		/// <param name="max_iter">the maximum number of iterations</param>
		/// <param name="find_iter">the report interval</param>
		/// <param name="show_fold_results">if set to true to print per-fold results to STDERR</param>
		static public void DoIterativeCrossValidation(
			this RatingPredictor recommender,
			uint num_folds,
			uint max_iter,
			uint find_iter = 1,
			bool show_fold_results = false)
		{
			var split = new RatingCrossValidationSplit(recommender.Ratings, num_folds);
			recommender.DoIterativeCrossValidation(split, max_iter, find_iter, show_fold_results);
		}

		/// <summary>Evaluate an iterative recommender on the folds of a dataset split, display results on STDOUT</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="split">a rating dataset split</param>
		/// <param name="max_iter">the maximum number of iterations</param>
		/// <param name="find_iter">the report interval</param>
		/// <param name="show_fold_results">if set to true to print per-fold results to STDERR</param>
		static public void DoIterativeCrossValidation(
			this RatingPredictor recommender,
			ISplit<IRatings> split,
			uint max_iter,
			uint find_iter = 1,
			bool show_fold_results = false)
		{
			if (!(recommender is IIterativeModel))
				throw new ArgumentException("recommender must be of type IIterativeModel");

			var split_recommenders     = new RatingPredictor[split.NumberOfFolds];
			var iterative_recommenders = new IIterativeModel[split.NumberOfFolds];
			var fold_results = new RatingPredictionEvaluationResults[split.NumberOfFolds];
			
			// initial training and evaluation
			Parallel.For(0, (int) split.NumberOfFolds, i =>
			{
				try
				{
					split_recommenders[i] = (RatingPredictor) recommender.Clone(); // to avoid changes in recommender
					split_recommenders[i].Ratings = split.Train[i];
					if (recommender is ITransductiveRatingPredictor)
						((ITransductiveRatingPredictor) split_recommenders[i]).AdditionalFeedback = split.Test[i];
					split_recommenders[i].Train();
					iterative_recommenders[i] = (IIterativeModel) split_recommenders[i];
					fold_results[i] = Ratings.Evaluate(split_recommenders[i], split.Test[i]);
					
					if (show_fold_results)
						Console.Error.WriteLine("fold {0} {1} iteration {2}", i, fold_results[i], iterative_recommenders[i].NumIter);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
					throw;
				}
			});
			Console.WriteLine("{0} iteration {1}", new RatingPredictionEvaluationResults(fold_results), iterative_recommenders[0].NumIter);

			// iterative training and evaluation
			for (int it = (int) iterative_recommenders[0].NumIter + 1; it <= max_iter; it++)
			{
				Parallel.For(0, (int) split.NumberOfFolds, i =>
				{
					try
					{
						iterative_recommenders[i].Iterate();

						if (it % find_iter == 0)
						{
							fold_results[i] = Ratings.Evaluate(split_recommenders[i], split.Test[i]);
							if (show_fold_results)
								Console.Error.WriteLine("fold {0} {1} iteration {2}", i, fold_results[i], it);
						}
					}
					catch (Exception e)
					{
						Console.Error.WriteLine("===> ERROR: " + e.Message + e.StackTrace);
						throw;
					}
				});
				Console.WriteLine("{0} iteration {1}", new RatingPredictionEvaluationResults(fold_results), it);
			}
		}
	}
}

