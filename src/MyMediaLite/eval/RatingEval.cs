// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.RatingPredictor;


namespace MyMediaLite.Eval
{
    /// <summary>Evaluation class</summary>
    public static class RatingEval
    {
		/// <summary>
		/// the evaluation measures for rating prediction offered by the class
		/// </summary>
		static public ICollection<string> RatingPredictionMeasures
		{
			get
			{
				string[] measures = { "RMSE", "MAE" };
				return new HashSet<string>(measures);
			}
		}


        /// <summary>
        /// Evaluates a rating predictor for RMSE and MAE.
        /// </summary>
        // <remarks>
        // Additionally, 'num_users' and 'num_items' report the number of users and items with ratings in the test set.
        // </remarks>
        /// <param name="engine">Rating prediction engine</param>
        /// <param name="ratings">Test cases</param>
        /// <returns>a Dictionary containing the evaluation results</returns>
        static public Dictionary<string, double> EvaluateRated(IRatingPredictor engine, RatingData ratings)
		{
            double rmse = 0;
            double mae  = 0;

			//HashSet<int> users = new HashSet<int>();
			//HashSet<int> items = new HashSet<int>();

            foreach (RatingEvent r in ratings)
            {
                double error = (engine.Predict(r.user_id, r.item_id) - r.rating);

				rmse += error * error;
                mae  += Math.Abs(error);

				//users.Add(r.user_id);
				//items.Add(r.item_id);
            }
            mae  = mae / ratings.Count;
            rmse = Math.Sqrt(rmse / ratings.Count);

			if (Double.IsNaN(rmse))
				Console.Error.WriteLine("RMSE is NaN!");
			if (Double.IsNaN(mae))
				Console.Error.WriteLine("MAE is NaN!");

			Dictionary<string, double> result = new Dictionary<string, double>();
			result.Add("RMSE", rmse);
			result.Add("MAE", mae);
			//result.Add("num_users", users.Count);
			//result.Add("num_items", items.Count);
			return result;
        }
	}
}
