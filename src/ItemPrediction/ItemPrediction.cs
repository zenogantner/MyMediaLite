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
using MyMediaLite.ItemRecommendation;
using MyMediaLite.Util;

/// <summary>Item prediction program, see Usage() method for more information</summary>
class ItemPrediction
{
	static IPosOnlyFeedback training_data;
	static IPosOnlyFeedback test_data;
	static ICollection<int> relevant_users;
	static ICollection<int> relevant_items;

	// recommenders
	static IItemRecommender recommender = null;

	// ID mapping objects
	static IEntityMapping user_mapping = new EntityMapping();
	static IEntityMapping item_mapping = new EntityMapping();

	// command-line parameters (data)
	static string training_file        = null;
	static string test_file            = null;
	static string data_dir             = string.Empty;
	static string relevant_users_file  = null;
	static string relevant_items_file  = null;
	static string user_attributes_file = null;
	static string item_attributes_file = null;
	static string user_relations_file  = null;
	static string item_relations_file  = null;

	// command-line parameters (other)
	static bool compute_fit;
	static double test_ratio;
	static int predict_items_number = -1;

	// time statistics
	static List<double> training_time_stats = new List<double>();
	static List<double> fit_time_stats      = new List<double>();
	static List<double> eval_time_stats     = new List<double>();

