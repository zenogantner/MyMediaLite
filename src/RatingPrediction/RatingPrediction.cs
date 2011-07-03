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
using Mono.Options;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

/// <summary>Rating prediction program, see Usage() method for more information</summary>
class RatingPrediction
{
	// data sets
	static IRatings training_data;
	static IRatings test_data;

	// recommenders
	static RatingPredictor recommender = null;

	// ID mapping objects
	static IEntityMapping user_mapping = new EntityMapping();
	static IEntityMapping item_mapping = new EntityMapping();

	// time statistics
	static List<double> training_time_stats = new List<double>();
	static List<double> fit_time_stats      = new List<double>();
	static List<double> eval_time_stats     = new List<double>();
	static List<double> rmse_eval_stats     = new List<double>();

	// global command line parameters
	static string training_file         = null;
	static string test_file             = null;
	static bool compute_fit             = false;
	static RatingFileFormat file_format = RatingFileFormat.DEFAULT;
	static RatingType rating_type       = RatingType.DOUBLE;

	static void ShowVersion()
	{
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite Rating Prediction {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine("Copyright (C) 2010, 2011 Zeno Gantner, Steffen Rendle");
	    Console.WriteLine("This is free software; see the source for copying conditions.  There is NO");
        Console.WriteLine("warranty; not even for MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.");
		Environment.Exit(0);
	}

	static void Usage(string message)
	{
		Console.WriteLine(message);
		Console.WriteLine();
		Usage(-1);
	}

	static void Usage(int exit_code)
	{
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite Rating Prediction {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine(@"
 usage:  RatingPrediction.exe --training-file=FILE --recommender=METHOD [OPTIONS]

  recommenders (plus options and their defaults):");

			Console.Write("   - ");
			Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.RatingPrediction")));

			Console.WriteLine(@"  method ARGUMENTS have the form name=value

  general OPTIONS:
   --recommender=METHOD             set recommender method (default: BiasedMatrixFactorization)
   --recommender-options=OPTIONS    use OPTIONS as recommender options
   --training-file=FILE             read training data from FILE
   --test-file=FILE                 read test data from FILE

   --help                           display this usage information and exit
   --version                        display version information and exit

   --random-seed=N                        set random seed to N
   --data-dir=DIR                         load all files from DIR
   --user-attributes=FILE                 file containing user attribute information
   --item-attributes=FILE                 file containing item attribute information
   --user-relations=FILE                  file containing user relation information
   --item-relations=FILE                  file containing item relation information
   --save-model=FILE                      save computed model to FILE
   --load-model=FILE                      load model from FILE
   --min-rating=NUM                       the smallest valid rating value
   --max-rating=NUM                       the greatest valid rating value
   --prediction-file=FILE                 write the rating predictions to  FILE ('-' for STDOUT)
   --prediction-line=FORMAT               format of the prediction line; {0}, {1}, {2} refer to user ID, item ID,
                                          and predicted rating, respectively; default is {0}\\t{1}\\t{2}
   --file-format=ml1m|kddcup2011|default
   --rating-type=float|byte|double        store ratings as floats or bytes or doubles (default)
   --cross-validation=K                   perform k-fold crossvalidation on the training data
   --split-ratio=NUM                      use a ratio of NUM of the training data for evaluation (simple split)
   --online-evaluation                    perform online evaluation (use every tested rating for incremental training)
   --search-hp                            search for good hyperparameter values (experimental)

  options for finding the right number of iterations (MF methods)
   --find-iter=N                  give out statistics every N iterations
   --max-iter=N                   perform at most N iterations
   --epsilon=NUM                  abort iterations if RMSE is more than best result plus NUM
   --rmse-cutoff=NUM              abort if RMSE is above NUM
   --mae-cutoff=NUM               abort if MAE is above NUM
   --compute-fit                  display fit on training data every find_iter iterations");

		Environment.Exit(exit_code);
	}

    static void Main(string[] args)
    {
		Assembly assembly = Assembly.GetExecutingAssembly();
		Assembly.LoadFile(Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar + "MyMediaLiteExperimental.dll");

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);

		// recommender arguments
		string method              = "BiasedMatrixFactorization";
		string recommender_options = string.Empty;

		// help/version
		bool show_help    = false;
		bool show_version = false;

		// arguments for iteration search
		int find_iter      = 0;
		int max_iter       = 500;
		double epsilon     = 0;
		double rmse_cutoff = double.MaxValue;
		double mae_cutoff  = double.MaxValue;

		// data characteristics
		double min_rating  = 1;
		double max_rating  = 5;

		// data arguments
		string data_dir             = string.Empty;
		string user_attributes_file = string.Empty;
		string item_attributes_file = string.Empty;
		string user_relations_file  = string.Empty;
		string item_relations_file  = string.Empty;

		// other arguments
		bool online_eval       = false;
		bool search_hp         = false;
		string save_model_file = string.Empty;
		string load_model_file = string.Empty;
		int random_seed        = -1;
		string prediction_file = string.Empty;
		string prediction_line = "{0}\t{1}\t{2}";
		int cross_validation   = 0;
		double split_ratio     = 0;

	   	var p = new OptionSet() {
			// string-valued options
			{ "training-file=",       v              => training_file        = v },
			{ "test-file=",           v              => test_file            = v },
			{ "recommender=",         v              => method               = v },
			{ "recommender-options=", v              => recommender_options += " " + v },
   			{ "data-dir=",            v              => data_dir             = v },
			{ "user-attributes=",     v              => user_attributes_file = v },
			{ "item-attributes=",     v              => item_attributes_file = v },
			{ "user-relations=",      v              => user_relations_file  = v },
			{ "item-relations=",      v              => item_relations_file  = v },
			{ "save-model=",          v              => save_model_file      = v },
			{ "load-model=",          v              => load_model_file      = v },
			{ "prediction-file=",     v              => prediction_file      = v },
			{ "prediction-line=",     v              => prediction_line      = v },
			// integer-valued options
   			{ "find-iter=",           (int v)        => find_iter            = v },
			{ "max-iter=",            (int v)        => max_iter             = v },
			{ "random-seed=",         (int v)        => random_seed          = v },
			{ "cross-validation=",    (int v)        => cross_validation     = v },
			// double-valued options
			{ "min-rating=",          (double v)     => min_rating           = v },
			{ "max-rating=",          (double v)     => max_rating           = v },
			{ "epsilon=",             (double v)     => epsilon              = v },
			{ "rmse-cutoff=",         (double v)     => rmse_cutoff          = v },
			{ "mae-cutoff=",          (double v)     => mae_cutoff           = v },
			{ "split-ratio=",         (double v)     => split_ratio          = v },
			// enum options
			{ "rating-type=",         (RatingType v) => rating_type          = v },
			{ "file-format=",         (RatingFileFormat v) => file_format    = v },
			// boolean options
			{ "compute-fit",          v => compute_fit  = v != null },
			{ "online-evaluation",    v => online_eval  = v != null },
			{ "search-hp",            v => search_hp    = v != null },
			{ "help",                 v => show_help    = v != null },
			{ "version",              v => show_version = v != null },
   	  	};
   		IList<string> extra_args = p.Parse(args);

		// TODO make sure interaction of --find-iter and --cross-validation works properly

		bool no_eval = test_file == null;

		if (show_version)
			ShowVersion();
		if (show_help)
			Usage(0);

		if (extra_args.Count > 0)
			Usage("Did not understand " + extra_args[0]);

		if (training_file == null)
			Usage("Parameter --training-file=FILE is missing.");

		if (cross_validation != 0 && split_ratio != 0)
			Usage("--cross-validation=K and --split-ratio=NUM are mutually exclusive.");

		if (random_seed != -1)
			MyMediaLite.Util.Random.InitInstance(random_seed);

		recommender = Recommender.CreateRatingPredictor(method);
		if (recommender == null)
			Usage(string.Format("Unknown method: '{0}'", method));

		Recommender.Configure(recommender, recommender_options, Usage);

		// ID mapping objects
		if (file_format == RatingFileFormat.KDDCUP_2011)
		{
			user_mapping = new IdentityMapping();
			item_mapping = new IdentityMapping();
		}

		// load all the data
		LoadData(data_dir, min_rating, max_rating, user_attributes_file, item_attributes_file, user_relations_file, item_relations_file, !online_eval);

		recommender.MinRating = min_rating;
		recommender.MaxRating = max_rating;
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));

		if (split_ratio > 0)
		{
			var split = new RatingsSimpleSplit(training_data, split_ratio);
			recommender.Ratings = split.Train[0];
			training_data = split.Train[0];
			test_data     = split.Test[0];
		}

		Utils.DisplayDataStats(training_data, test_data, recommender);

		if (find_iter != 0)
		{
			if ( !(recommender is IIterativeModel) )
				Usage("Only iterative recommenders support find_iter.");
			var iterative_recommender = (IIterativeModel) recommender;
			Console.WriteLine(recommender.ToString() + " ");

			if (load_model_file == string.Empty)
				recommender.Train();
			else
				Recommender.LoadModel(iterative_recommender, load_model_file);

			if (compute_fit)
				Console.Write(string.Format(CultureInfo.InvariantCulture, "fit {0,0:0.#####} ", iterative_recommender.ComputeFit()));

			MyMediaLite.Eval.Ratings.DisplayResults(MyMediaLite.Eval.Ratings.Evaluate(recommender, test_data));
			Console.WriteLine(" " + iterative_recommender.NumIter);

			for (int i = (int) iterative_recommender.NumIter + 1; i <= max_iter; i++)
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
						Console.Write(string.Format(CultureInfo.InvariantCulture, "fit {0,0:0.#####} ", fit));
					}

					Dictionary<string, double> results = null;
					time = Utils.MeasureTime(delegate() {
						results = MyMediaLite.Eval.Ratings.Evaluate(recommender, test_data);
						MyMediaLite.Eval.Ratings.DisplayResults(results);
						rmse_eval_stats.Add(results["RMSE"]);
						Console.WriteLine(" " + i);
					});
					eval_time_stats.Add(time.TotalSeconds);

					Recommender.SaveModel(recommender, save_model_file, i);
					if (prediction_file != string.Empty)
						Prediction.WritePredictions(recommender, test_data, user_mapping, item_mapping, prediction_line, prediction_file + "-it-" + i);

					if (epsilon > 0.0 && results["RMSE"] - rmse_eval_stats.Min() > epsilon)
					{
						Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} >> {1}", results["RMSE"], rmse_eval_stats.Min()));
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

			DisplayStats();
		}
		else
		{
			TimeSpan seconds;

			if (load_model_file == string.Empty)
			{
				if (cross_validation > 0)
				{
					Console.Write(recommender.ToString());
					Console.WriteLine();
					var split = new RatingCrossValidationSplit(training_data, cross_validation);
					var results = MyMediaLite.Eval.Ratings.EvaluateOnSplit(recommender, split); // TODO if (search_hp)
					MyMediaLite.Eval.Ratings.DisplayResults(results);
					no_eval = true;
					recommender.Ratings = training_data;
				}
				else
				{
					if (search_hp)
					{
						// TODO --search-hp-criterion=RMSE
						double result = NelderMead.FindMinimum("RMSE", recommender);
						Console.Error.WriteLine("estimated quality (on split) {0}", result.ToString(CultureInfo.InvariantCulture));
						// TODO give out hp search time
					}

					Console.Write(recommender.ToString());
					seconds = Utils.MeasureTime( delegate() { recommender.Train(); } );
        			Console.Write(" training_time " + seconds + " ");
				}
			}
			else
			{
				Recommender.LoadModel(recommender, load_model_file);
				Console.Write(recommender.ToString() + " ");
			}

			if (!no_eval)
			{
				if (online_eval)  // TODO support also for prediction outputs (to allow external evaluation)
					seconds = Utils.MeasureTime(delegate() { MyMediaLite.Eval.Ratings.DisplayResults(MyMediaLite.Eval.Ratings.EvaluateOnline(recommender, test_data)); });
				else
					seconds = Utils.MeasureTime(delegate() { MyMediaLite.Eval.Ratings.DisplayResults(MyMediaLite.Eval.Ratings.Evaluate(recommender, test_data)); });

				Console.Write(" testing_time " + seconds);
			}

			if (compute_fit)
			{
				Console.Write("fit ");
				seconds = Utils.MeasureTime(delegate() {
					MyMediaLite.Eval.Ratings.DisplayResults(MyMediaLite.Eval.Ratings.Evaluate(recommender, training_data));
				});
				Console.Write(string.Format(CultureInfo.InvariantCulture, " fit_time {0,0:0.#####} ", seconds));
			}

			if (prediction_file != string.Empty)
			{
				seconds = Utils.MeasureTime(delegate() {
						Console.WriteLine();
						Prediction.WritePredictions(recommender, test_data, user_mapping, item_mapping, prediction_line, prediction_file);
				});
				Console.Error.Write("predicting_time " + seconds);
			}

			Console.WriteLine();
			Console.Error.WriteLine("memory {0}", Memory.Usage);
		}
		Recommender.SaveModel(recommender, save_model_file);
	}

