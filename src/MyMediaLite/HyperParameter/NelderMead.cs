// Copyright (C) 2010, 2011, 2012, 2017 Zeno Gantner
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
using MathNet.Numerics.LinearAlgebra.Double;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.RatingPrediction;

namespace MyMediaLite.HyperParameter
{
	/// <summary>Nelder-Mead algorithm for finding suitable hyperparameters</summary>
	public class NelderMead
	{
		// TODO make configurable
		static double alpha = 1.0;
		static double gamma = 2.0;
		static double rho = 0.5;
		static double sigma = 0.5;
		static double num_it = 50;
		static double split_ratio = 0.2;

		private string evaluation_measure;
		private RatingPredictor recommender;
		private ISplit<IRatings> split;
		private string[] hp_names;
		private IList<DenseVector> initial_hp_values;

		/// <summary>
		///
		/// </summary>
		/// <param name="evaluation_measure">the name of the evaluation measure</param>/// 
		/// <param name="recommender">the recommender</param>
		public NelderMead(string evaluation_measure, RatingPredictor recommender)
		{
			this.evaluation_measure = evaluation_measure;
			this.recommender = recommender;
			Init();
		}

		private static void EnsureNonNegativity(DenseVector vector)
		{
			for (int i = 0; i < vector.Count; i++)
				if (vector[i] < 0)
					vector[i] = 0;
		}

		/// <summary>
		/// Creates a config string out of a list of parameter names and values
		/// </summary>
		/// <returns>The config string.</returns>
		/// <param name="vector">hyperparameter vector</param>
		private string CreateConfigString(DenseVector vector)
		{
			string hp_string = string.Empty;
			for (int i = 0; i < hp_names.Length; i++)
				hp_string += string.Format(CultureInfo.InvariantCulture, " {0}={1}", hp_names[i], vector[i]);
			return hp_string;
		}

		private double Evaluate(string hp_string)
		{
			recommender.Configure(hp_string);

			double result = recommender.DoCrossValidation(split)[evaluation_measure];
			Console.Error.WriteLine("Nelder-Mead: {0}: {1}", hp_string, result.ToString(CultureInfo.InvariantCulture));
			return result;
		}

		static DenseVector ComputeCenter(Dictionary<string, DenseVector> vectors)
		{
			if (vectors.Count == 0)
				throw new ArgumentException("need at least one vector to build center");

			var center = new DenseVector(vectors.Values.First().Count);

			foreach (var vector in vectors.Values)
				center += vector;

			center /= vectors.Count;

			return center;
		}

		private static IList<DenseVector> CreateInitialValues(double[][] values)
		{
			var result = new DenseVector[values.Length];
			for (int i = 0; i < values.Length; i++)
				result[i] = new DenseVector(values[i]);
			return result;
		}

		private void Init()
		{
			this.split = new RatingsSimpleSplit(recommender.Ratings, split_ratio);
			//this.split = new RatingCrossValidationSplit(recommender.Ratings, 5);

			// TODO manage this via reflection?
			if (recommender is UserItemBaseline) {
				this.hp_names = new string[] { "reg_u", "reg_i" };
				this.initial_hp_values = CreateInitialValues(
					new double[][] {
						new double[] { 25, 10 },
						new double[] { 10, 25 },
						new double[] { 2, 5 },
						new double[] { 5, 2 },
						new double[] { 1, 4 },
						new double[] { 4, 1 },
						new double[] { 3, 3 },
					}
				);
			}
			else if (recommender is BiasedMatrixFactorization)
			{
				this.hp_names = new string[] { "regularization", "bias_reg" };
				this.initial_hp_values = CreateInitialValues(
					// TODO reg_u and reg_i (in a second step?)
					new double[][]
					{
						new double[] { 0.1,     0 },
						new double[] { 0.01,    0 },
						new double[] { 0.0001,  0 },
						new double[] { 0.00001, 0 },
						new double[] { 0.1,     0.0001 },
						new double[] { 0.01,    0.0001 },
						new double[] { 0.0001,  0.0001 },
						new double[] { 0.00001, 0.0001 },
					}
				);
			}
			else if (recommender is MatrixFactorization)
			{
				this.hp_names = new string[] { "regularization" };
				// TODO normal interval search could be more efficient
				this.initial_hp_values = CreateInitialValues(
					new double[][]
					{
						new double[] { 0.1 },
						new double[] { 0.01 },
						new double[] { 0.0001 },
						new double[] { 0.00001 },
					}
				);
			}
			// TODO kNN-based methods
			else
			{
				throw new Exception("not prepared for type " + recommender.GetType().ToString());
			}
		}
			
		/// <summary>Find the the parameters resulting in the minimal results for a given evaluation measure</summary>
		/// <remarks>The recommender will be set to the best parameter value after calling this method.</remarks>
		/// <returns>the best (lowest) average value for the hyperparameter</returns>
		public double FindMinimum()
		{
			var results    = new Dictionary<string, double>();
			var hp_vectors = new Dictionary<string, DenseVector>();

			Console.WriteLine();
			// initialize
			foreach (var vector in initial_hp_values)
			{
				string hp_string = CreateConfigString(vector);
				results[hp_string] = Evaluate(hp_string);
				hp_vectors[hp_string] = vector;
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

				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "Nelder-Mead: iteration {0} ({1})", i, results[min_key]));

				var worst_vector = hp_vectors[max_key];
				var worst_result = results[max_key];
				hp_vectors.Remove(max_key);
				results.Remove(max_key);

				// compute center
				DenseVector center = ComputeCenter(hp_vectors);

				// reflection
				DenseVector reflection = center + alpha * (center - worst_vector);
				EnsureNonNegativity(reflection);
				var ref_string = CreateConfigString(reflection);
				double ref_result = Evaluate(ref_string);
				if (results[min_key] <= ref_result && ref_result < results.Values.Max())
				{
					results[ref_string]    = ref_result;
					hp_vectors[ref_string] = reflection;
					continue;
				}

				// expansion
				if (ref_result < results[min_key])
				{
					var expansion = center + gamma * (center - worst_vector);
					EnsureNonNegativity(expansion);
					string exp_string = CreateConfigString(expansion);
					double exp_result = Evaluate(exp_string);
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
				var contraction = worst_vector + rho * (center - worst_vector);
				EnsureNonNegativity(contraction);
				string con_string = CreateConfigString(contraction);
				double con_result = Evaluate(con_string);
				if (con_result < worst_result)
				{
					results[con_string]    = con_result;
					hp_vectors[con_string] = contraction;
					continue;
				}

				// reduction
				var best_vector = hp_vectors[min_key];
				var best_result = results[min_key];
				hp_vectors.Remove(min_key);
				results.Remove(min_key);
				foreach (var key in new List<string>(results.Keys))
				{
					var reduction = hp_vectors[key] + sigma * (hp_vectors[key] - best_vector);
					EnsureNonNegativity(reduction);
					string red_string = CreateConfigString(reduction);
					double red_result = Evaluate(red_string);

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
			recommender.Configure(keys.First());

			return results[keys.First()];
		}
	}
}
