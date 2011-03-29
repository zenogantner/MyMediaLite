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
using MyMediaLite.ItemRecommendation;
using MyMediaLite.Util;

/// <summary>Item prediction program, see Usage() method for more information</summary>
public class ItemPrediction
{
	static PosOnlyFeedback training_data;
	static PosOnlyFeedback test_data;
	static ICollection<int> relevant_items;

	static NumberFormatInfo ni = new NumberFormatInfo();

	// recommenders
	static IItemRecommender recommender = null;

	static bool compute_fit;
	static double test_ratio;

	// time statistics
	static List<double> training_time_stats = new List<double>();
	static List<double> fit_time_stats      = new List<double>();
	static List<double> eval_time_stats     = new List<double>();

	static void Usage(string message)
	{
		Console.WriteLine(message);
		Console.WriteLine();
		Usage(-1);
	}

	static void Usage(int exit_code)
	{
		Console.WriteLine("MyMediaLite item prediction from implicit feedback");
		Console.WriteLine();
		Console.WriteLine("  usage:   ItemPrediction.exe TRAINING_FILE TEST_FILE METHOD [ARGUMENTS] [OPTIONS]");
		Console.WriteLine();
		Console.WriteLine("   use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN");
		Console.WriteLine();
		Console.WriteLine("   methods (plus arguments and their defaults):");

		Console.Write("   - ");
		Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.ItemRecommendation")));

		Console.WriteLine("  method ARGUMENTS have the form name=value");
		Console.WriteLine();
		Console.WriteLine("  general OPTIONS have the form name=value");
		Console.WriteLine("   - option_file=FILE           read options from FILE (line format KEY: VALUE)");
		Console.WriteLine("   - random_seed=N");
		Console.WriteLine("   - data_dir=DIR               load all files from DIR");
		Console.WriteLine("   - relevant_items=FILE        use the items in FILE for evaluation, otherwise all items that occur in the training set");
		Console.WriteLine("   - user_attributes=FILE       file containing user attribute information");
		Console.WriteLine("   - item_attributes=FILE       file containing item attribute information");
		Console.WriteLine("   - user_relation=FILE         file containing user relation information");
		Console.WriteLine("   - item_relation=FILE         file containing item relation information");
		Console.WriteLine("   - save_model=FILE            save computed model to FILE");
		Console.WriteLine("   - load_model=FILE            load model from FILE");
		Console.WriteLine("   - no_eval=BOOL               do not evaluate");
		Console.WriteLine("   - prediction_file=FILE       write predictions to FILE ('-' for STDOUT)");
		Console.WriteLine("   - predict_items_num=N        predict N items per user (needs predict_items_file)");
		Console.WriteLine("   - predict_for_users=FILE     predict items for users specified in FILE (needs predict_items_file)");
		Console.WriteLine("   - test_ratio=NUM             evaluate by splitting of a NUM part of the feedback");
		Console.WriteLine();
		Console.WriteLine("  options for finding the right number of iterations (MF methods and BPR-Linear)");
		Console.WriteLine("   - find_iter=N                give out statistics every N iterations");
		Console.WriteLine("   - max_iter=N                 perform at most N iterations");
		Console.WriteLine("   - auc_cutoff=NUM             abort if AUC is below NUM");
		Console.WriteLine("   - prec5_cutoff=NUM           abort if prec@5 is below NUM");
		Console.WriteLine("   - compute_fit=BOOL           display fit on training data every find_iter iterations");
		Environment.Exit(exit_code);
	}

    public static void Main(string[] args)
    {
		Assembly assembly = Assembly.GetExecutingAssembly();
		Assembly.LoadFile(Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar + "MyMediaLiteExperimental.dll");

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyMediaLite.Util.Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);
		ni.NumberDecimalDigits = '.';

		// check number of command line parameters
		if (args.Length < 3)
			Usage("Not enough arguments.");

		// read command line parameters
		CommandLineParameters parameters = null;
		try	{ parameters = new CommandLineParameters(args, 3); }
		catch (ArgumentException e)	{ Usage(e.Message);  	   }

		// variables for iteration search
		int find_iter                 = parameters.GetRemoveInt32(  "find_iter", 0);
		int max_iter                  = parameters.GetRemoveInt32(  "max_iter", 500);
		double auc_cutoff             = parameters.GetRemoveDouble( "auc_cutoff");
		double prec5_cutoff           = parameters.GetRemoveDouble( "prec5_cutoff");
		compute_fit                   = parameters.GetRemoveBool(   "compute_fit", false);

		// data parameters
		string data_dir               = parameters.GetRemoveString( "data_dir");
		string relevant_items_file    = parameters.GetRemoveString( "relevant_items");
		string user_attributes_file   = parameters.GetRemoveString( "user_attributes");
		string item_attributes_file   = parameters.GetRemoveString( "item_attributes");
		string user_relation_file     = parameters.GetRemoveString( "user_relation");
		string item_relation_file     = parameters.GetRemoveString( "item_relation");

		// other parameters
		string save_model_file        = parameters.GetRemoveString( "save_model");
		string load_model_file        = parameters.GetRemoveString( "load_model");
		int random_seed               = parameters.GetRemoveInt32(  "random_seed", -1);
		bool no_eval                  = parameters.GetRemoveBool(   "no_eval", false);
		string prediction_file        = parameters.GetRemoveString( "prediction_file", string.Empty);
		int predict_items_number      = parameters.GetRemoveInt32(  "predict_items_num", -1);
		string predict_for_users_file = parameters.GetRemoveString( "predict_for_users", string.Empty);
		test_ratio                    = parameters.GetRemoveDouble( "test_ratio", 0);

