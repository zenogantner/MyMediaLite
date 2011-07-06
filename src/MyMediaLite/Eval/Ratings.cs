// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System.Globalization;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class for rating prediction</summary>
	public static class Ratings
	{
		/// <summary>the evaluation measures for rating prediction offered by the class</summary>
		static public ICollection<string> Measures
		{
			get	{
				string[] measures = { "RMSE", "MAE", "NMAE" };
				return new HashSet<string>(measures);
			}
		}

		/// <summary>Write rating prediction results to STDOUT</summary>
		/// <param name="result">the output of the Evaluate() method</param>
		static public void DisplayResults(Dictionary<string, double> result)
		{
			Console.Write(string.Format(CultureInfo.InvariantCulture, "RMSE {0,0:0.#####} MAE {1,0:0.#####} NMAE {2,0:0.#####}",
		                                result["RMSE"], result["MAE"], result["NMAE"]));
		}

		/// <summary>Evaluates a rating predictor for RMSE, MAE, and NMAE</summary>
		/// <remarks>
		/// For NMAE, see "Eigentaste: A Constant Time Collaborative Filtering Algorithm" by Goldberg et al.
		/// </remarks>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">Test cases</param>
		/// <returns>a Dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(IRatingPredictor recommender, IRatings ratings)
		{
			double rmse = 0;
			double mae  = 0;

			if (recommender == null)
				throw new ArgumentNullException("recommender");
			if (ratings == null)
				throw new ArgumentNullException("ratings");

			for (int index = 0; index < ratings.Count; index++)
			{
				double error = (recommender.Predict(ratings.Users[index], ratings.Items[index]) - ratings[index]);

				rmse += error * error;
				mae  += Math.Abs(error);
			}
			mae  = mae / ratings.Count;
			rmse = Math.Sqrt(rmse / ratings.Count);

			var result = new Dictionary<string, double>();
			result["RMSE"] = rmse;
			result["MAE"]  = mae;
			result["NMAE"] = mae / (recommender.MaxRating - recommender.MinRating);
			return result;
		}

		/// <summary>Online evaluation for rating prediction</summary>
		/// <remarks>
		/// Every rating that is tested is added to the training set afterwards.
		/// </remarks>
		/// <param name="recommender">rating predictor</param>
		/// <param name="ratings">Test cases</param>
		/// <returns>a Dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> EvaluateOnline(IRatingPredictor recommender, IRatings ratings)
		{
			double rmse = 0;
			double mae  = 0;

			if (recommender == null)
				throw new ArgumentNullException("recommender");
			if (ratings == null)
				throw new ArgumentNullException("ratings");

			// iterate in random order    // TODO also support chronological order
			foreach (int index in ratings.RandomIndex)
			{
				double error = (recommender.Predict(ratings.Users[index], ratings.Items[index]) - ratings[index]);

				rmse += error * error;
				mae  += Math.Abs(error);

				recommender.AddRating(ratings.Users[index], ratings.Items[index], ratings[index]);
			}
			mae  = mae / ratings.Count;
			rmse = Math.Sqrt(rmse / ratings.Count);

			var result = new Dictionary<string, double>();
			result["RMSE"] = rmse;
			result["MAE"]  = mae;
			result["NMAE"] = mae / (recommender.MaxRating - recommender.MinRating);
			return result;
		}

		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="split">a rating dataset split</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> EvaluateOnSplit(RatingPredictor recommender, ISplit<IRatings> split)
		{
			return EvaluateOnSplit(recommender, split, false);
		}

		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="recommender">a rating predictor</param>
		/// <param name="split">a rating dataset split</param>
		/// <param name="show_results">set to true to print results to STDERR</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> EvaluateOnSplit(RatingPredictor recommender, ISplit<IRatings> split, bool show_results)
		{
			var avg_results = new Dictionary<string, double>();
			foreach (var key in Measures)
				avg_results[key] = 0;

			for (int i = 0; i < split.NumberOfFolds; i++)
			{
				var split_recommender = (RatingPredictor) recommender.Clone(); // to avoid changes in recommender
				split_recommender.Ratings = split.Train[i];
				split_recommender.Train();
				var fold_results = Evaluate(split_recommender, split.Test[i]);

				foreach (var key in fold_results.Keys)
					avg_results[key] += fold_results[key];
				if (show_results)
					Console.Error.WriteLine("fold {0}, RMSE {1,0:0.#####}, MAE {2,0:0.#####}", i, fold_results["RMSE"].ToString(CultureInfo.InvariantCulture), fold_results["MAE"].ToString(CultureInfo.InvariantCulture));
			}

			foreach (var key in avg_results.Keys.ToList())
				avg_results[key] /= split.NumberOfFolds;

			return avg_results;
		}
	}
}