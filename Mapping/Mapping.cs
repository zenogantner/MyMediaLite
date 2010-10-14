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
using MyMediaLite.experimental.attr_to_feature;
using MyMediaLite.io;
using MyMediaLite.util;


namespace Mapping
{
	/// <summary>
	/// Zeno Gantner, Lucas Drumond, Christoph Freudenthaler, Steffen Rendle, Lars Schmidt-Thieme:
    /// Learning Attribute-to-Feature Mappings for Cold-start Recommendations
    /// Proceedings of the 10th IEEE International Conference on Data Mining (ICDM 2010), Sydney, Australia.
	/// </summary>
	public class Mapping
	{
		static Pair<SparseBooleanMatrix, SparseBooleanMatrix> training_data;
		static Pair<SparseBooleanMatrix, SparseBooleanMatrix> test_data;
		static ICollection<int> relevant_items;

		static BPRMF_ItemMapping bprmf_map             = new BPRMF_ItemMapping();
		static BPRMF_ItemMapping_Optimal bprmf_map_bpr = new BPRMF_ItemMapping_Optimal();
		static BPRMF_ItemMapping bprmf_map_com         = new BPRMF_ItemMapping_Complex();
		static BPRMF_ItemMapping bprmf_map_knn         = new BPRMF_ItemMapping_kNN();
		static BPRMF_ItemMapping bprmf_map_svr         = new BPRMF_ItemMapping_SVR();
		static BPRMF_Mapping bprmf_user_map            = new BPRMF_UserMapping();
		static BPRMF_Mapping bprmf_user_map_bpr        = new BPRMF_UserMapping_Optimal();

		public static void Usage(string message)
		{
			Console.Error.WriteLine(message);
			Usage(-1);
		}

