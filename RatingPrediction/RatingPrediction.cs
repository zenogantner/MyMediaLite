// Copyright (C) 2010 Zeno Gantner
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
using MyMediaLite;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.eval;
using MyMediaLite.io;
using MyMediaLite.rating_predictor;
using MyMediaLite.util;


namespace RatingPrediction
{
	/// <summary>Rating prediction program, see Usage() method for more information</summary>
	public class RatingPrediction
	{
		// recommender engines
		static MatrixFactorization mf  = new MatrixFactorization();
		static MatrixFactorization bmf = new BiasedMatrixFactorization();
		static UserKNNCosine    uknn_c = new UserKNNCosine();
		static UserKNNPearson   uknn_p = new UserKNNPearson();
		static ItemKNNCosine    iknn_c = new ItemKNNCosine();
		static ItemKNNPearson   iknn_p = new ItemKNNPearson();
		static ItemAttributeKNN  iaknn = new ItemAttributeKNN();
		static UserItemBaseline    uib = new UserItemBaseline();
		static GlobalAverage        ga = new GlobalAverage();
		static UserAverage          ua = new UserAverage();
		static ItemAverage          ia = new ItemAverage();

		public static void Usage(string message)
		{
			Console.Error.WriteLine(message);
			Console.Error.WriteLine();
			Usage(-1);
		}

		public static void Usage(int exit_code)
		{
			Console.WriteLine("MyMedia rating prediction; usage:");
			Console.WriteLine(" RatingPrediction.exe TRAINING_FILE TEST_FILE METHOD [ARGUMENTS] [OPTIONS]");
			Console.WriteLine("    - use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN");
			Console.WriteLine("  - methods (plus arguments and their defaults):");
			Console.WriteLine("    - " + mf);
			Console.WriteLine("    - " + bmf);
			Console.WriteLine("    - " + uknn_p);
			Console.WriteLine("    - " + uknn_c);
			Console.WriteLine("    - " + iknn_p);
			Console.WriteLine("    - " + iknn_c);
			Console.WriteLine("    - " + iaknn + " (needs item_attributes)");
			Console.WriteLine("    - " + uib);
			Console.WriteLine("    - " + ga);
			Console.WriteLine("    - " + ua);
			Console.WriteLine("    - " + ia);
			Console.WriteLine("  - method ARGUMENTS have the form name=value");
			Console.WriteLine("  - general OPTIONS have the form name=value");
			Console.WriteLine("    - option_file=FILE           read options from FILE (line format KEY: VALUE)");
			Console.WriteLine("    - random_seed=N              ");
			Console.WriteLine("    - data_dir=DIR               load all files from DIR");
			Console.WriteLine("    - item_attributes=FILE       file containing item attribute information");
			Console.WriteLine("    - save_model=FILE            save computed model to FILE");
			Console.WriteLine("    - load_model=FILE            load model from FILE");
			Console.WriteLine("    - min_rating=NUM             ");
			Console.WriteLine("    - max_rating=NUM             ");
			Console.WriteLine("    - no_eval=BOOL               ");
			Console.WriteLine("    - predict_ratings_file=FILE  write the rating predictions to STDOUT");
			Console.WriteLine("  - options for finding the right number of iterations (MF methods)");
			Console.WriteLine("    - find_iter=STEP");
			Console.WriteLine("    - max_iter=N");
			Console.WriteLine("    - compute_fit=BOOL");
			/*
			Console.WriteLine("  - options for hyperparameter search:");
			Console.WriteLine("    - hyper_split=N          number of folds used in cross-validation");
			Console.WriteLine("    - hyper_criterion=NAME   values {0}", String.Join( ", ", Evaluate.GetRatingPredictionMeasures().ToArray()));
			Console.WriteLine("    - hyper_half_size=N      number of values tried in each iteration/2");
			*/

			Environment.Exit(exit_code);
		}

        public static void Main(string[] args)
        {
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);

			// check number of command line parameters
			if (args.Length < 3)
				Usage("Not enough arguments.");

