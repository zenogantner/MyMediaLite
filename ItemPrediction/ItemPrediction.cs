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
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.eval;
using MyMediaLite.io;
using MyMediaLite.item_recommender;
using MyMediaLite.util;


namespace MyMediaLite
{
	/// <summary>Item prediction program, see Usage() method for more information</summary>
	public class ItemPrediction
	{
		static Pair<SparseBooleanMatrix, SparseBooleanMatrix> training_data;
		static Pair<SparseBooleanMatrix, SparseBooleanMatrix> test_data;
		static ICollection<int> relevant_items;

		static NumberFormatInfo ni = new NumberFormatInfo();

		// recommender engines
		static ItemRecommender recommender = null;
		static KNN                     iknn       = new ItemKNN();
		static KNN                     iaknn      = new ItemAttributeKNN();
		static KNN                     uknn       = new UserKNN();
		static KNN                     wuknn      = new WeightedUserKNN();
		static KNN                     uaknn      = new UserAttributeKNN();
		static MostPopular             mp         = new MostPopular();
		static WRMF                    wrmf       = new WRMF();
		static BPRMF                   bprmf      = new BPRMF();
		static item_recommender.Random random     = new item_recommender.Random();
		static BPR_Linear              bpr_linear = new BPR_Linear();
		static ItemAttributeSVM        svm        = new ItemAttributeSVM();

		static bool compute_fit;

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
			Console.WriteLine("MyMediaLite item prediction; usage:");
			Console.WriteLine(" ItemPrediction.exe TRAINING_FILE TEST_FILE METHOD [ARGUMENTS] [OPTIONS]");
			Console.WriteLine("    - use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN");
			Console.WriteLine("  - methods (plus arguments and their defaults):");
			Console.WriteLine("    - " + wrmf);
			Console.WriteLine("    - " + bprmf);
			Console.WriteLine("    - " + bpr_linear + " (needs item_attributes=FILE)");
			Console.WriteLine("    - " + iknn);
			Console.WriteLine("    - " + iaknn      + " (needs item_attributes=FILE)");
			Console.WriteLine("    - " + uknn);
			Console.WriteLine("    - " + wuknn);
			Console.WriteLine("    - " + uaknn      + " (needs user_attributes=FILE)");
			Console.WriteLine("    - " + svm        + " (needs item_attributes=FILE)");
			Console.WriteLine("    - " + mp);
			Console.WriteLine("    - " + random);
			Console.WriteLine("  - method ARGUMENTS have the form name=value");
			Console.WriteLine("  - general OPTIONS have the form name=value");
			Console.WriteLine("    - option_file=FILE           read options from FILE (line format KEY: VALUE)");
			Console.WriteLine("    - random_seed=N");
			Console.WriteLine("    - data_dir=DIR               load all files from DIR");
			Console.WriteLine("    - relevant_items=FILE        use the items in FILE for evaluation, otherwise all items that occur in the training set");
			Console.WriteLine("    - user_attributes=FILE       file containing user attribute information");
			Console.WriteLine("    - item_attributes=FILE       file containing item attribute information");
			Console.WriteLine("    - user_relation=FILE         file containing user relation information");
			Console.WriteLine("    - item_relation=FILE         file containing item relation information");
			Console.WriteLine("    - save_model=FILE            save computed model to FILE");
			Console.WriteLine("    - load_model=FILE            load model from FILE");
			Console.WriteLine("    - no_eval=BOOL               do not evaluate");
			Console.WriteLine("    - predict_items_file=FILE    write predictions to FILE ('-' for STDOUT)");
			Console.WriteLine("    - predict_items_num=N        predict N items per user (needs predict_items_file)");
			Console.WriteLine("    - predict_for_users=FILE     predict items for users specified in FILE (needs predict_items_file)");
			Console.WriteLine("  - options for finding the right number of iterations (MF methods and BPR-Linear)");
			Console.WriteLine("    - find_iter=N                give out statistics every N iterations");
			Console.WriteLine("    - max_iter=N                 perform at most N iterations");
			Console.WriteLine("    - auc_cutoff=NUM             abort if AUC is below NUM");
			Console.WriteLine("    - prec5_cutoff=NUM           abort if prec@5 is below NUM");
			Console.WriteLine("    - compute_fit=BOOL           display fit on training data every find_iter iterations");
			Environment.Exit(exit_code);
		}

