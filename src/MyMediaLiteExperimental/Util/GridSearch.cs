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

namespace MyMediaLite.Util
{
	public class GridSearch
	{
		// TODO implement first for rating prediction, then for item prediction
		//      generalize also to FindMaximum

		// TODO use delegates or boolean flag to be able to use fit on the test data as a criterion

		public static double FindMinimum(string evaluation_measure,
		                                 string hyperparameter_name,
		                                 double[] hyperparameter_values,
		                                 RatingPrediction.Memory engine,
		                                 ISplit<RatingData> split)
		{
			var ni = new NumberFormatInfo();

			var eval_results = new double[hyperparameter_values.Length];
			for (int i = 0; i < hyperparameter_values.Length; i++)
			{
				Engine.SetProperty(engine, hyperparameter_name, hyperparameter_values[i].ToString(ni));
				eval_results[i] = RatingEval.EvaluateOnSplit(engine, split)[evaluation_measure];
			}

			return eval_results.Min();
		}

		public static double FindMinimumExponential(string evaluation_measure,
		                                 		    string hyperparameter_name,
		                                 		    double[] hyperparameter_values,
		                                            double basis,
		                                 		    RatingPrediction.Memory engine,
		                                 		    ISplit<RatingData> split)
		{
			for (int i = 0; i < hyperparameter_values.Length; i++)
				hyperparameter_values[i] = Math.Pow(basis, hyperparameter_values[i]);

			return FindMinimum(evaluation_measure, hyperparameter_name, hyperparameter_values, engine, split);
		}

		public static double FindMinimum(string evaluation_measure,
		                                 string hyperparameter_name,
		                                 double[] hyperparameter_values,
		                                 RatingPrediction.Memory engine,
		                                 int k)
		{
			RatingData data = engine.Ratings;
			var split = new RatingCrossValidationSplit(data, k);
			double result = FindMinimum(evaluation_measure, hyperparameter_name, hyperparameter_values, engine, split);
			engine.Ratings = data;
			return result;
		}
	}
}