			// read command line parameters
			string training_file = args[0];
			string testfile  = args[1];
			string method    = args[2];

			CommandLineParameters parameters = null;
			try	{ parameters = new CommandLineParameters(args, 3);	}
			catch (ArgumentException e) { Usage(e.Message);			}

			// arguments for iteration search
			int find_iter               = parameters.GetRemoveInt32(  "find_iter", 0);
			int max_iter                = parameters.GetRemoveInt32(  "max_iter", 500);
			bool compute_fit            = parameters.GetRemoveBool(   "compute_fit", false);

			// collaborative data characteristics
			double min_rating           = parameters.GetRemoveDouble( "min_rating",  1);
			double max_rating           = parameters.GetRemoveDouble( "max_rating",  5);
			//int num_ratings             = parameters.GetRemoveInt32(  "num_ratings", 1);
			//int num_users               = parameters.GetRemoveInt32(  "num_users",   1);
			//int num_items               = parameters.GetRemoveInt32(  "num_items",   1);

			// other arguments
			string data_dir             = parameters.GetRemoveString( "data_dir");
			string user_attributes_file = parameters.GetRemoveString( "user_attributes");
			string item_attributes_file = parameters.GetRemoveString( "item_attributes");
			string save_model_file      = parameters.GetRemoveString( "save_model");
			string load_model_file      = parameters.GetRemoveString( "load_model");
			int random_seed             = parameters.GetRemoveInt32(  "random_seed",  -1);
			bool no_eval                = parameters.GetRemoveBool(   "no_eval",      false);
			string predict_ratings_file = parameters.GetRemoveString( "predict_ratings_file");

			/*
			// hyperparameter search arguments
			int    hyper_split          = parameters.GetRemoveInt32(  "hyper_split", -1);
			string hyper_criterion      = parameters.GetRemoveString( "hyper_criterion", "RMSE");
			       half_size            = parameters.GetRemoveUInt32( "half_size", 2);
			*/

			if (random_seed != -1)
				MyMediaLite.util.Random.InitInstance(random_seed);

			// set correct recommender
			MyMediaLite.rating_predictor.Memory recommender = null;
			switch (method)
			{
				case "matrix-factorization":
					recommender = InitMatrixFactorization(parameters, mf);
					break;
				case "biased-matrix-factorization":
					recommender = InitMatrixFactorization(parameters, bmf);
					break;
				case "user-knn-pearson":
				case "user-kNN-pearson":
					recommender = InitKNN(parameters, uknn_p);
					break;
				case "user-knn-cosine":
				case "user-kNN-cosine":
					recommender = InitKNN(parameters, uknn_c);
					break;
				case "item-knn-pearson":
				case "item-kNN-pearson":
					recommender = InitKNN(parameters, iknn_p);
					break;
				case "item-knn-cosine":
				case "item-kNN-cosine":
					recommender = InitKNN(parameters, iknn_c);
					break;
				case "item-attribute-knn":
				case "item-attribute-kNN":
					recommender = InitKNN(parameters, iaknn);
					break;
				case "user-item-baseline":
					recommender = InitUIB(parameters);
					break;
				case "global-average":
					recommender = ga;
					break;
				case "user-average":
					recommender = ua;
					break;
				case "item-average":
					recommender = ia;
					break;
				default:
					Usage(String.Format("Unknown method: '{0}'", method));
					break;
			}

			recommender.MinRatingValue = min_rating;
			recommender.MaxRatingValue = max_rating;
			Console.Error.WriteLine("ratings range: [{0}, {1}]", recommender.MinRatingValue, recommender.MaxRatingValue);

			// check command-line parameters
			if (parameters.CheckForLeftovers())
				Usage(-1);
			if (training_file.Equals("-") && testfile.Equals("-"))
			{
				Console.Out.WriteLine("Either training OR test data, not both, can be read from STDIN.");
				Usage(-1);
			}

			// ID mapping objects
			EntityMapping user_mapping = new EntityMapping();
			EntityMapping item_mapping = new EntityMapping();