        public static void Main(string[] args)
        {
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(util.Handlers.UnhandledExceptionHandler);
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
			string predict_items_file     = parameters.GetRemoveString( "predict_items_file", string.Empty);
			int predict_items_number      = parameters.GetRemoveInt32(  "predict_items_num", -1);
			string predict_for_users_file = parameters.GetRemoveString( "predict_for_users", string.Empty);

			// main data files and method
			string trainfile = args[0].Equals("-") ? "-" : Path.Combine(data_dir, args[0]);
			string testfile  = args[1].Equals("-") ? "-" : Path.Combine(data_dir, args[1]);
			string method    = args[2];

			if (random_seed != -1)
				util.Random.InitInstance(random_seed);

			// set requested recommender
			switch (method)
			{
				case "WR-MF":
				case "wr-mf":
					compute_fit = false; // deactivate as long it is not implemented
					recommender = Engine.Configure(wrmf, parameters, Usage);
					break;

                case "BPR-MF":
				case "bpr-mf":
					recommender = Engine.Configure(bprmf, parameters, Usage);
					break;
				case "BPR-Linear":
				case "bpr-linear":
					recommender = Engine.Configure(bpr_linear, parameters, Usage);
					break;
				case "item-knn":
			    case "item-kNN":
				case "item-KNN":
					recommender = Engine.Configure(iknn, parameters, Usage);
					break;
				case "item-attribute-knn":
				case "item-attribute-kNN":
				case "item-attribute-KNN":
					recommender = Engine.Configure(iaknn, parameters, Usage);
					break;
				case "user-knn":
				case "user-kNN":
				case "user-KNN":
					recommender = Engine.Configure(uknn, parameters, Usage);
					break;
				case "weighted-user-knn":
				case "weighted-user-kNN":
				case "weighted-user-KNN":
					recommender = Engine.Configure(wuknn, parameters, Usage);
					break;
				case "user-attribute-knn":
				case "user-attribute-kNN":
				case "user-attribute-KNN":
					recommender = Engine.Configure(uaknn, parameters, Usage);
					break;
				case "item-attribute-svm":
				case "item-attribute-SVM":
					recommender = svm;
					break;
				case "most-popular":
					recommender = mp;
					break;
				case "random":
					recommender = random;
					break;
				default:
					Console.WriteLine("Unknown method: '{0}'", method);
					Usage(-1);
					break;
			}

			// check command-line parameters
			if (trainfile.Equals("-") && testfile.Equals("-"))
				Usage("Either training OR test data, not both, can be read from STDIN.");

			// ID mapping objects
			EntityMapping user_mapping = new EntityMapping();
			EntityMapping item_mapping = new EntityMapping();

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
					Engine.LoadModel(iterative_recommender, data_dir, load_model_file);

				if (compute_fit)
					Console.Write(string.Format(ni, "fit {0,0:0.#####} ", iterative_recommender.ComputeFit()));

				var result = ItemPredictionEval.EvaluateItemRecommender(recommender,
				                                 test_data.First,
					                             training_data.First,
					                             relevant_items);
				DisplayResults(result);
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
							result = ItemPredictionEval.EvaluateItemRecommender(
								recommender,
							    test_data.First,
								training_data.First,
								relevant_items
							);
							DisplayResults(result);
							Console.WriteLine(" " + i);
						});
						eval_time_stats.Add(t.TotalSeconds);

						Engine.SaveModel(recommender, data_dir, save_model_file, i);

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
					Engine.LoadModel(recommender, data_dir, load_model_file);
					Console.Write(recommender.ToString() + " ");
					// TODO is this the right time to load the model?
				}

				if (!predict_items_file.Equals(string.Empty))
				{
					if (predict_for_users_file.Equals(string.Empty))
						time_span = Utils.MeasureTime( delegate()
					    	{
						    	eval.ItemPrediction.WritePredictions(
							    	recommender,
							        training_data.First,
							        relevant_items, predict_items_number,
							        user_mapping, item_mapping,
							        predict_items_file
								);
								Console.Error.WriteLine("Wrote predictions to {0}", predict_items_file);
					    	}
						);
					else
						time_span = Utils.MeasureTime( delegate()
					    	{
						    	eval.ItemPrediction.WritePredictions(
							    	recommender,
							        training_data.First,
							        user_mapping.ToInternalID(Utils.ReadIntegers(predict_for_users_file)),
							        relevant_items, predict_items_number,
							        user_mapping, item_mapping,
							        predict_items_file
								);
								Console.Error.WriteLine("Wrote predictions for selected users to {0}", predict_items_file);
					    	}
						);
					Console.Write(" predicting_time " + time_span);
				}
				else if (!no_eval)
				{
					time_span = Utils.MeasureTime( delegate()
				    	{
					    	var result = ItemPredictionEval.EvaluateItemRecommender(
						    	recommender,
								test_data.First,
					            training_data.First,
					            relevant_items
							);
							DisplayResults(result);
				    	}
					);
					Console.Write(" testing_time " + time_span);
				}
				Console.WriteLine();
			}
			Engine.SaveModel(recommender, data_dir, save_model_file);
		}

        static void LoadData(string data_dir, string trainfile, string testfile,
		                     EntityMapping user_mapping, EntityMapping item_mapping,
		                     string relevant_items_file,
		                     string user_attributes_file, string item_attributes_file,
		                     string user_relation_file, string item_relation_file)
		{
			// training data
			training_data = ItemRecommenderData.Read(trainfile, user_mapping, item_mapping);

			// relevant items
			if (! relevant_items_file.Equals(string.Empty) )
				relevant_items = new HashSet<int>(item_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_items_file))));
			else
				relevant_items = training_data.Second.NonEmptyRowIDs;

			if (recommender != random)
				((Memory)recommender).SetCollaborativeData(training_data.First, training_data.Second);

			// user attributes
			if (recommender is IUserAttributeAwareRecommender)
				if (user_attributes_file.Equals(string.Empty))
				{
					Usage("Recommender expects user_attributes=FILE.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
					((IUserAttributeAwareRecommender)recommender).UserAttributes    = attr_data.First;
					((IUserAttributeAwareRecommender)recommender).NumUserAttributes = attr_data.Second;
					Console.WriteLine("{0} user attributes", attr_data.Second);
				}

			// item attributes
			if (recommender is IItemAttributeAwareRecommender)
				if (item_attributes_file.Equals(string.Empty))
				{
					Usage("Recommender expects item_attributes=FILE.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
					((IItemAttributeAwareRecommender)recommender).ItemAttributes    = attr_data.First;
					((IItemAttributeAwareRecommender)recommender).NumItemAttributes = attr_data.Second;
					Console.WriteLine("{0} item attributes", attr_data.Second);
				}

			// user relation
			if (recommender is IUserRelationAwareRecommender)
				if (user_relation_file.Equals(string.Empty))
				{
					Usage("Recommender expects user_relation=FILE.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> relation_data = RelationData.Read(Path.Combine(data_dir, user_relation_file), user_mapping);
					((IUserRelationAwareRecommender)recommender).UserRelation = relation_data.First;
					((IUserRelationAwareRecommender)recommender).NumUsers     = relation_data.Second;
					Console.WriteLine("relation over {0} users", relation_data.Second);
				}

			// item relation
			if (recommender is IItemRelationAwareRecommender)
				if (user_relation_file.Equals(string.Empty))
				{
					Usage("Recommender expects item_relation=FILE.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> relation_data = RelationData.Read(Path.Combine(data_dir, item_relation_file), item_mapping);
					((IItemRelationAwareRecommender)recommender).ItemRelation = relation_data.First;
					((IItemRelationAwareRecommender)recommender).NumItems     = relation_data.Second;
					Console.WriteLine("relation over {0} items", relation_data.Second);
				}

			// test data
	        test_data = ItemRecommenderData.Read(testfile, user_mapping, item_mapping );
		}

		static void AbortHandler(object sender, ConsoleCancelEventArgs args)
		{
			DisplayIterationStats();
		}

		static void DisplayResults(Dictionary<string, double> result) {
			Console.Write(string.Format(ni, "AUC {0,0:0.#####} prec@5 {1,0:0.#####} prec@10 {2,0:0.#####} MAP {3,0:0.#####} NDCG {4,0:0.#####} num_users {5} num_items {6}",
			                            result["AUC"], result["prec@5"], result["prec@10"], result["MAP"], result["NDCG"], result["num_users"], result["num_items"]));
		}

		static void DisplayDataStats()
		{
			// training data stats
			int num_users = training_data.First.NonEmptyRowIDs.Count;
			int num_items = training_data.Second.NonEmptyRowIDs.Count;
			long matrix_size = (long) num_users * num_items;
			long empty_size  = (long) matrix_size - training_data.First.NumberOfEntries;
			double sparsity = (double) 100L * empty_size / matrix_size;
			Console.WriteLine(string.Format(ni, "training data: {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));

			// test data stats
			num_users = test_data.First.NonEmptyRowIDs.Count;
			num_items = test_data.Second.NonEmptyRowIDs.Count;
			matrix_size = (long) num_users * num_items;
			empty_size  = (long) matrix_size - test_data.First.NumberOfEntries;
			sparsity = (double) 100L * empty_size / matrix_size;
			Console.WriteLine(string.Format(ni, "test data:     {0} users, {1} items, sparsity {2,0:0.#####}", num_users, num_items, sparsity));
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
}