		// main data files and method
		string trainfile = args[0].Equals("-") ? "-" : Path.Combine(data_dir, args[0]);
		string testfile  = args[1].Equals("-") ? "-" : Path.Combine(data_dir, args[1]);
		string method    = args[2];

		if (random_seed != -1)
			MyMediaLite.Util.Random.InitInstance(random_seed);

		recommender = Recommender.CreateItemRecommender(method);
		if (recommender == null)
			Usage(string.Format("Unknown method: '{0}'", method));

		Recommender.Configure(recommender, parameters, Usage);

		if (parameters.CheckForLeftovers())
			Usage(-1);

		// check command-line parameters
		if (trainfile.Equals("-") && testfile.Equals("-"))
			Usage("Either training OR test data, not both, can be read from STDIN.");

		// ID mapping objects
		var user_mapping = new EntityMapping();
		var item_mapping = new EntityMapping();

		// load all the data
		TimeSpan loading_time = Utils.MeasureTime(delegate() {
			LoadData(data_dir, trainfile, testfile, user_mapping, item_mapping, relevant_items_file, user_attributes_file, item_attributes_file, user_relation_file, item_relation_file);
		});
		Console.WriteLine(string.Format(ni, "loading_time {0,0:0.##}", loading_time.TotalSeconds));

		DisplayDataStats();

		TimeSpan time_span;

		if (find_iter != 0)
		{
			IIterativeModel iterative_recommender = (IIterativeModel) recommender;
			Console.WriteLine(recommender.ToString() + " ");

			if (load_model_file.Equals(string.Empty))
				iterative_recommender.Train();
			else
				Recommender.LoadModel(iterative_recommender, load_model_file);

			if (compute_fit)
				Console.Write(string.Format(ni, "fit {0,0:0.#####} ", iterative_recommender.ComputeFit()));

			var result = ItemPredictionEval.Evaluate(recommender,
			                                 test_data,
				                             training_data,
			                                 test_data.AllUsers,
				                             relevant_items);
			ItemPredictionEval.DisplayResults(result);
			Console.WriteLine(" " + iterative_recommender.NumIter);

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
						Console.Write(string.Format(ni, "fit {0,0:0.#####} ", fit));
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

					if (result["AUC"] < auc_cutoff || result["prec@5"] < prec5_cutoff)
					{
							Console.Error.WriteLine("Reached cutoff after {0} iterations.", i);
							Console.Error.WriteLine("DONE");
							break;
					}
				}
			} // for
			DisplayIterationStats();
		}
		else
		{
			if (load_model_file.Equals(string.Empty))
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

			if (!prediction_file.Equals(string.Empty))
			{
				if (predict_for_users_file.Equals(string.Empty))
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
			else if (!no_eval)
			{
				time_span = Utils.MeasureTime( delegate()
			    	{
				    	var result = ItemPredictionEval.Evaluate(
					    	recommender,
							test_data,
				            training_data,
					        test_data.AllUsers,
				            relevant_items
						);
						ItemPredictionEval.DisplayResults(result);
			    	}
				);
				Console.Write(" testing_time " + time_span);
			}
			Console.WriteLine();
		}
		Console.Error.WriteLine("memory {0}", Memory.Usage);
		Recommender.SaveModel(recommender, save_model_file);
	}

    static void LoadData(string data_dir, string trainfile, string testfile,
	                     EntityMapping user_mapping, EntityMapping item_mapping,
	                     string relevant_items_file,
	                     string user_attributes_file, string item_attributes_file,
	                     string user_relation_file, string item_relation_file)
	{
		// training data
		training_data = ItemRecommendation.Read(trainfile, user_mapping, item_mapping);

		// relevant items
		if (! relevant_items_file.Equals(string.Empty) )
			relevant_items = new HashSet<int>(item_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_items_file))));
		else
			relevant_items = training_data.AllItems;

		if (! (recommender is MyMediaLite.ItemRecommendation.Random))
			((ItemRecommender)recommender).Feedback = training_data;

		// user attributes
		if (recommender is IUserAttributeAwareRecommender)
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

		// test data
		if (test_ratio == 0)
		{
			// normal case
        	test_data = ItemRecommendation.Read(testfile, user_mapping, item_mapping);
		}
		else
		{
			var split = new PosOnlyFeedbackSimpleSplit(training_data, test_ratio);
			training_data = split.Train[0];
			test_data     = split.Test[0];
		}
	}

	static void AbortHandler(object sender, ConsoleCancelEventArgs args)
	{
		DisplayIterationStats();
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
		Console.WriteLine(string.Format(ni, "training data: {0} users, {1} items, {2} events, sparsity {3,0:0.#####}", num_users, num_items, training_data.Count, sparsity));

		// test data stats
		num_users = test_data.AllUsers.Count;
		num_items = test_data.AllItems.Count;
		matrix_size = (long) num_users * num_items;
		empty_size  = (long) matrix_size - test_data.Count;
		sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(ni, "test data:     {0} users, {1} items, {2} events, sparsity {3,0:0.#####}", num_users, num_items, test_data.Count, sparsity));

		// attribute stats
		if (recommender is IUserAttributeAwareRecommender)
			Console.WriteLine("{0} user attributes for {1} users",
			                  ((IUserAttributeAwareRecommender)recommender).NumUserAttributes,
			                  ((IUserAttributeAwareRecommender)recommender).UserAttributes.NumberOfRows);
		if (recommender is IItemAttributeAwareRecommender)
			Console.WriteLine("{0} item attributes for {1} items",
			                  ((IItemAttributeAwareRecommender)recommender).NumItemAttributes,
			                  ((IItemAttributeAwareRecommender)recommender).ItemAttributes.NumberOfRows);
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