			// read training data
			RatingData training_data = RatingPredictionData.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping);
			recommender.Ratings = training_data;

			// user attributes
			if (recommender is UserAttributeAwareRecommender)
				if (user_attributes_file.Equals(String.Empty))
				{
					Usage("Recommender expects user_attributes.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
					((UserAttributeAwareRecommender)recommender).UserAttributes    = attr_data.First;
					((UserAttributeAwareRecommender)recommender).NumUserAttributes = attr_data.Second;
				}

			// item attributes
			if (recommender is ItemAttributeAwareRecommender)
				if (item_attributes_file.Equals(string.Empty) )
				{
					Usage("Recommender expects item_attributes.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
					((ItemAttributeAwareRecommender)recommender).ItemAttributes    = attr_data.First;
					((ItemAttributeAwareRecommender)recommender).NumItemAttributes = attr_data.Second;
				}

			// read test data
			RatingData test_data = RatingPredictionData.Read(Path.Combine(data_dir, testfile), min_rating, max_rating, user_mapping, item_mapping);

			if (find_iter != 0)
			{
				if ( !(recommender is IterativeModel) )
					Usage("Only iterative recommender engines support find_iter.");
				IterativeModel iterative_recommender = (MatrixFactorization) recommender;
				Console.WriteLine(recommender.ToString() + " ");

				if (load_model_file.Equals(string.Empty))
					recommender.Train();
				else
					EngineStorage.LoadModel(iterative_recommender, data_dir, load_model_file);

				if (compute_fit)
					Console.Write("fit {0,0:0.#####} ", iterative_recommender.ComputeFit());

				var result = RatingEval.EvaluateRated(recommender, test_data);
				Console.WriteLine("RMSE {0,0:0.#####} MAE {1,0:0.#####} {2}", result["RMSE"], result["MAE"], iterative_recommender.NumIter);

				List<double> training_time_stats = new List<double>();
				List<double> fit_time_stats      = new List<double>();
				List<double> eval_time_stats     = new List<double>();

				for (int i = iterative_recommender.NumIter + 1; i <= max_iter; i++)
				{
					TimeSpan t = Utils.MeasureTime(delegate() {
						iterative_recommender.Iterate();
					});
					training_time_stats.Add(t.TotalSeconds);

					if (i % find_iter == 0)
					{
						if (compute_fit)
						{
							double fit = 0;
							t = Utils.MeasureTime(delegate() {
								fit = iterative_recommender.ComputeFit();
							});
							fit_time_stats.Add(t.TotalSeconds);
							Console.Write("fit {0,0:0.#####} ", fit);
						}

						t = Utils.MeasureTime(delegate() {
							result = RatingEval.EvaluateRated(recommender, test_data);
							Console.WriteLine("RMSE {0,0:0.#####} MAE {1,0:0.#####} {2}", result["RMSE"], result["MAE"], i);
						});
						eval_time_stats.Add(t.TotalSeconds);

						EngineStorage.SaveModel(recommender, data_dir, save_model_file, i);
					}
				} // for
				Console.Out.Flush();

				if (training_time_stats.Count > 0)
				{
					Console.Error.WriteLine(
						"iteration_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
			            training_time_stats.Min(), training_time_stats.Max(), training_time_stats.Average()
					);
				}
				if (eval_time_stats.Count > 0)
				{
					Console.Error.WriteLine(
						"eval_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
			            eval_time_stats.Min(), eval_time_stats.Max(), eval_time_stats.Average()
					);
				}
				if (compute_fit)
				{
					if (fit_time_stats.Count > 0)
					{
						Console.Error.WriteLine(
							"fit_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
			            	fit_time_stats.Min(), fit_time_stats.Max(), fit_time_stats.Average()
						);
					}
				}
				EngineStorage.SaveModel(recommender, data_dir, save_model_file);
				Console.Error.Flush();
			}
			else
			{
				TimeSpan seconds;

				if (load_model_file.Equals(string.Empty))
				{
					/*
					if (hyper_split != -1)
						FindHyperparameters(recommender, hyper_split, hyper_criterion);
					*/

					Console.Write(recommender.ToString() + " ");
					seconds = Utils.MeasureTime( delegate() { recommender.Train(); } );
            		Console.Write("training_time " + seconds + " ");
					EngineStorage.SaveModel(recommender, data_dir, save_model_file);
				}
				else
				{
					EngineStorage.LoadModel(recommender, data_dir, load_model_file);
					Console.Write(recommender.ToString() + " ");
				}

				if (!no_eval)
				{
					seconds = Utils.MeasureTime(
				    	delegate()
					    {
							var result = RatingEval.EvaluateRated(recommender, test_data);
							Console.Write("RMSE {0,0:0.#####} MAE {1,0:0.#####}", result["RMSE"], result["MAE"]);
						}
					);
					Console.Write(" testing_time " + seconds);
				}

				if (!predict_ratings_file.Equals(string.Empty))
				{
					seconds = Utils.MeasureTime(
				    	delegate() {
							Console.WriteLine();
							MyMediaLite.eval.RatingPrediction.WritePredictions(recommender, test_data, predict_ratings_file);
						}
					);
					Console.Error.Write("predicting_time " + seconds);
				}

				Console.WriteLine();
			}
		}

		static Memory InitMatrixFactorization(CommandLineParameters parameters, MatrixFactorization mf)
		{
			mf.NumIter        = parameters.GetRemoveInt32( "num_iter",       mf.NumIter);
			mf.num_features   = parameters.GetRemoveInt32( "num_features",   mf.num_features);
   			mf.init_f_mean    = parameters.GetRemoveDouble("init_f_mean",    mf.init_f_mean);
   			mf.init_f_stdev   = parameters.GetRemoveDouble("init_f_stdev",   mf.init_f_stdev);
			mf.regularization = parameters.GetRemoveDouble("reg",            mf.regularization);
			mf.regularization = parameters.GetRemoveDouble("regularization", mf.regularization);
			mf.learn_rate     = parameters.GetRemoveDouble("lr",             mf.learn_rate);
			mf.learn_rate     = parameters.GetRemoveDouble("learn_rate",     mf.learn_rate);
			return mf;
		}

		static Memory InitKNN(CommandLineParameters parameters, KNN knn)
		{
			knn.k         = parameters.GetRemoveUInt32("k",         knn.k);  // TODO handle "inf"
			knn.shrinkage = parameters.GetRemoveDouble("shrinkage", knn.shrinkage);
			knn.reg_i     = parameters.GetRemoveDouble("reg_i",     knn.reg_i);
			knn.reg_u     = parameters.GetRemoveDouble("reg_u",     knn.reg_u);

			return knn;
		}

		static Memory InitUIB(CommandLineParameters parameters)
		{
			uib.reg_i = parameters.GetRemoveDouble("reg_i", uib.reg_i);
			uib.reg_u = parameters.GetRemoveDouble("reg_u", uib.reg_u);

			return uib;
		}

		/*
		static void FindHyperparameters(RatingPredictor recommender, int cross_validation, string criterion)
		{
			TimeSpan seconds = Utils.MeasureTime( delegate() {
				// TODO handle via multiple dispatch
				if (recommender is MatrixFactorization)
				{
					FindGoodLearnRate((MatrixFactorization) recommender);
					FindGoodHyperparameters((MatrixFactorization) recommender, cross_validation, criterion);
				}
				else
				{
					throw new ArgumentException(string.Format("Hyperparameter search not supported for {0}", recommender));
				}
			});
			Console.Write("hyperparameters {0} ", seconds);
		}

        static void FindGoodHyperparameters(MatrixFactorization recommender, int cross_validation, string criterion)
		{
			Console.Error.WriteLine();
			Console.Error.WriteLine("Hyperparameter search ...");

			double step = 1.0;

			double center = -4; // TODO make configurable
			double new_center;

			double upper_limit = 0; // highest (log) hyperparameter tried so far
			double lower_limit = 0; // lowest (log) hyperparameter tried so far

			while (step > 0.125)
			{
				upper_limit = Math.Max(upper_limit, center + half_size * step);
				lower_limit = Math.Min(lower_limit, center - half_size * step);
				new_center = FindGoodHyperparameters(recommender, half_size, center, step,  cross_validation, criterion);
				if (new_center == lower_limit)
				{
					center = new_center - half_size * step;
					Console.Error.WriteLine("Set center below lower limit: {0} < {1}", center, lower_limit);
				}
				else if (new_center == upper_limit)
				{
					center = new_center + half_size * step;
					Console.Error.WriteLine("Set center above upper limit: {0} > {1}", center, upper_limit);
				}
				else
				{
					center = new_center;
					step = step / 2;
					Console.Error.WriteLine("Set center: {0}", center);
				}
			}
			//recommender.num_iter_mapping = num_iter_mapping; // restore
			Console.Error.WriteLine();
		}

		static double FindGoodHyperparameters(MatrixFactorization engine, uint half_size, double center, double step_size, int cross_validation, string criterion)
		{
			IEntityRelationDataProvider backend = engine.EntityRelationDataProvider; // save for later use
			CrossvalidationSplit split = new CrossvalidationSplit((WP2Backend)backend, cross_validation, true);
			// TODO the split should only be created once, not on every call of the function

			double best_log_reg = 0;
			double best_q       = double.MaxValue;

            double[] log_reg = new double[2 * half_size + 1];

            log_reg[half_size] = center;
	        for (int i = 0; i <= half_size; i++) {
      	        log_reg[half_size - i] = center - step_size * i;
                log_reg[half_size + i] = center + step_size * i;
            }

			foreach (double exp in log_reg)
			{
				double reg = Math.Pow(reg_base, exp);
				engine.regularization = reg;
				IList<double> quality = new List<double>();
				for (int j = 0; j < cross_validation; j++)
				{
					engine.EntityRelationDataProvider = split.GetTrainingSet(j);
					engine.Train();

					var results = Evaluate.EvaluateRated(engine, split.GetTestSet(j));
					if (!results.ContainsKey(criterion))
						throw new ArgumentException(string.Format("Unknown criterion {0}, valid criteria are {1}",
						                            	          criterion, String.Join( ", ", Evaluate.GetRatingPredictionMeasures().ToArray() ) ));
					quality.Add(results[criterion]);
					Console.Error.Write(".");
				}

				if (quality.Average() < best_q)
				{
					best_q = quality.Average();
					best_log_reg = exp;
				}

				Console.Error.WriteLine("reg={0}, {1}=({2}, {3}, {4})", reg, criterion, quality.Min(), quality.Average(), quality.Max());
			} //foreach

			engine.regularization = Math.Pow(reg_base, best_log_reg);
			engine.EntityRelationDataProvider = backend; // reset
			Console.Error.WriteLine();
			return best_log_reg;
		}

		static void FindGoodLearnRate(MatrixFactorization engine)
		{
			Console.Error.WriteLine("Finding good learn rate ...");

			double best_fit = double.MaxValue;
			double best_lr  = 0;

			engine.regularization = 0;

			double[] learn_rates = new double[]{ 0.0001, 0.001, 0.005, 0.01, 0.05, 0.1, 0.25};

			foreach (var lr in learn_rates)
			{
				try
				{
					engine.learn_rate = lr;

					engine.Train();
					double fit = engine.ComputeFit();

					if (fit < best_fit)
					{
						best_fit = fit;
						best_lr  = lr;
					}
					Console.Error.WriteLine("lr={0}, fit={1}", lr, fit);
				}
				catch (Exception)
				{
					Console.Error.WriteLine("Caught exception, ignore this learn rate.");
				}
			} //foreach

			engine.learn_rate = best_lr;
			Console.Error.WriteLine("Pick {0}", best_lr);
		}
		*/
		// TODO move hyperparameter search into library
	}
}