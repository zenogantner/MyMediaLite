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

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation class</summary>
	public static class RatingEval
	{
		/// <summary>the evaluation measures for rating prediction offered by the class</summary>
		static public ICollection<string> RatingPredictionMeasures
		{
			get	{
				string[] measures = { "RMSE", "MAE", "NMAE" };
				return new HashSet<string>(measures);
			}
		}

		/// <summary>Evaluates a rating predictor for RMSE, MAE, and NMAE</summary>
		/// <remarks>
		/// For NMAE, see "Eigentaste: A Constant Time Collaborative Filtering Algorithm" by Goldberg et al.
		/// </remarks>
		/// <param name="engine">Rating prediction engine</param>
		/// <param name="ratings">Test cases</param>
		/// <returns>a Dictionary containing the evaluation results</returns>
		static public Dictionary<string, double> Evaluate(IRatingPredictor engine, Ratings ratings)
		{
			double rmse = 0;
			double mae  = 0;

			for (int index = 0; index < ratings.Count; index++)
			{
				double error = (engine.Predict(ratings.users[index], ratings.items[index]) - ratings[index]);

				rmse += error * error;
				mae  += Math.Abs(error);
			}
			mae  = mae / ratings.Count;
			rmse = Math.Sqrt(rmse / ratings.Count);

			var result = new Dictionary<string, double>();
			result.Add("RMSE", rmse);
			result.Add("MAE",  mae);
			result.Add("NMAE", mae / (engine.MaxRating - engine.MinRating));
			return result;
		}

		/*
		/// <summary>Evaluate on the folds of a dataset split</summary>
		/// <param name="engine">a rating prediction engine</param>
		/// <param name="split">a rating dataset split</param>
		/// <returns>a dictionary containing the average results over the different folds of the split</returns>
		static public Dictionary<string, double> EvaluateOnSplit(RatingPredictor engine, ISplit<Ratings> split)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			var avg_results = new Dictionary<string, double>();
			foreach (var key in RatingPredictionMeasures)
				avg_results[key] = 0;

			for (int i = 0; i < split.NumberOfFolds; i++)
			{
				engine.Ratings = split.Train[i];
				engine.Train(); // TODO measure time
				var fold_results = Evaluate(engine, split.Test[i]);

				foreach (var key in fold_results.Keys)
					avg_results[key] += fold_results[key];
				Console.Error.WriteLine("fold {0}, RMSE {1,0:0.#####}, MAE {2,0:0.#####}", i, fold_results["RMSE"].ToString(ni), fold_results["MAE"].ToString(ni));
			}

			foreach (var key in avg_results.Keys.ToList())
				avg_results[key] /= split.NumberOfFolds;

			return avg_results;
		}
		*/
	}
}