    static void LoadData(string data_dir, double min_rating, double max_rating,
	                     string user_attributes_file, string item_attributes_file,
	                     string user_relation_file, string item_relation_file,
	                     bool static_data)
	{
		if (training_file == null)
			Usage("Program expects --training-file=FILE.");

		TimeSpan loading_time = Utils.MeasureTime(delegate() {
			// read training data
			if (file_format == RatingFileFormat.DEFAULT)
				training_data = static_data ? RatingPredictionStatic.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping, rating_type)
					                        : MyMediaLite.IO.RatingPrediction.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping);
			else if (file_format == RatingFileFormat.MOVIELENS_1M)
				training_data = MovieLensRatingData.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping);
			else if (file_format == RatingFileFormat.KDDCUP_2011)
				training_data = MyMediaLite.IO.KDDCup2011.Ratings.Read(Path.Combine(data_dir, training_file));

			recommender.Ratings = training_data;

			// user attributes
			if (recommender is IUserAttributeAwareRecommender) // TODO also support the MovieLens format here
			{
				if (user_attributes_file == string.Empty)
					Usage("Recommender expects --user-attributes=FILE.");
				else
					((IUserAttributeAwareRecommender)recommender).UserAttributes = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
			}

			// item attributes
			if (recommender is IItemAttributeAwareRecommender)
			{
				if (item_attributes_file == string.Empty)
					Usage("Recommender expects --item-attributes=FILE.");
				else
					((IItemAttributeAwareRecommender)recommender).ItemAttributes = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
			}

