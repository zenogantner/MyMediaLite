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
		static MatrixFactorization        mf = new MatrixFactorization();
		static MatrixFactorization       bmf = new BiasedMatrixFactorization();
		static MatrixFactorization social_mf = new SocialMF();		
		static UserKNNCosine    uknn_c = new UserKNNCosine();
		static UserKNNPearson   uknn_p = new UserKNNPearson();
		static ItemKNNCosine    iknn_c = new ItemKNNCosine();
		static ItemKNNPearson   iknn_p = new ItemKNNPearson();
		static ItemAttributeKNN  iaknn = new ItemAttributeKNN();
		static UserItemBaseline    uib = new UserItemBaseline();
		static GlobalAverage        ga = new GlobalAverage();
		static UserAverage          ua = new UserAverage();
		static ItemAverage          ia = new ItemAverage();

		static void Usage(string message)
		{
			Console.WriteLine(message);
			Console.WriteLine();
			Usage(-1);
		}

		static void Usage(int exit_code)
		{
			Console.WriteLine("MyMediaLite rating prediction; usage:");
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

			// other arguments
			string data_dir             = parameters.GetRemoveString( "data_dir");
			string user_attributes_file = parameters.GetRemoveString( "user_attributes");
			string item_attributes_file = parameters.GetRemoveString( "item_attributes");
			string save_model_file      = parameters.GetRemoveString( "save_model");
			string load_model_file      = parameters.GetRemoveString( "load_model");
			int random_seed             = parameters.GetRemoveInt32(  "random_seed",  -1);
			bool no_eval                = parameters.GetRemoveBool(   "no_eval",      false);
			string predict_ratings_file = parameters.GetRemoveString( "predict_ratings_file");

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
				case "SocialMF":
					recommender = InitMatrixFactorization(parameters, social_mf); // TODO setup social regularization
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
					Usage(string.Format("Unknown method: '{0}'", method));
					break;
			}

			recommender.MinRatingValue = min_rating;
			recommender.MaxRatingValue = max_rating;
			Console.Error.WriteLine("ratings range: [{0}, {1}]", recommender.MinRatingValue, recommender.MaxRatingValue);

			// check command-line parameters
			if (parameters.CheckForLeftovers())
				Usage(-1);
			if (training_file.Equals("-") && testfile.Equals("-"))
				Usage("Either training or test data, not both, can be read from STDIN.");

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
					Usage("Recommender expects user_attributes=FILE.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
					((UserAttributeAwareRecommender)recommender).UserAttributes    = attr_data.First;
					((UserAttributeAwareRecommender)recommender).NumUserAttributes = attr_data.Second;
					Console.WriteLine("{0} user attributes", attr_data.Second);
				}

			// item attributes
			if (recommender is ItemAttributeAwareRecommender)
				if (item_attributes_file.Equals(string.Empty) )
				{
					Usage("Recommender expects item_attributes=FILE.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
					((ItemAttributeAwareRecommender)recommender).ItemAttributes    = attr_data.First;
					((ItemAttributeAwareRecommender)recommender).NumItemAttributes = attr_data.Second;
					Console.WriteLine("{0} item attributes", attr_data.Second);
				}
			
			// read test data
			RatingData test_data = RatingPredictionData.Read(Path.Combine(data_dir, testfile), min_rating, max_rating, user_mapping, item_mapping);

			// TODO DisplayStats
			
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

				DisplayResults(RatingEval.EvaluateRated(recommender, test_data));
				Console.WriteLine(" " + iterative_recommender.NumIter);

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
							DisplayResults(RatingEval.EvaluateRated(recommender, test_data));
							Console.WriteLine(" " + i);
						});
						eval_time_stats.Add(t.TotalSeconds);

						EngineStorage.SaveModel(recommender, data_dir, save_model_file, i);
					}
				} // for
				Console.Out.Flush();

				if (training_time_stats.Count > 0)
					Console.Error.WriteLine(
						"iteration_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
			            training_time_stats.Min(), training_time_stats.Max(), training_time_stats.Average()
					);
				if (eval_time_stats.Count > 0)
					Console.Error.WriteLine(
						"eval_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
			            eval_time_stats.Min(), eval_time_stats.Max(), eval_time_stats.Average()
					);
				if (compute_fit)
					if (fit_time_stats.Count > 0)
						Console.Error.WriteLine(
							"fit_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
			            	fit_time_stats.Min(), fit_time_stats.Max(), fit_time_stats.Average()
						);
				EngineStorage.SaveModel(recommender, data_dir, save_model_file);
				Console.Error.Flush();
			}
			else
			{
				TimeSpan seconds;

				if (load_model_file.Equals(string.Empty))
				{
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
							DisplayResults(RatingEval.EvaluateRated(recommender, test_data));
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
			mf.NumFeatures    = parameters.GetRemoveInt32( "num_features",   mf.NumFeatures);
   			mf.InitMean       = parameters.GetRemoveDouble("init_f_mean",    mf.InitMean);
   			mf.InitStdev      = parameters.GetRemoveDouble("init_f_stdev",   mf.InitStdev);
			mf.Regularization = parameters.GetRemoveDouble("reg",            mf.Regularization);
			mf.Regularization = parameters.GetRemoveDouble("regularization", mf.Regularization);
			mf.LearnRate      = parameters.GetRemoveDouble("lr",             mf.LearnRate);
			mf.LearnRate      = parameters.GetRemoveDouble("learn_rate",     mf.LearnRate);
			return mf;
		}

		static Memory InitKNN(CommandLineParameters parameters, KNN knn)
		{
			knn.k         = parameters.GetRemoveUInt32("k",         knn.k);  // TODO handle "inf"
			knn.shrinkage = parameters.GetRemoveFloat( "shrinkage", knn.shrinkage);
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

		static void DisplayResults(Dictionary<string, double> result) {
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';			
			
			Console.Write(string.Format(ni, "RMSE {0,0:0.#####} MAE {1,0:0.#####}", result["RMSE"], result["MAE"]));
		}
	}
}