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
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.AttrToFactor;
using MyMediaLite.IO;
using MyMediaLite.Util;

/// <summary></summary>
/// <remarks>
/// <inproceedings>
///   <author>Zeno Gantner</author>
///   <author>Lucas Drumond</author>
///   <author>Christoph Freudenthaler</author>
///   <author>Steffen Rendle</author>
///   <author>Lars Schmidt-Thieme</author>
///   <title>Learning Attribute-to-Feature Mappings for Cold-start Recommendations</title>
///   <booktitle>IEEE International Conference on Data Mining (ICDM 2010)</booktitle>
///   <location>Sydney, Australia</location>
///   <year>2010</year>
/// </inproceedings>
/// </remarks>
class MappingItemPrediction
{
	static NumberFormatInfo ni = new NumberFormatInfo();

	static PosOnlyFeedback training_data;
	static PosOnlyFeedback test_data;
	static ICollection<int> relevant_items;

	static BPRMF_Mapping recommender;
	static BPRMF_ItemMapping bprmf_map             = new BPRMF_ItemMapping();
	static BPRMF_ItemMapping_Optimal bprmf_map_bpr = new BPRMF_ItemMapping_Optimal();
	static BPRMF_ItemMapping bprmf_map_com         = new BPRMF_ItemMapping_Complex();
	static BPRMF_ItemMapping bprmf_map_knn         = new BPRMF_ItemMappingKNN();
	static BPRMF_ItemMapping bprmf_map_svr         = new BPRMF_ItemMappingSVR();
	static BPRMF_Mapping bprmf_user_map            = new BPRMF_UserMapping();
	static BPRMF_Mapping bprmf_user_map_bpr        = new BPRMF_UserMapping_Optimal();

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
		Console.WriteLine("    - " + bprmf_map     + " (needs item_attributes)");
		Console.WriteLine("    - " + bprmf_map_bpr + " (needs item_attributes)");
		Console.WriteLine("    - " + bprmf_map_com + " (needs item_attributes)");
		Console.WriteLine("    - " + bprmf_map_knn + " (needs item_attributes)");
		Console.WriteLine("    - " + bprmf_map_svr + " (needs item_attributes)");
		Console.WriteLine("    - " + bprmf_user_map     + " (needs user_attributes)");
		Console.WriteLine("    - " + bprmf_user_map_bpr + " (needs user_attributes)");
		Console.WriteLine("  - method ARGUMENTS have the form name=value");
		Console.WriteLine("  - general OPTIONS have the form name=value");
		Console.WriteLine("    - random_seed=N");
		Console.WriteLine("    - data_dir=DIR           load all files from DIR");
		Console.WriteLine("    - relevant_items=FILE    use only item in the given file for evaluation");
		Console.WriteLine("    - item_attributes=FILE   file containing item attribute information");
		Console.WriteLine("    - user_attributes=FILE   file containing user attribute information");
		//Console.WriteLine("    - save_mappings=FILE     save computed mapping model to FILE");
		Console.WriteLine("    - no_eval=BOOL           don't evaluate, only run the mapping");
		Console.WriteLine("    - compute_fit=N          compute fit every N iterations");