		public static void Usage(int exit_code)
		{
			Console.WriteLine("MyMedia attribute mapping for item prediction; usage:");
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
				MyMediaLite.util.Random.InitInstance(random_seed);

			// main data files and method
			string trainfile = args[0].Equals("-") ? "-" : Path.Combine(data_dir, args[0]);
			string testfile  = args[1].Equals("-") ? "-" : Path.Combine(data_dir, args[1]);
			string load_model_file = args[2];
			string method    = args[3];

			// set correct recommender
			BPRMF_Mapping recommender = null;
			switch (method)
			{   // TODO shorter names
				case "BPR-MF-ItemMapping":
					recommender = InitBPR_MF_ItemMapping(bprmf_map, parameters);
					break;
				case "BPR-MF-ItemMapping-Optimal":
					recommender = InitBPR_MF_ItemMapping(bprmf_map_bpr, parameters);
					break;
				case "BPR-MF-ItemMapping-Complex":
					recommender = InitBPR_MF_ItemMapping(bprmf_map_com, parameters);
					break;
				case "BPR-MF-ItemMapping-kNN":
					recommender = InitBPR_MF_ItemMapping(bprmf_map_knn, parameters);
					break;
				case "BPR-MF-ItemMapping-SVR":
					recommender = InitBPR_MF_ItemMapping(bprmf_map_svr, parameters);
					break;
				case "BPR-MF-UserMapping":
					recommender = InitBPR_MF_UserMapping(bprmf_user_map, parameters);
					break;
				case "BPR-MF-UserMapping-Optimal":
					recommender = InitBPR_MF_UserMapping(bprmf_user_map_bpr, parameters);
					break;
				default:
					Usage(String.Format("Unknown method: '{0}'", method));
					break;
			}

			if (parameters.CheckForLeftovers())
				Usage(-1);

			// ID mapping objects
			EntityMapping user_mapping = new EntityMapping();
			EntityMapping item_mapping = new EntityMapping();

			// training data
			training_data = ItemRecommenderData.Read(Path.Combine(data_dir, trainfile), user_mapping, item_mapping);
			recommender.SetCollaborativeData(training_data.First, training_data.Second);

			// relevant items
			if (! relevant_items_file.Equals(String.Empty) )
				relevant_items = new HashSet<int>(item_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_items_file))));
			else
				relevant_items = training_data.Second.NonEmptyRowIDs;

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
				if (item_attributes_file.Equals(String.Empty))
				{
					Usage("Recommender expects item_attributes.\n");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
					((ItemAttributeAwareRecommender)recommender).ItemAttributes    = attr_data.First;
					((ItemAttributeAwareRecommender)recommender).NumItemAttributes = attr_data.Second;
				}

			// test data
            test_data = ItemRecommenderData.Read( Path.Combine(data_dir, testfile), user_mapping, item_mapping );

			TimeSpan seconds;

			EngineStorage.LoadModel(recommender, data_dir, load_model_file);

			Console.Write(recommender.ToString() + " ");

			if (compute_fit)
			{
				seconds = Utils.MeasureTime( delegate() {
					int num_iter = recommender.num_iter_mapping;
					recommender.num_iter_mapping = 0;
					recommender.LearnAttributeToFactorMapping();
					Console.Error.WriteLine();
					Console.Error.WriteLine("iteration {0} fit {1}", -1, recommender.ComputeFit());

					recommender.num_iter_mapping = 1;
					for (int i = 0; i < num_iter; i++, i++)
					{
						recommender.iterate_mapping();
						Console.Error.WriteLine("iteration {0} fit {1}", i, recommender.ComputeFit());
					}
					recommender.num_iter_mapping = num_iter; // restore
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
				seconds = EvaluateRecommender(recommender, test_data.First, training_data.First);
			Console.WriteLine();
		}

        static TimeSpan EvaluateRecommender(BPRMF_Mapping recommender, SparseBooleanMatrix test_user_items, SparseBooleanMatrix train_user_items)
		{
			Console.Error.WriteLine("fit {0}", recommender.ComputeFit());

			TimeSpan seconds = Utils.MeasureTime( delegate()
		    	{
		    		var result = ItemPredictionEval.EvaluateItemRecommender(
	                                recommender,
									test_user_items,
            	                    train_user_items,
                	                relevant_items
				    );
					DisplayResults(result);
		    	} );
			Console.Write(" testing " + seconds);

			return seconds;
		}

		static BPRMF_ItemMapping InitBPR_MF_ItemMapping(BPRMF_ItemMapping engine, CommandLineParameters parameters)
		{
			engine.init_f_mean          = parameters.GetRemoveDouble("init_f_mean",          engine.init_f_mean);
			engine.init_f_stdev         = parameters.GetRemoveDouble("init_f_stdev",         engine.init_f_stdev);
			engine.reg_mapping          = parameters.GetRemoveDouble("reg_mapping",          engine.reg_mapping);
			engine.learn_rate_mapping   = parameters.GetRemoveDouble("learn_rate_mapping",   engine.learn_rate_mapping);
			engine.num_iter_mapping     = parameters.GetRemoveInt32( "num_iter_mapping",     engine.num_iter_mapping);
			engine.mapping_feature_bias = parameters.GetRemoveBool(  "mapping_feature_bias", engine.mapping_feature_bias);

			if (engine is BPRMF_ItemMapping_Complex)
			{
				var map_engine                 = (BPRMF_ItemMapping_Complex)engine;
				map_engine.num_hidden_features = parameters.GetRemoveInt32("num_hidden_features", map_engine.num_hidden_features);
			}
			if (engine is BPRMF_ItemMapping_kNN)
			{
				var map_engine = (BPRMF_ItemMapping_kNN)engine;
				map_engine.k   = parameters.GetRemoveUInt32("k", map_engine.k);
			}
			if (engine is BPRMF_ItemMapping_SVR)
			{
				var map_engine     = (BPRMF_ItemMapping_SVR)engine;
				map_engine.C       = parameters.GetRemoveDouble("c",     map_engine.C);
				map_engine.Gamma   = parameters.GetRemoveDouble("gamma", map_engine.Gamma);
			}
			return engine;
		}

		static BPRMF_Mapping InitBPR_MF_UserMapping(BPRMF_Mapping engine, CommandLineParameters parameters)
		{
			engine.init_f_mean        = parameters.GetRemoveDouble("init_f_mean",        engine.init_f_mean);
			engine.init_f_stdev       = parameters.GetRemoveDouble("init_f_stdev",       engine.init_f_stdev);
			engine.reg_mapping        = parameters.GetRemoveDouble("reg_mapping",        engine.reg_mapping);
			engine.learn_rate_mapping = parameters.GetRemoveDouble("learn_rate_mapping", engine.learn_rate_mapping);
			engine.num_iter_mapping   = parameters.GetRemoveInt32( "num_iter_mapping",   engine.num_iter_mapping);
			engine.reg_mapping        = parameters.GetRemoveDouble("reg_mapping",        engine.reg_mapping);
			engine.learn_rate_mapping = parameters.GetRemoveDouble("learn_rate_mapping", engine.learn_rate_mapping);
			engine.num_iter_mapping   = parameters.GetRemoveInt32( "num_iter_mapping",   engine.num_iter_mapping);
			return engine;
		}

		static void DisplayResults(Dictionary<string, double> result) {
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			Console.Write("AUC {0} prec@5 {1} prec@10 {2} MAP {3} NDCG {4}",
			              result["AUC"].ToString(ni),
			              result["prec@5"].ToString(ni),
			              result["prec@10"].ToString(ni),
			              result["MAP"].ToString(ni),
			              result["NDCG"].ToString(ni));
		}

		static void DisplayDataStats()
		{
			NumberFormatInfo ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			// training data stats
			int num_users = training_data.First.NonEmptyRowIDs.Count;
			int num_items = training_data.Second.NonEmptyRowIDs.Count;
			int matrix_size = num_users * num_items;
			int empty_size  = matrix_size - training_data.First.NumberOfEntries;
			double sparsity = (double) 100 * empty_size / matrix_size;
			Console.WriteLine(string.Format(ni, "training data: {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));

			// test data stats
			num_users = test_data.First.NonEmptyRowIDs.Count;
			num_items = test_data.Second.NonEmptyRowIDs.Count;
			matrix_size = num_users * num_items;
			empty_size  = matrix_size - test_data.First.NumberOfEntries;
			sparsity = (double) 100 * empty_size / matrix_size;
			Console.WriteLine(string.Format(ni, "test data:     {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));
		}
	}
}
