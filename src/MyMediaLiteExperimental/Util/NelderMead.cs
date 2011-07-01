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
using MathNet.Numerics.LinearAlgebra;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.Util
{
	/// <summary>Nealder-Mead algorithm for finding suitable hyperparameters</summary>
	public static class NelderMead
	{
		// TODO avoid zero values e.g. for regularization ...

		// TODO make configurable
		static double alpha = 1.0;
		static double gamma = 2.0;
		static double rho = 0.5;
		static double sigma = 0.5;
		static double num_it = 50;
		static double split_ratio = 0.2;

		static string CreateConfigString(IList<string> hp_names, IList<double> hp_values)
		{
			string hp_string = string.Empty;
			for (int i = 0; i < hp_names.Count; i++)
				hp_string += string.Format(CultureInfo.InvariantCulture, " {0}={1}", hp_names[i], hp_values[i]);

			return hp_string;
		}

		static double Run(RatingPredictor recommender, ISplit<IRatings> split, string hp_string, string evaluation_measure)
		{
			Recommender.Configure(recommender, hp_string);

			double result = RatingEval.EvaluateOnSplit(recommender, split)[evaluation_measure];
			Console.Error.WriteLine("Nelder-Mead: {0}: {1}", hp_string, result.ToString(CultureInfo.InvariantCulture));
			return result;
		}

		static Vector ComputeCenter(Dictionary<string, double> results, Dictionary<string, Vector> hp_values)
		{
			if (hp_values.Count == 0)
				throw new ArgumentException("need at least one vector to build center");

			var center = new Vector(hp_values.Values.First().Length);

			foreach (var key in results.Keys)
				center += hp_values[key];

			center /= hp_values.Count - 1;

			return center;
		}
		
		/// <summary>Find best hyperparameter (according to an error measure) using Nelder-Mead search</summary>
		/// <param name="error_measure">an error measure (lower is better)</param>
		/// <param name="recommender">a rating predictor (will be set to best hyperparameter combination)</param>
		/// <returns>the estimated error of the best hyperparameter combination</returns>
		public static double FindMinimum(string error_measure,
		                                 RatingPredictor recommender)
		{
			var split = new RatingsSimpleSplit(recommender.Ratings, split_ratio);
			//var split = new RatingCrossValidationSplit(recommender.Ratings, 5);

			IList<string> hp_names;
			IList<Vector> initial_hp_values;

			// TODO manage this via reflection?
			if (recommender is UserItemBaseline)
			{
				hp_names = new string[] { "reg_u", "reg_i" };
				initial_hp_values = new Vector[] {
					new Vector(	new double[] { 25, 10 } ),
					new Vector(	new double[] { 10, 25 } ),
					new Vector(	new double[] { 2, 5 } ),
					new Vector(	new double[] { 5, 2 } ),
					new Vector(	new double[] { 1, 4 } ),
					new Vector(	new double[] { 4, 1 } ),
					new Vector(	new double[] { 3, 3 } ),
				};
			}
			else if (recommender is BiasedMatrixFactorization)
			{
				hp_names = new string[] { "regularization", "bias_reg" };
				initial_hp_values = new Vector[] { // TODO reg_u and reg_i (in a second step?)
					new Vector(	new double[] { 0.1,     0 } ),
					new Vector(	new double[] { 0.01,    0 } ),
					new Vector(	new double[] { 0.0001,  0 } ),
					new Vector(	new double[] { 0.00001, 0 } ),
					new Vector(	new double[] { 0.1,     0.0001 } ),
					new Vector(	new double[] { 0.01,    0.0001 } ),
					new Vector(	new double[] { 0.0001,  0.0001 } ),
					new Vector(	new double[] { 0.00001, 0.0001 } ),
				};
			}
			else if (recommender is MatrixFactorization)
			{ // TODO normal interval search could be more efficient
				hp_names = new string[] { "regularization", };
				initial_hp_values = new Vector[] {
					new Vector(	new double[] { 0.1     } ),
					new Vector(	new double[] { 0.01    } ),
					new Vector(	new double[] { 0.0001  } ),
					new Vector(	new double[] { 0.00001 } ),
				};				
			}
			// TODO kNN-based methods
			else
			{
				throw new Exception("not prepared for type " + recommender.GetType().ToString());
			}

			return FindMinimum(error_measure,
			                   hp_names, initial_hp_values, recommender, split);
		}

		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>
		/// <param name="hp_names">the names of the hyperparameters to optimize</param>
		/// <param name="initial_hp_values">the values of the hyperparameters to try out first</param>
		/// <param name="recommender">the recommender</param>
		/// <param name="split">the dataset split to use</param>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public static double FindMinimum(string evaluation_measure,
		                                 IList<string> hp_names,
		                                 IList<Vector> initial_hp_values,
		                                 RatingPredictor recommender, // TODO make more general?
		                                 ISplit<IRatings> split)
		{
			var results    = new Dictionary<string, double>();
			var hp_vectors = new Dictionary<string, Vector>();

			// initialize
			foreach (var hp_values in initial_hp_values)
			{
				string hp_string = CreateConfigString(hp_names, hp_values);
				results[hp_string] = Run(recommender, split, hp_string, evaluation_measure);
				hp_vectors[hp_string] = hp_values;
			}

			List<string> keys;
			for (int i = 0; i < num_it; i++)
			{
				if (results.Count != hp_vectors.Count)
					throw new Exception(string.Format("{0} vs. {1}", results.Count, hp_vectors.Count));

				keys = new List<string>(results.Keys);
				keys.Sort(delegate(string k1, string k2) { return results[k1].CompareTo(results[k2]); });

				var min_key = keys.First();
				var max_key = keys.Last();

				Console.Error.WriteLine("Nelder-Mead: iteration {0} ({1})", i, results[min_key]);

				var worst_vector = hp_vectors[max_key];
				var worst_result = results[max_key];
				hp_vectors.Remove(max_key);
				results.Remove(max_key);

				// compute center
				var center = ComputeCenter(results, hp_vectors);

				// reflection
				//Console.Error.WriteLine("ref");
				var reflection = center + alpha * (center - worst_vector);
				string ref_string = CreateConfigString(hp_names, reflection);
				double ref_result = Run(recommender, split, ref_string, evaluation_measure);
				if (results[min_key] <= ref_result && ref_result < results.Values.Max())
				{
					results[ref_string]    = ref_result;
					hp_vectors[ref_string] = reflection;
					continue;
				}

				// expansion
				if (ref_result < results[min_key])
				{
					//Console.Error.WriteLine("exp");

					var expansion = center + gamma * (center - worst_vector);
					string exp_string = CreateConfigString(hp_names, expansion);
					double exp_result = Run(recommender, split, exp_string, evaluation_measure);
					if (exp_result < ref_result)
					{
						results[exp_string]    = exp_result;
						hp_vectors[exp_string] = expansion;
					}
					else
					{
						results[ref_string]    = ref_result;
						hp_vectors[ref_string] = reflection;
					}
					continue;
				}

				// contraction
				//Console.Error.WriteLine("con");
				var contraction = worst_vector + rho * (center - worst_vector);
				string con_string = CreateConfigString(hp_names, contraction);
				double con_result = Run(recommender, split, con_string, evaluation_measure);
				if (con_result < worst_result)
				{
					results[con_string]    = con_result;
					hp_vectors[con_string] = contraction;
					continue;
				}

				// reduction
				//Console.Error.WriteLine("red");
				var best_vector = hp_vectors[min_key];
				var best_result = results[min_key];
				hp_vectors.Remove(min_key);
				results.Remove(min_key);
				foreach (var key in new List<string>(results.Keys))
				{
					var reduction = hp_vectors[key] + sigma * (hp_vectors[key] - best_vector);
					string red_string = CreateConfigString(hp_names, reduction);
					double red_result = Run(recommender, split, red_string, evaluation_measure);

					// replace by reduced vector
					results.Remove(key);
					hp_vectors.Remove(key);
					results[red_string]    = red_result;
					hp_vectors[red_string] = reduction;
				}
				results[min_key]    = best_result;
				hp_vectors[min_key] = best_vector;
				results[max_key]    = worst_result;
				hp_vectors[max_key] = worst_vector;
			}

			keys = new List<string>(results.Keys);
			keys.Sort(delegate(string k1, string k2) { return results[k1].CompareTo(results[k2]); });

			// set to best hyperparameter values
			Recommender.Configure(recommender, keys.First());

			return results[keys.First()];
		}
	}
}