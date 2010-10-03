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
using System.IO;
using System.Linq;
using System.Text;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.rating_predictor;
using MyMediaLite.item_recommender;
using MyMediaLite.util;


namespace MyMediaLite.eval
{
    /// <summary>Evaluation class</summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public static class RatingEval
    {
        /// <summary>
        /// Evaluates a rating predictor for RMSE and MAE.
        /// </summary>
        /// <param name="engine">Rating prediction engine</param>
        /// <param name="ratings">Test cases</param>
        /// <returns>a Dictionary<string, double> containing the evaluation results</returns>
        static public Dictionary<string, double> EvaluateRated(RatingPredictor engine, RatingData ratings)
		{
            double rmse = 0;
            double mae = 0;
            int cnt = 0;

            foreach (RatingEvent r in ratings)
            {
                double error = (engine.Predict(r.user_id, r.item_id) - r.rating);

				rmse += error * error;
                mae  += Math.Abs(error);
                cnt++;
            }
            mae  = mae / cnt;
            rmse = Math.Sqrt(rmse / cnt);

			if (Double.IsNaN(rmse))
				Console.WriteLine("RMSE is NaN!");
			if (Double.IsNaN(mae))
				Console.WriteLine("MAE is NaN!");

			Dictionary<string, double> result = new Dictionary<string, double>();
			result.Add("RMSE", rmse);
			result.Add("MAE", mae);
			return result;
        }
	}
}