	static void ShowVersion()
	{
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite Item Prediction from Implicit Feedback {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine("Copyright (C) 2010, 2011 Zeno Gantner, Steffen Rendle, Christoph Freudenthaler");
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
		Console.WriteLine("MyMediaLite Item Prediction from Implicit Feedback {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine(@"
 usage:   ItemPrediction.exe --training-file=FILE --recommender=METHOD [OPTIONS]

   methods (plus arguments and their defaults):");

		Console.Write("   - ");
		Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.ItemRecommendation")));

		Console.WriteLine(@"  method ARGUMENTS have the form name=value

  general OPTIONS:
   --recommender=METHOD             set recommender method (default: BiasedMatrixFactorization)
   --recommender-options=OPTIONS    use OPTIONS as recommender options
   --training-file=FILE             read training data from FILE
   --test-file=FILE                 read test data from FILE

   --help                           display this usage information and exit
   --version                        display version information and exit

   --random-seed=N
   --data-dir=DIR               load all files from DIR
   --relevant-items=FILE        use the items in FILE (one per line) as candidate items, otherwise all items in the training set
   --relevant-users=FILE        predict items for users specified in FILE (one user per line)
   --user-attributes=FILE       file containing user attribute information
   --item-attributes=FILE       file containing item attribute information
   --user-relations=FILE        file containing user relation information
   --item-relations=FILE        file containing item relation information
   --save-model=FILE            save computed model to FILE
   --load-model=FILE            load model from FILE
   --prediction-file=FILE       write ranked predictions to FILE ('-' for STDOUT), one user per line
   --predict-items-number=N     predict N items per user (needs --predict-items-file)
   --test-ratio=NUM             evaluate by splitting of a NUM part of the feedback
   --online-evaluation          perform online evaluation (use every tested user-item combination for online training)

  options for finding the right number of iterations (MF methods and BPR-Linear)
   --find-iter=N                give out statistics every N iterations
   --max-iter=N                 perform at most N iterations
   --auc-cutoff=NUM             abort if AUC is below NUM
   --prec5-cutoff=NUM           abort if prec@5 is below NUM
   --compute-fit                display fit on training data every find_iter iterations");
		Environment.Exit(exit_code);
	}

    public static void Main(string[] args)
    {
		Assembly assembly = Assembly.GetExecutingAssembly();
		Assembly.LoadFile(Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar + "MyMediaLiteExperimental.dll");

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyMediaLite.Util.Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);

		// recommender arguments
		string method              = "MostPopular";
		string recommender_options = string.Empty;

		// help/version
		bool show_help    = false;
		bool show_version = false;

		// variables for iteration search
		int find_iter       = 0;
		int max_iter        = 500;
		double auc_cutoff   = 0;
		double prec5_cutoff = 0;
		compute_fit         = false;

		// other parameters
		string save_model_file        = string.Empty;
		string load_model_file        = string.Empty;
		int random_seed               = -1;
		string prediction_file        = string.Empty;
		bool online_eval              = false;
		test_ratio                    = 0;

	   	var p = new OptionSet() {
			// string-valued options
			{ "training-file=",       v => training_file          = v },
			{ "test-file=",           v => test_file              = v },
			{ "recommender=",         v => method                 = v },
			{ "recommender-options=", v => recommender_options   += " " + v },
   			{ "data-dir=",            v => data_dir               = v },
			{ "user-attributes=",     v => user_attributes_file   = v },
			{ "item-attributes=",     v => item_attributes_file   = v },
			{ "user-relations=",      v => user_relations_file    = v },
			{ "item-relations=",      v => item_relations_file    = v },
			{ "save-model=",          v => save_model_file        = v },
			{ "load-model=",          v => load_model_file        = v },
			{ "prediction-file=",     v => prediction_file        = v },
			{ "relevant-users=",      v => relevant_users_file    = v },
			{ "relevant-items=",      v => relevant_items_file    = v },
			// integer-valued options
   			{ "find-iter=",            (int v) => find_iter            = v },
			{ "max-iter=",             (int v) => max_iter             = v },
			{ "random-seed=",          (int v) => random_seed          = v },
			{ "predict-items-number=", (int v) => predict_items_number = v },
			// double-valued options
//			{ "epsilon=",             (double v) => epsilon      = v },
			{ "auc-cutoff=",          (double v) => auc_cutoff   = v },
			{ "prec5-cutoff=",        (double v) => prec5_cutoff = v },
			{ "test-ratio=",          (double v) => test_ratio   = v },
			// enum options
			//   * currently none *
			// boolean options
			{ "compute-fit",          v => compute_fit  = v != null },
			{ "online-evaluation",    v => online_eval  = v != null },
			{ "help",                 v => show_help    = v != null },
			{ "version",              v => show_version = v != null },

   	  	};
   		IList<string> extra_args = p.Parse(args);

		if (show_version)
			ShowVersion();
		if (show_help)
			Usage(0);

		bool no_eval = test_file == null;

		if (training_file == null)
			Usage("Parameter --training-file=FILE is missing.");

		if (extra_args.Count > 0)
			Usage("Did not understand " + extra_args[0]);

		if (random_seed != -1)
			MyMediaLite.Util.Random.InitInstance(random_seed);

		recommender = Recommender.CreateItemRecommender(method);
		if (recommender == null)
			Usage(string.Format("Unknown method: '{0}'", method));

		Recommender.Configure(recommender, recommender_options, Usage);

		// load all the data
		LoadData();
		Utils.DisplayDataStats(training_data, test_data, recommender);

		TimeSpan time_span;

		if (find_iter != 0)
		{
			var iterative_recommender = (IIterativeModel) recommender;
			Console.WriteLine(recommender.ToString() + " ");

			if (load_model_file == string.Empty)
				iterative_recommender.Train();
			else
				Recommender.LoadModel(iterative_recommender, load_model_file);

			if (compute_fit)
				Console.Write(string.Format(CultureInfo.InvariantCulture, "fit {0,0:0.#####} ", iterative_recommender.ComputeFit()));

			var result = ItemPredictionEval.Evaluate(recommender, test_data, training_data, relevant_users, relevant_items);
			ItemPredictionEval.DisplayResults(result);
			Console.WriteLine(" " + iterative_recommender.NumIter);

			for (int i = (int) iterative_recommender.NumIter + 1; i <= max_iter; i++)
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
						Console.Write(string.Format(CultureInfo.InvariantCulture, "fit {0,0:0.#####} ", fit));
					}

					t = Utils.MeasureTime(delegate() {
						result = ItemPredictionEval.Evaluate(
							recommender,
						    test_data,
							training_data,
						    test_data.AllUsers,
							relevant_items
						);
						ItemPredictionEval.DisplayResults(result);
						Console.WriteLine(" " + i);
					});
					eval_time_stats.Add(t.TotalSeconds);

					Recommender.SaveModel(recommender, save_model_file, i);
					Predict(prediction_file, relevant_users_file, i);

					if (result["AUC"] < auc_cutoff || result["prec@5"] < prec5_cutoff)
					{
							Console.Error.WriteLine("Reached cutoff after {0} iterations.", i);
							Console.Error.WriteLine("DONE");
							break;
					}
				}
			} // for
			DisplayStats();
		}
		else
		{
			if (load_model_file == string.Empty)
			{
				Console.Write(recommender.ToString() + " ");
				time_span = Utils.MeasureTime( delegate() { recommender.Train(); } );
        		Console.Write("training_time " + time_span + " ");
			}
			else
			{
				Recommender.LoadModel(recommender, load_model_file);
				Console.Write(recommender.ToString() + " ");
				// TODO is this the right time to load the model?
			}

			if (prediction_file != string.Empty)
			{
				Predict(prediction_file, relevant_users_file);
			}
			else if (!no_eval)
			{
				if (online_eval)
					time_span = Utils.MeasureTime( delegate() {
						var result = ItemPredictionEval.EvaluateOnline(recommender, test_data, training_data, relevant_users, relevant_items); // TODO support also for prediction outputs (to allow external evaluation)
						ItemPredictionEval.DisplayResults(result);
			    	});
				else
					time_span = Utils.MeasureTime( delegate() {
						var result = ItemPredictionEval.Evaluate(recommender, test_data, training_data, test_data.AllUsers, relevant_items);
						ItemPredictionEval.DisplayResults(result);
			    	});
				Console.Write(" testing_time " + time_span);
			}
			Console.WriteLine();
		}
		Recommender.SaveModel(recommender, save_model_file);
	}

    static void LoadData()
	{
		TimeSpan loading_time = Utils.MeasureTime(delegate() {
			// training data
			training_data = ItemRecommendation.Read(Path.Combine(data_dir, training_file), user_mapping, item_mapping);

			// relevant users and items
			if (relevant_users_file != null)
				relevant_users = new HashSet<int>(user_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_users_file))));
			else
				relevant_users = training_data.AllUsers;
			if (relevant_items_file != null)
				relevant_items = new HashSet<int>(item_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_items_file))));
			else
				relevant_items = training_data.AllItems;

			if (! (recommender is MyMediaLite.ItemRecommendation.Random))
				((ItemRecommender)recommender).Feedback = training_data;

			// user attributes
			if (recommender is IUserAttributeAwareRecommender)
			{
				if (user_attributes_file == null)
					Usage("Recommender expects user_attributes=FILE.");
				else
					((IUserAttributeAwareRecommender)recommender).UserAttributes = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
			}

			// item attributes
			if (recommender is IItemAttributeAwareRecommender)
			{
				if (item_attributes_file == null)
					Usage("Recommender expects item_attributes=FILE.");
				else
					((IItemAttributeAwareRecommender)recommender).ItemAttributes = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
			}

			// user relation
			if (recommender is IUserRelationAwareRecommender)
				if (user_relations_file == null)
				{
					Usage("Recommender expects user_relation=FILE.");
				}
				else
				{
					((IUserRelationAwareRecommender)recommender).UserRelation = RelationData.Read(Path.Combine(data_dir, user_relations_file), user_mapping);
					Console.WriteLine("relation over {0} users", ((IUserRelationAwareRecommender)recommender).NumUsers); // TODO move to DisplayDataStats
				}

			// item relation
			if (recommender is IItemRelationAwareRecommender)
				if (user_relations_file == null)
				{
					Usage("Recommender expects item_relation=FILE.");
				}
				else
				{
					((IItemRelationAwareRecommender)recommender).ItemRelation = RelationData.Read(Path.Combine(data_dir, item_relations_file), item_mapping);
					Console.WriteLine("relation over {0} items", ((IItemRelationAwareRecommender)recommender).NumItems); // TODO move to DisplayDataStats
				}

			// test data
			if (test_ratio == 0)
			{
				if (test_file != null)
	        		test_data = ItemRecommendation.Read(Path.Combine(data_dir, test_file), user_mapping, item_mapping);
			}
			else
			{
				var split = new PosOnlyFeedbackSimpleSplit<PosOnlyFeedback<SparseBooleanMatrix>>(training_data, test_ratio);
				training_data = split.Train[0];
				test_data     = split.Test[0];
			}
		});
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loading_time {0,0:0.##}", loading_time.TotalSeconds));
	}

	static void Predict(string prediction_file, string predict_for_users_file, int iteration)
	{
		if (prediction_file == string.Empty)
			return;

		Predict(prediction_file + "-it-" + iteration, predict_for_users_file);
	}

	static void Predict(string prediction_file, string predict_for_users_file)
	{
		TimeSpan time_span;

		if (predict_for_users_file == string.Empty)
			time_span = Utils.MeasureTime( delegate()
		    	{
			    	MyMediaLite.Eval.ItemPrediction.WritePredictions(
				    	recommender,
				        training_data,
				        relevant_items, predict_items_number,
				        user_mapping, item_mapping,
				        prediction_file
					);
					Console.Error.WriteLine("Wrote predictions to {0}", prediction_file);
		    	}
			);
		else
			time_span = Utils.MeasureTime( delegate()
		    	{
			    	MyMediaLite.Eval.ItemPrediction.WritePredictions(
				    	recommender,
				        training_data,
				        user_mapping.ToInternalID(Utils.ReadIntegers(predict_for_users_file)),
				        relevant_items, predict_items_number,
				        user_mapping, item_mapping,
				        prediction_file
					);
					Console.Error.WriteLine("Wrote predictions for selected users to {0}", prediction_file);
		    	}
			);
		Console.Write(" predicting_time " + time_span);
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
				"eval_time: min={0,0:0.###}, max={1,0:0.###}, avg={2,0:0.###}",
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
