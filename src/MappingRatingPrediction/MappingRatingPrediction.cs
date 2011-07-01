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
using MyMediaLite;
using MyMediaLite.AttrToFactor;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

/// <summary>Command-line tool: attribute-to-factor mappings for rating prediction</summary>
/// <remarks>
/// </remarks>
class MappingRatingPrediction
{
	// data sets
	static IRatings training_data;
	static IRatings test_data;

	static MF_Mapping recommender;
	static MF_ItemMapping mf_map = new MF_ItemMapping();
	//static MF_ItemMapping_Optimal mf_map_opt = new MF_ItemMapping_Optimal();
	//static MF_ItemMapping mf_map_knn         = new MF_ItemMapping_kNN();
	//static MF_ItemMapping mf_map_svr         = new MF_ItemMapping_SVR();

	static void Usage(string message)
	{
		Console.Error.WriteLine(message);
		Usage(-1);
	}

	static void Usage(int exit_code)
	{
		Console.WriteLine("MyMediaLite attribute mapping for item prediction; usage:");
		Console.WriteLine(" Mapping.exe TRAINING_FILE TEST_FILE MODEL_FILE METHOD [ARGUMENTS] [OPTIONS]");
		Console.WriteLine("  - methods (plus arguments and their defaults):");
		Console.WriteLine("    - " + mf_map     + " (needs item_attributes)");
		//Console.WriteLine("    - " + mf_map_opt + " (needs item_attributes)");
		//Console.WriteLine("    - " + mf_map_knn + " (needs item_attributes)");
		//Console.WriteLine("    - " + mf_map_svr + " (needs item_attributes)");
		Console.WriteLine("  - method ARGUMENTS have the form name=value");
		Console.WriteLine("  - general OPTIONS have the form name=value");
		Console.WriteLine("    - random_seed=N");
		Console.WriteLine("    - data_dir=DIR           load all data files from DIR");
		Console.WriteLine("    - item_attributes=FILE   file containing item attribute information");
		Console.WriteLine("    - user_attributes=FILE   file containing user attribute information");
		//Console.WriteLine("    - save_mappings=FILE     save computed mapping model to FILE");
		Console.WriteLine("    - min_rating=NUM         the smallest valid rating value");
		Console.WriteLine("    - max_rating=NUM         the greatest valid rating value");
		Console.WriteLine("    - no_eval=BOOL           don't evaluate, only run the mapping");
		Console.WriteLine("    - compute_fit=N          compute fit every N iterations");
		Console.WriteLine("    - prediction_file=FILE   write the rating predictions to  FILE ('-' for STDOUT)");

		Environment.Exit (exit_code);
	}

    public static void Main(string[] args)
    {
		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);

		// check number of command line parameters
		if (args.Length < 4)
			Usage("Not enough arguments.");

		// read command line parameters
		RecommenderParameters parameters = null;
		try	{ parameters = new RecommenderParameters(args, 4);	}
		catch (ArgumentException e)	{ Usage(e.Message); 		}

		// collaborative data characteristics
		double min_rating           = parameters.GetRemoveDouble( "min_rating",  1);
		double max_rating           = parameters.GetRemoveDouble( "max_rating",  5);

		// other parameters
		string data_dir             = parameters.GetRemoveString( "data_dir");
		//Console.Error.WriteLine("data_dir " + data_dir);
		string item_attributes_file = parameters.GetRemoveString( "item_attributes");
		string user_attributes_file = parameters.GetRemoveString( "user_attributes");
		//string save_mapping_file    = parameters.GetRemoveString( "save_model");
		int random_seed             = parameters.GetRemoveInt32(  "random_seed", -1);
		bool no_eval                = parameters.GetRemoveBool(   "no_eval", false);
		bool compute_fit            = parameters.GetRemoveBool(   "compute_fit", false);
		string prediction_file      = parameters.GetRemoveString( "prediction_file");

		if (random_seed != -1)
			MyMediaLite.Util.Random.InitInstance(random_seed);

		// main data files and method
		string trainfile = args[0].Equals("-") ? "-" : Path.Combine(data_dir, args[0]);
		string testfile  = args[1].Equals("-") ? "-" : Path.Combine(data_dir, args[1]);
		string load_model_file = args[2];
		string method    = args[3];