		Environment.Exit (exit_code);
	}

    public static void Main(string[] args)
    {
		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);
		ni.NumberDecimalDigits = '.';

		// check number of command line parameters
		if (args.Length < 4)
			Usage("Not enough arguments.");

		// read command line parameters
		CommandLineParameters parameters = null;
		try	{ parameters = new CommandLineParameters(args, 4);	}
		catch (ArgumentException e)	{ Usage(e.Message); 		}

		// other parameters
		string data_dir             = parameters.GetRemoveString( "data_dir");
		string relevant_items_file  = parameters.GetRemoveString( "relevant_items");
		string item_attributes_file = parameters.GetRemoveString( "item_attributes");
		string user_attributes_file = parameters.GetRemoveString( "user_attributes");
		//string save_mapping_file    = parameters.GetRemoveString( "save_model");
		int random_seed             = parameters.GetRemoveInt32(  "random_seed", -1);
		bool no_eval                = parameters.GetRemoveBool(   "no_eval", false);
		bool compute_fit            = parameters.GetRemoveBool(   "compute_fit", false);

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
			case "BPR-MF-ItemMapping":
				recommender = Recommender.Configure(bprmf_map, parameters, Usage);
				break;
			case "BPR-MF-ItemMapping-Optimal":
				recommender = Recommender.Configure(bprmf_map_bpr, parameters, Usage);
				break;
			case "BPR-MF-ItemMapping-Complex":
				recommender = Recommender.Configure(bprmf_map_com, parameters, Usage);
				break;
			case "BPR-MF-ItemMapping-kNN":
				recommender = Recommender.Configure(bprmf_map_knn, parameters, Usage);
				break;
			case "BPR-MF-ItemMapping-SVR":
				recommender = Recommender.Configure(bprmf_map_svr, parameters, Usage);
				break;
			case "BPR-MF-UserMapping":
				recommender = Recommender.Configure(bprmf_user_map, parameters, Usage);
				break;
			case "BPR-MF-UserMapping-Optimal":
				recommender = Recommender.Configure(bprmf_user_map_bpr, parameters, Usage);
				break;
			default:
				Usage(string.Format("Unknown method: '{0}'", method));
				break;
		}

		if (parameters.CheckForLeftovers())
			Usage(-1);

		// ID mapping objects
		var user_mapping = new EntityMapping();
		var item_mapping = new EntityMapping();

		// training data
		training_data = ItemRecommendation.Read(Path.Combine(data_dir, trainfile), user_mapping, item_mapping);
		recommender.Feedback = training_data;

		// relevant items
		if (! relevant_items_file.Equals(string.Empty) )
			relevant_items = new HashSet<int>(item_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_items_file))));
		else
			relevant_items = training_data.AllItems;

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
        test_data = ItemRecommendation.Read( Path.Combine(data_dir, testfile), user_mapping, item_mapping );

		TimeSpan seconds;

		Recommender.LoadModel(recommender, load_model_file);

		// set the maximum user and item IDs in the recommender - this is important for the cold start use case
		recommender.MaxUserID = user_mapping.InternalIDs.Max();
		recommender.MaxItemID = item_mapping.InternalIDs.Max();

		DisplayDataStats();

		Console.Write(recommender.ToString() + " ");

		if (compute_fit)
		{
			seconds = Utils.MeasureTime( delegate() {
				int num_iter = recommender.NumIterMapping;
				recommender.NumIterMapping = 0;
				recommender.LearnAttributeToFactorMapping();
				Console.Error.WriteLine();
				Console.Error.WriteLine(string.Format(ni, "iteration {0} fit {1}", -1, recommender.ComputeFit()));

				recommender.NumIterMapping = 1;
				for (int i = 0; i < num_iter; i++, i++)
				{
					recommender.IterateMapping();
					Console.Error.WriteLine(string.Format(ni, "iteration {0} fit {1}", i, recommender.ComputeFit()));
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
			seconds = EvaluateRecommender(recommender, test_data, training_data);
		Console.WriteLine();
	}

    static TimeSpan EvaluateRecommender(BPRMF_Mapping recommender, PosOnlyFeedback test_data, PosOnlyFeedback train_data)
	{
		Console.Error.WriteLine(string.Format(ni, "fit {0}", recommender.ComputeFit()));

		TimeSpan seconds = Utils.MeasureTime( delegate()
	    	{
	    		var result = ItemPredictionEval.Evaluate(
                                recommender,
								test_data,
        	                    train_data,
			                    test_data.AllUsers,
            	                relevant_items
			    );
				DisplayResults(result);
	    	} );
		Console.Write(" testing " + seconds);

		return seconds;
	}

	static void DisplayResults(Dictionary<string, double> result)
	{
		Console.Write(string.Format(ni, "AUC {0,0:0.#####} prec@5 {1,0:0.#####} prec@10 {2,0:0.#####} MAP {3,0:0.#####} NDCG {4,0:0.#####} num_users {5} num_items {6}",
		                            result["AUC"], result["prec@5"], result["prec@10"], result["MAP"], result["NDCG"], result["num_users"], result["num_items"]));
	}

	static void DisplayDataStats()
	{
		// training data stats
		int num_users = training_data.AllUsers.Count;
		int num_items = training_data.AllItems.Count;
		long matrix_size = (long) num_users * num_items;
		long empty_size  = (long) matrix_size - training_data.Count;
		double sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(ni, "training data: {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));

		// test data stats
		num_users = test_data.AllUsers.Count;
		num_items = test_data.AllItems.Count;
		matrix_size = (long) num_users * num_items;
		empty_size  = (long) matrix_size - test_data.Count;
		sparsity = (double) 100L * empty_size / matrix_size;
		Console.WriteLine(string.Format(ni, "test data:     {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));

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
}