			// user relation
			if (recommender is IUserRelationAwareRecommender)
				if (user_relation_file == string.Empty)
				{
					Usage("Recommender expects --user-relations=FILE.");
				}
				else
				{
					((IUserRelationAwareRecommender)recommender).UserRelation = RelationData.Read(Path.Combine(data_dir, user_relation_file), user_mapping);
					Console.WriteLine("relation over {0} users", ((IUserRelationAwareRecommender)recommender).NumUsers);
				}

			// item relation
			if (recommender is IItemRelationAwareRecommender)
				if (user_relation_file == string.Empty)
				{
					Usage("Recommender expects --item-relations=FILE.");
				}
				else
				{
					((IItemRelationAwareRecommender)recommender).ItemRelation = RelationData.Read(Path.Combine(data_dir, item_relation_file), item_mapping);
					Console.WriteLine("relation over {0} items", ((IItemRelationAwareRecommender)recommender).NumItems);
				}

			// read test data
			if (test_file != null)
			{
				if (file_format == RatingFileFormat.MOVIELENS_1M)
					test_data = MovieLensRatingData.Read(Path.Combine(data_dir, test_file), min_rating, max_rating, user_mapping, item_mapping);
				else
					test_data = RatingPredictionStatic.Read(Path.Combine(data_dir, test_file), min_rating, max_rating, user_mapping, item_mapping, rating_type);
				// TODO add KDD Cup
			}
		});
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loading_time {0,0:0.##}", loading_time.TotalSeconds));
	}

	static void AbortHandler(object sender, ConsoleCancelEventArgs args)
	{
		DisplayStats();
	}

	static void DisplayStats()
	{
		if (training_time_stats.Count > 0)
			Console.Error.WriteLine(string.Format(
			    CultureInfo.InvariantCulture,
				"iteration_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
	            training_time_stats.Min(), training_time_stats.Max(), training_time_stats.Average()
			));
		if (eval_time_stats.Count > 0)
			Console.Error.WriteLine(string.Format(
			    CultureInfo.InvariantCulture,
				"eval_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
	            eval_time_stats.Min(), eval_time_stats.Max(), eval_time_stats.Average()
			));
		if (compute_fit && fit_time_stats.Count > 0)
			Console.Error.WriteLine(string.Format(
			    CultureInfo.InvariantCulture,
				"fit_time: min={0,0:0.##}, max={1,0:0.##}, avg={2,0:0.##}",
            	fit_time_stats.Min(), fit_time_stats.Max(), fit_time_stats.Average()
			));
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}
}