		// set correct recommender
		switch (method)
		{
			case "MF-ItemMapping":
				recommender = Recommender.Configure(mf_map, parameters, Usage);
				break;
//				case "MF-ItemMapping-Optimal":
//					recommender = Recommender.Configure(mf_map_opt, parameters, Usage);
//					break;
//				case "BPR-MF-ItemMapping-kNN":
//					recommender = Recommender.Configure(mf_map_knn, parameters, Usage);
//					break;
//				case "BPR-MF-ItemMapping-SVR":
//					recommender = Recommender.Configure(mf_map_svr, parameters, Usage);
//					break;
			default:
				Usage(string.Format("Unknown method: '{0}'", method));
				break;
		}

		if (parameters.CheckForLeftovers())
			Usage(-1);

		// TODO move loading into its own method

		// ID mapping objects
		EntityMapping user_mapping = new EntityMapping();
		EntityMapping item_mapping = new EntityMapping();

		// training data
		training_data = MyMediaLite.IO.RatingPrediction.Read(Path.Combine(data_dir, trainfile), min_rating, max_rating, user_mapping, item_mapping);
		recommender.Ratings = training_data;

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

		// test data
        test_data = MyMediaLite.IO.RatingPrediction.Read( Path.Combine(data_dir, testfile), min_rating, max_rating, user_mapping, item_mapping );

		TimeSpan seconds;

		Recommender.LoadModel(recommender, load_model_file);

		// set the maximum user and item IDs in the recommender - this is important for the cold start use case
		recommender.MaxUserID = user_mapping.InternalIDs.Max();
		recommender.MaxItemID = item_mapping.InternalIDs.Max();

		// TODO move that into the recommender functionality (set from data)
		recommender.MinRating = min_rating;
		recommender.MaxRating = max_rating;
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));

		DisplayDataStats();

		Console.Write(recommender.ToString() + " ");

		if (compute_fit)
		{
			seconds = Utils.MeasureTime( delegate() {
				int num_iter = recommender.NumIterMapping;
				recommender.NumIterMapping = 0;
				recommender.LearnAttributeToFactorMapping();
				Console.Error.WriteLine();
				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "iteration {0} fit {1}", -1, recommender.ComputeFit()));

				recommender.NumIterMapping = 1;
				for (int i = 0; i < num_iter; i++, i++)
				{
					recommender.IterateMapping();
					Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "iteration {0} fit {1}", i, recommender.ComputeFit()));
				}
				recommender.NumIterMapping = num_iter; // restore
	    	} );
		}
		else
		{
			seconds = Utils.MeasureTime( delegate() {
				recommender.LearnAttributeToFactorMapping();
	    	} );
		}
		Console.Write("mapping_time " + seconds + " ");

		if (!no_eval)
			seconds = EvaluateRecommender(recommender);
		Console.WriteLine();

		if (prediction_file != string.Empty)
		{
			Console.WriteLine();
			seconds = Utils.MeasureTime(
		    	delegate() {
					Prediction.WritePredictions(recommender, test_data, user_mapping, item_mapping, prediction_file);
				}
			);
			Console.Error.WriteLine("predicting_time " + seconds);
		}
	}

    static TimeSpan EvaluateRecommender(MF_Mapping recommender)
	{
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "fit {0}", recommender.ComputeFit()));

		TimeSpan seconds = Utils.MeasureTime( delegate()
	    	{
	    		var result = RatingEval.Evaluate(recommender, test_data);
				RatingEval.DisplayResults(result);
	    	} );
		Console.Write(" testing " + seconds);

		return seconds;
	}

	static void DisplayDataStats()
	{
		// training data stats
		int num_users = training_data.AllUsers.Count;
		int num_items = training_data.AllItems.Count;
		long matrix_size = (long) num_users * num_items;
		long empty_size  = (long) matrix_size - training_data.Count;
		double sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "training data: {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));

		// test data stats
		num_users = test_data.AllUsers.Count;
		num_items = test_data.AllItems.Count;
		matrix_size = (long) num_users * num_items;
		empty_size  = (long) matrix_size - test_data.Count;
		sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "test data:     {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));

		// attribute stats
		if (recommender is IUserAttributeAwareRecommender)
			Console.WriteLine("{0} user attributes", ((IUserAttributeAwareRecommender)recommender).NumUserAttributes);
		if (recommender is IItemAttributeAwareRecommender)
			Console.WriteLine("{0} item attributes", ((IItemAttributeAwareRecommender)recommender).NumItemAttributes);
	}
}
