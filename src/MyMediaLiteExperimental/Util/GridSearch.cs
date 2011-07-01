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
using System.Globalization;
using System.Linq;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Util
{
	/// <summary>Grid search for finding suitable hyperparameters</summary>
	public static class GridSearch
	{
		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure (1D)</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>
		/// <param name="hyperparameter_name">the name of the hyperparameter to optimize</param>
		/// <param name="hyperparameter_values">the values of the hyperparameter to try out</param>
		/// <param name="recommender">the recommender</param>
		/// <param name="split">the dataset split to use</param>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public static double FindMinimum(string evaluation_measure,
		                                 string hyperparameter_name,
		                                 double[] hyperparameter_values,
		                                 RatingPredictor recommender,
		                                 ISplit<IRatings> split)
		{
			double min_result = double.MaxValue;
			int min_i = -1;

			for (int i = 0; i < hyperparameter_values.Length; i++)
			{
				Recommender.SetProperty(recommender, hyperparameter_name, hyperparameter_values[i].ToString(CultureInfo.InvariantCulture));
				double result = Eval.Ratings.EvaluateOnSplit(recommender, split)[evaluation_measure];

				if (result < min_result)
				{
					min_i = i;
					min_result = result;
				}
			}

			Recommender.SetProperty(recommender, hyperparameter_name, hyperparameter_values[min_i].ToString(CultureInfo.InvariantCulture));

			return min_result;
		}

		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure (2D)</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>
		/// <param name="hp_name1">the name of the first hyperparameter to optimize</param>
		/// <param name="hp_values1">the values of the first hyperparameter to try out</param>
		/// <param name="hp_name2">the name of the second hyperparameter to optimize</param>
		/// <param name="hp_values2">the values of the second hyperparameter to try out</param>
		/// <param name="recommender">the recommender</param>
		/// <param name="split">the dataset split to use</param>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public static double FindMinimum(string evaluation_measure,
		                                 string hp_name1, string hp_name2,
		                                 double[] hp_values1, double[] hp_values2,
		                                 RatingPredictor recommender,
		                                 ISplit<IRatings> split)
		{
			double min_result = double.MaxValue;
			int min_i = -1;
			int min_j = -1;

			for (int i = 0; i < hp_values1.Length; i++)
				for (int j = 0; j < hp_values2.Length; j++)
				{
					Recommender.SetProperty(recommender, hp_name1, hp_values1[i].ToString(CultureInfo.InvariantCulture));
					Recommender.SetProperty(recommender, hp_name2, hp_values2[j].ToString(CultureInfo.InvariantCulture));

					Console.Error.WriteLine("reg_u={0} reg_i={1}", hp_values1[i].ToString(CultureInfo.InvariantCulture), hp_values2[j].ToString(CultureInfo.InvariantCulture)); // TODO this is not generic
					double result = Eval.Ratings.EvaluateOnSplit(recommender, split)[evaluation_measure];
					if (result < min_result)
					{
						min_i = i;
						min_j = j;
						min_result = result;
					}
				}

			// set to best hyperparameter values
			Recommender.SetProperty(recommender, hp_name1, hp_values1[min_i].ToString(CultureInfo.InvariantCulture));
			Recommender.SetProperty(recommender, hp_name2, hp_values2[min_j].ToString(CultureInfo.InvariantCulture));

			return min_result;
		}

		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure (2D)</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>
		/// <param name="hp_name1">the name of the first hyperparameter to optimize</param>
		/// <param name="hp_values1">the logarithm values of the first hyperparameter to try out</param>
		/// <param name="hp_name2">the name of the second hyperparameter to optimize</param>
		/// <param name="hp_values2">the logarithm values of the second hyperparameter to try out</param>
		/// <param name="basis">the basis to use for the logarithms</param>
		/// <param name="recommender">the recommender</param>
		/// <param name="split">the dataset split to use</param>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public static double FindMinimumExponential(string evaluation_measure,
		                                 		    string hp_name1,
		                                            string hp_name2,
		                                 		    double[] hp_values1,
		                                            double[] hp_values2,
		                                            double basis,
		                                 		    RatingPrediction.RatingPredictor recommender,
		                                 		    ISplit<IRatings> split)
		{
			var new_hp_values1 = new double[hp_values1.Length];
			var new_hp_values2 = new double[hp_values2.Length];

			for (int i = 0; i < hp_values1.Length; i++)
				new_hp_values1[i] = Math.Pow(basis, hp_values1[i]);
			for (int i = 0; i < hp_values2.Length; i++)
				new_hp_values2[i] = Math.Pow(basis, hp_values2[i]);

			return FindMinimum(evaluation_measure, hp_name1, hp_name2, new_hp_values1, new_hp_values2, recommender, split);
		}

		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure (1D)</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>
		/// <param name="hp_name">the name of the hyperparameter to optimize</param>
		/// <param name="hp_values">the logarithms of the values of the hyperparameter to try out</param>
		/// <param name="basis">the basis to use for the logarithms</param>
		/// <param name="recommender">the recommender</param>
		/// <param name="split">the dataset split to use</param>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public static double FindMinimumExponential(string evaluation_measure,
		                                 		    string hp_name,
		                                 		    double[] hp_values,
		                                            double basis,
		                                 		    RatingPrediction.RatingPredictor recommender,
		                                 		    ISplit<IRatings> split)
		{
			var new_hp_values = new double[hp_values.Length];

			for (int i = 0; i < hp_values.Length; i++)
				new_hp_values[i] = Math.Pow(basis, hp_values[i]);

			return FindMinimum(evaluation_measure, hp_name, new_hp_values, recommender, split);
		}

		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure using k-fold cross-validation</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>
		/// <param name="hyperparameter_name">the name of the hyperparameter to optimize</param>
		/// <param name="hyperparameter_values">the values of the hyperparameter to try out</param>
		/// <param name="recommender">the recommender</param>
		/// <param name="k">the number of folds to be used for cross-validation</param>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public static double FindMinimum(string evaluation_measure,
		                                 string hyperparameter_name,
		                                 double[] hyperparameter_values,
		                                 RatingPrediction.RatingPredictor recommender,
		                                 int k)
		{
			var data = recommender.Ratings;
			var split = new RatingCrossValidationSplit(data, k);
			double result = FindMinimum(evaluation_measure, hyperparameter_name, hyperparameter_values, recommender, split);
			recommender.Ratings = data;
			return result;
		}
	}
}

