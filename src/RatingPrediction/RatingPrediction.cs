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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

/// <summary>Rating prediction program, see Usage() method for more information</summary>
public class RatingPrediction
{
	static NumberFormatInfo ni = new NumberFormatInfo();

	// data sets
	static IRatings training_data;
	static IRatings test_data;

	// recommenders
	static RatingPredictor recommender = null;

	// time statistics
	static List<double> training_time_stats = new List<double>();
	static List<double> fit_time_stats      = new List<double>();
	static List<double> eval_time_stats     = new List<double>();
	static List<double> rmse_eval_stats     = new List<double>();

	// global command line parameters
	static bool compute_fit;
	static bool movielens1m_format;

	static void Usage(string message)
	{
		Console.WriteLine(message);
		Console.WriteLine();
		Usage(-1);
	}

	static void Usage(int exit_code)
	{
		Console.WriteLine(@"
MyMediaLite rating prediction

 usage:  RatingPrediction.exe TRAINING_FILE TEST_FILE METHOD [ARGUMENTS] [OPTIONS]

  use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN

  methods (plus arguments and their defaults):");

			Console.Write("   - ");
			Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.RatingPrediction")));

			Console.WriteLine(@"method ARGUMENTS have the form name=value

  general OPTIONS have the form name=value
   - option_file=FILE           read options from FILE (line format KEY: VALUE)
   - random_seed=N              set random seed to N
   - data_dir=DIR               load all files from DIR
   - user_attributes=FILE       file containing user attribute information
   - item_attributes=FILE       file containing item attribute information
   - user_relation=FILE         file containing user relation information
   - item_relation=FILE         file containing item relation information
   - save_model=FILE            save computed model to FILE
   - load_model=FILE            load model from FILE
   - min_rating=NUM             the smallest valid rating value
   - max_rating=NUM             the greatest valid rating value
   - no_eval=BOOL               do not evaluate
   - predict_ratings_file=FILE  write the rating predictions to  FILE ('-' for STDOUT)
   - cross_validation=K         perform k-fold crossvalidation on the training data
                                 (ignores the test data)
   - ml1m_format=BOOL           read rating data in MovieLens 1M (and 10M) format

  options for finding the right number of iterations (MF methods)
   - find_iter=N                give out statistics every N iterations
   - max_iter=N                 perform at most N iterations
   - epsilon=NUM                abort iterations if RMSE is more than best result plus NUM
   - rmse_cutoff=NUM            abort if RMSE is above NUM
   - mae_cutoff=NUM             abort if MAE is above NUM
   - compute_fit=BOOL           display fit on training data every find_iter iterations");

		Environment.Exit(exit_code);
	}

    static void Main(string[] args)
    {
		Assembly assembly = Assembly.GetExecutingAssembly();
		Assembly.LoadFile(Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar + "MyMediaLiteExperimental.dll");

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);
		ni.NumberDecimalDigits = '.';

		// check number of command line parameters
		if (args.Length < 3)
			Usage("Not enough arguments.");

		// read command line parameters
		string training_file = args[0];
		string testfile      = args[1];
		string method        = args[2];

		CommandLineParameters parameters = null;
		try	{ parameters = new CommandLineParameters(args, 3);	}
		catch (ArgumentException e) { Usage(e.Message);			}

		// arguments for iteration search
		int find_iter      = parameters.GetRemoveInt32(  "find_iter",   0);
		int max_iter       = parameters.GetRemoveInt32(  "max_iter",    500);
		compute_fit        = parameters.GetRemoveBool(   "compute_fit", false);
		double epsilon     = parameters.GetRemoveDouble( "epsilon",     0);
		double rmse_cutoff = parameters.GetRemoveDouble( "rmse_cutoff", double.MaxValue);
		double mae_cutoff  = parameters.GetRemoveDouble( "mae_cutoff",  double.MaxValue);

		// collaborative data characteristics
		double min_rating           = parameters.GetRemoveDouble( "min_rating",  1);
		double max_rating           = parameters.GetRemoveDouble( "max_rating",  5);

		// data arguments
		string data_dir             = parameters.GetRemoveString( "data_dir");
		string user_attributes_file = parameters.GetRemoveString( "user_attributes");
		string item_attributes_file = parameters.GetRemoveString( "item_attributes");
		string user_relation_file   = parameters.GetRemoveString( "user_relation");
		string item_relation_file   = parameters.GetRemoveString( "item_relation");

		// other arguments
		string save_model_file      = parameters.GetRemoveString( "save_model");
		string load_model_file      = parameters.GetRemoveString( "load_model");
		int random_seed             = parameters.GetRemoveInt32(  "random_seed",  -1);
		bool no_eval                = parameters.GetRemoveBool(   "no_eval",      false);
		string predict_ratings_file = parameters.GetRemoveString( "predict_ratings_file");
		int cross_validation        = parameters.GetRemoveInt32(  "cross_validation", 0);
		movielens1m_format          = parameters.GetRemoveBool(   "ml1m_format",  false); // TODO automagically recognize file format

		if (random_seed != -1)
			MyMediaLite.Util.Random.InitInstance(random_seed);

		recommender = Recommender.CreateRatingPredictor(method);
		if (recommender == null)
			Usage(string.Format("Unknown method: '{0}'", method));

		Recommender.Configure(recommender, parameters, Usage);

		if (parameters.CheckForLeftovers())
			Usage(-1);

		// check command-line parameters
		if (training_file.Equals("-") && testfile.Equals("-"))
			Usage("Either training or test data, not both, can be read from STDIN.");

		// ID mapping objects
		var user_mapping = new EntityMapping();
		var item_mapping = new EntityMapping();

		// load all the data
		TimeSpan loading_time = Utils.MeasureTime(delegate() {
			LoadData(data_dir, training_file, testfile, min_rating, max_rating,
			         user_mapping, item_mapping, user_attributes_file, item_attributes_file,
			         user_relation_file, item_relation_file);
		});
		Console.WriteLine(string.Format(ni, "loading_time {0,0:0.##}", loading_time.TotalSeconds));

		// TODO move that into the recommender functionality (set from data)
		recommender.MinRating = min_rating;
		recommender.MaxRating = max_rating;
		Console.Error.WriteLine(string.Format(ni, "ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));

		DisplayDataStats();

		if (find_iter != 0)
		{
			if ( !(recommender is IIterativeModel) )
				Usage("Only iterative recommenders support find_iter.");
			IIterativeModel iterative_recommender = (MatrixFactorization) recommender;
			Console.WriteLine(recommender.ToString() + " ");

			if (load_model_file.Equals(string.Empty))
				recommender.Train();
			else
				Recommender.LoadModel(iterative_recommender, load_model_file);

			if (compute_fit)
				Console.Write(string.Format(ni, "fit {0,0:0.#####} ", iterative_recommender.ComputeFit()));

			DisplayResults(RatingEval.Evaluate(recommender, test_data));
			Console.WriteLine(" " + iterative_recommender.NumIter);

			for (int i = iterative_recommender.NumIter + 1; i <= max_iter; i++)
			{
				TimeSpan time = Utils.MeasureTime(delegate() {
					iterative_recommender.Iterate();
				});
				training_time_stats.Add(time.TotalSeconds);

				if (i % find_iter == 0)
				{
					if (compute_fit)
					{
						double fit = 0;
						time = Utils.MeasureTime(delegate() {
							fit = iterative_recommender.ComputeFit();
						});
						fit_time_stats.Add(time.TotalSeconds);
						Console.Write(string.Format(ni, "fit {0,0:0.#####} ", fit));
					}

					Dictionary<string, double> results = null;
					time = Utils.MeasureTime(delegate() {
						results = RatingEval.Evaluate(recommender, test_data);
						DisplayResults(results);
						rmse_eval_stats.Add(results["RMSE"]);
						Console.WriteLine(" " + i);
					});
					eval_time_stats.Add(time.TotalSeconds);

					Recommender.SaveModel(recommender, save_model_file, i);

					if (epsilon > 0 && results["RMSE"] > rmse_eval_stats.Min() + epsilon)
					{
						Console.Error.WriteLine(string.Format(ni, "{0} >> {1}", results["RMSE"], rmse_eval_stats.Min()));
						Console.Error.WriteLine("Reached convergence on training/validation data after {0} iterations.", i);
						break;
					}
					if (results["RMSE"] > rmse_cutoff || results["MAE"] > mae_cutoff)
					{
							Console.Error.WriteLine("Reached cutoff after {0} iterations.", i);
							break;
					}
				}
			} // for

			DisplayIterationStats();
			Recommender.SaveModel(recommender, save_model_file);
		}
		else
		{
			TimeSpan seconds;

			if (load_model_file.Equals(string.Empty))
			{
				Console.Write(recommender.ToString());
				if (cross_validation > 0)
				{
					Console.WriteLine();
					var split = new RatingCrossValidationSplit(training_data, cross_validation);
					var results = RatingEval.EvaluateOnSplit(recommender, split);
					DisplayResults(results);
					no_eval = true;
					recommender.Ratings = training_data;
				}
				else
				{
					seconds = Utils.MeasureTime( delegate() { recommender.Train(); } );
        			Console.Write(" training_time " + seconds + " ");
					Recommender.SaveModel(recommender, save_model_file);
				}
			}
			else
			{
				Recommender.LoadModel(recommender, load_model_file);
				Console.Write(recommender.ToString() + " ");
			}

			if (!no_eval)
			{
				seconds = Utils.MeasureTime(
			    	delegate()
				    {
						DisplayResults(RatingEval.Evaluate(recommender, test_data));
					}
				);
				Console.Write(" testing_time " + seconds);
			}

			if (!predict_ratings_file.Equals(string.Empty))
			{
				seconds = Utils.MeasureTime(
			    	delegate() {
						Console.WriteLine();
						MyMediaLite.Eval.RatingPrediction.WritePredictions(recommender, test_data, user_mapping, item_mapping, predict_ratings_file);
					}
				);
				Console.Error.Write("predicting_time " + seconds);
			}

			Console.WriteLine();
		}
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}

    static void LoadData(string data_dir,
	              string training_file, string test_file,
	              double min_rating, double max_rating,
	              EntityMapping user_mapping, EntityMapping item_mapping,
	              string user_attributes_file, string item_attributes_file,
	              string user_relation_file, string item_relation_file)
	{
		// TODO check for the existence of files before starting to load all of them

		// read training data

		if (movielens1m_format)
			training_data = MovieLensRatingData.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping);
		else
			training_data = RatingPredictionStatic.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping);
		recommender.Ratings = training_data;

		// user attributes
		if (recommender is IUserAttributeAwareRecommender) // TODO also support the MovieLens format here
		{
			if (user_attributes_file.Equals(string.Empty))
				Usage("Recommender expects user_attributes=FILE.");
			else
				((IUserAttributeAwareRecommender)recommender).UserAttributes = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
		}

		// item attributes
		if (recommender is IItemAttributeAwareRecommender)
		{
			if (item_attributes_file.Equals(string.Empty))
				Usage("Recommender expects item_attributes=FILE.");
			else
				((IItemAttributeAwareRecommender)recommender).ItemAttributes = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
		}

		// user relation
		if (recommender is IUserRelationAwareRecommender)
			if (user_relation_file.Equals(string.Empty))
			{
				Usage("Recommender expects user_relation=FILE.");
			}
			else
			{
				((IUserRelationAwareRecommender)recommender).UserRelation = RelationData.Read(Path.Combine(data_dir, user_relation_file), user_mapping);
				Console.WriteLine("relation over {0} users", ((IUserRelationAwareRecommender)recommender).NumUsers);
			}

		// item relation
		if (recommender is IItemRelationAwareRecommender)
			if (user_relation_file.Equals(string.Empty))
			{
				Usage("Recommender expects item_relation=FILE.");
			}
			else
			{
				((IItemRelationAwareRecommender)recommender).ItemRelation = RelationData.Read(Path.Combine(data_dir, item_relation_file), item_mapping);
				Console.WriteLine("relation over {0} items", ((IItemRelationAwareRecommender)recommender).NumItems);
			}

		// read test data
		if (movielens1m_format) // TODO re-activate
			test_data = MovieLensRatingData.Read(Path.Combine(data_dir, test_file), min_rating, max_rating, user_mapping, item_mapping);
		else
			test_data = RatingPredictionStatic.Read(Path.Combine(data_dir, test_file), min_rating, max_rating, user_mapping, item_mapping);
	}

	static void AbortHandler(object sender, ConsoleCancelEventArgs args)
	{
		DisplayIterationStats();
	}

	// TODO move to a class in the MyMediaLite base library
	static void DisplayResults(Dictionary<string, double> result)
	{
		Console.Write(string.Format(ni, "RMSE {0,0:0.#####} MAE {1,0:0.#####} NMAE {2,0:0.#####}",
		                            result["RMSE"], result["MAE"], result["NMAE"]));
	}

	// TODO move to a class in the MyMediaLite base library
	static void DisplayDataStats()
	{
		// training data stats
		int num_users = training_data.AllUsers.Count;
		int num_items = training_data.AllItems.Count;
		long matrix_size = (long) num_users * num_items;
		long empty_size  = (long) matrix_size - training_data.Count;
		double sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(ni, "training data: {0} users, {1} items, {2} ratings, sparsity {3,0:0.#####}", num_users, num_items, training_data.Count, sparsity));

		// test data stats
		num_users = test_data.AllUsers.Count;
		num_items = test_data.AllItems.Count;
		matrix_size = (long) num_users * num_items;
		empty_size  = (long) matrix_size - test_data.Count;
		sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(ni, "test data:     {0} users, {1} items, {2} ratings, sparsity {3,0:0.#####}", num_users, num_items, test_data.Count, sparsity));

		// attribute stats
		if (recommender is IUserAttributeAwareRecommender)
			Console.WriteLine("{0} user attributes", ((IUserAttributeAwareRecommender)recommender).NumUserAttributes);
		if (recommender is IItemAttributeAwareRecommender)
			Console.WriteLine("{0} item attributes", ((IItemAttributeAwareRecommender)recommender).NumItemAttributes);
	}

	static void DisplayIterationStats()
	{
		if (training_time_stats.Count > 0)
			Console.Error.WriteLine(string.Format(
			    ni,
				"iteration_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
	            training_time_stats.Min(), training_time_stats.Max(), training_time_stats.Average()
			));
		if (eval_time_stats.Count > 0)
			Console.Error.WriteLine(string.Format(
			    ni,
				"eval_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
	            eval_time_stats.Min(), eval_time_stats.Max(), eval_time_stats.Average()
			));
		if (compute_fit && fit_time_stats.Count > 0)
			Console.Error.WriteLine(string.Format(
			    ni,
				"fit_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
            	fit_time_stats.Min(), fit_time_stats.Max(), fit_time_stats.Average()
			));
	}
}
