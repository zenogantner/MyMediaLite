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
	// TODO catch FileNotFoundException
	// TODO predict_items=true option

	/// <summary>Item prediction program, see Usage() method for more information</summary>
	public class ItemPrediction
	{
		static ItemRecommender recommender = null;
		static Pair<SparseBooleanMatrix, SparseBooleanMatrix> training_data;

		static HashSet<int> relevant_items;

		// recommender engines
		static KNN         iknn   = new ItemKNN();
		static KNN         iaknn  = new ItemAttributeKNN();
		static KNN         uknn   = new UserKNN();
		static KNN         wuknn  = new WeightedUserKNN();
		static KNN         uaknn  = new UserAttributeKNN();
		static MostPopular mp     = new MostPopular();
		static WRMF        wrmf   = new WRMF();
		static BPRMF       bprmf  = new BPRMF();
		static item_recommender.Random random = new item_recommender.Random();
		static BPR_Linear  bpr_linear = new BPR_Linear();

		public static void Usage(string message)
		{
			Console.Error.WriteLine(message);
			Usage(-1);
		}

		public static void Usage(int exit_code)
		{
			Console.WriteLine("MyMedia item prediction; usage:");
			Console.WriteLine(" ItemPrediction.exe TRAINING_FILE TEST_FILE METHOD [ARGUMENTS] [OPTIONS]");
			Console.WriteLine("    - use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN");
			Console.WriteLine("  - methods (plus arguments and their defaults):");
			Console.WriteLine("    - " + wrmf);
			Console.WriteLine("    - " + bprmf);
			Console.WriteLine("    - " + bpr_linear + " (needs item_attributes)");
			Console.WriteLine("    - " + iknn);
			Console.WriteLine("    - " + iaknn + " (needs item_attributes)");
			Console.WriteLine("    - " + uknn);
			Console.WriteLine("    - " + wuknn);
			Console.WriteLine("    - " + uaknn + " (needs user_attributes)");
			Console.WriteLine("    - " + mp);
			Console.WriteLine("    - " + random);
			Console.WriteLine("  - method ARGUMENTS have the form name=value");
			Console.WriteLine("  - general OPTIONS have the form name=value");
			Console.WriteLine("    - option_file=FILE           read options from FILE (line format KEY: VALUE)");
			Console.WriteLine("    - random_seed=N");
			Console.WriteLine("    - data_dir=DIR               load all files from DIR");
			Console.WriteLine("    - relevant_items=FILE        use only item in the given file for evaluation");
			Console.WriteLine("    - item_attributes=FILE       file containing item attribute information");
			Console.WriteLine("    - save_model=FILE            save computed model to FILE");
			Console.WriteLine("    - load_model=FILE            load model from FILE");
			Console.WriteLine("    - no_eval=BOOL               do not evaluate");
			Console.WriteLine("    - eval_new_users=BOOL        also evaluate users not present in the training data");
			Console.WriteLine("    - predict_items_file=FILE    write predictions to FILE");
			Console.WriteLine("    - predict_items_num=N        predict N items per user (needs predict_items_file)");
			Console.WriteLine("    - predict_for_users=FILE     predict items for users specified in FILE (needs predict_items_file)");
			Console.WriteLine("  - options for finding the right number of iterations (MF methods)");
			Console.WriteLine("    - find_iter=STEP");
			Console.WriteLine("    - max_iter=N");
			Console.WriteLine("    - auc_cutoff=F");
			Console.WriteLine("    - prec_cutoff=F");
			Console.WriteLine("    - compute_fit=BOOL");
			/*
			Console.WriteLine("  - options for hyperparameter search:");
			Console.WriteLine("    - hyper_split=N              number of folds used in cross-validation");
			Console.WriteLine("    - hyper_criterion=NAME       values {0}", String.Join( ", ", Evaluate.GetItemPredictionMeasures().ToArray()));
			Console.WriteLine("    - hyper_half_size=N          number of values tried in each iteration/2");
			*/
			Environment.Exit(exit_code);
		}

        public static void Main(string[] args)
        {
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(util.Handlers.UnhandledExceptionHandler);

			// check number of command line parameters
			if (args.Length < 3)
				Usage("Not enough arguments.");

			// read command line parameters
			CommandLineParameters parameters = null;
			try	{ parameters = new CommandLineParameters(args, 3);	}
			catch (ArgumentException e)	{ Usage(e.Message);  		}

			// variables for iteration search
			int find_iter                 = parameters.GetRemoveInt32(  "find_iter", 0);
			int max_iter                  = parameters.GetRemoveInt32(  "max_iter", 500);
			double auc_cutoff             = parameters.GetRemoveDouble( "auc_cutoff");
			double prec_cutoff            = parameters.GetRemoveDouble( "prec_cutoff");
			bool compute_fit              = parameters.GetRemoveBool(   "compute_fit", false);

			// other parameters
			string data_dir               = parameters.GetRemoveString( "data_dir");
			string relevant_items_file    = parameters.GetRemoveString( "relevant_items");
			string item_attributes_file   = parameters.GetRemoveString( "item_attributes");
			string user_attributes_file   = parameters.GetRemoveString( "user_attributes");
			string save_model_file        = parameters.GetRemoveString( "save_model");
			string load_model_file        = parameters.GetRemoveString( "load_model");
			int random_seed               = parameters.GetRemoveInt32(  "random_seed", -1);
			bool no_eval                  = parameters.GetRemoveBool(   "no_eval", false);
			bool eval_new_users           = parameters.GetRemoveBool(   "eval_new_users", false);
			string predict_items_file     = parameters.GetRemoveString( "predict_items_file", String.Empty);
			int predict_items_number      = parameters.GetRemoveInt32(  "predict_items_num", -1);
			string predict_for_users_file = parameters.GetRemoveString( "predict_for_users", String.Empty);

			// TODO specific model file directory, specific prediction file directory

			// variables for hyperparameter search via crossvalidation
			/*
			int  hyper_split            = parameters.GetRemoveInt32(  "hyper_split", -1);
			string hyper_criterion      = parameters.GetRemoveString( "hyper_criterion", "prec@5"); // TODO AUC must be maximized - take care of that!!
			     half_size              = parameters.GetRemoveUInt32( "half_size", 2); // TODO nicer name?
				 reg_base               = parameters.GetRemoveUInt32( "reg_base", 2);  // TODO nicer name?
			*/

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
					InitWRMF(wrmf, parameters);
					break;

			case "BPR-MF":
				case "bpr-mf":
					InitBPRMF(bprmf, parameters);
					break;
				case "BPR-Linear":
				case "bpr-linear":
					InitBPR_Linear(bpr_linear, parameters);
					break;
				case "item-knn":
			    case "item-kNN":
				case "item-KNN":
					InitKNN(iknn, parameters);
					break;
				case "item-attribute-knn":
				case "item-attribute-kNN":
				case "item-attribute-KNN":
					InitKNN(iaknn, parameters);
					break;
				case "user-knn":
				case "user-kNN":
				case "user-KNN":
					InitKNN(uknn, parameters);
					break;
				case "weighted-user-knn":
				case "weighted-user-kNN":
				case "weighted-user-KNN":
					InitKNN(wuknn, parameters);
					break;
				case "user-attribute-knn":
				case "user-attribute-kNN":
				case "user-attribute-KNN":
					InitKNN(uaknn, parameters);
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
			if (parameters.CheckForLeftovers())
				Usage(-1);
			if (trainfile.Equals("-") && testfile.Equals("-"))
			{
				Console.Out.WriteLine("Either training OR test data, not both, can be read from STDIN.");
				Usage(-1);
			}

			// ID mapping objects
			EntityMapping user_mapping = new EntityMapping();
			EntityMapping item_mapping = new EntityMapping();

			// training data
			training_data   = ItemRecommenderData.Read(trainfile, user_mapping, item_mapping);

			// relevant items
			if (! relevant_items_file.Equals(String.Empty) )
				relevant_items = new HashSet<int>(item_mapping.ToInternalID(Utils.ReadIntegers(Path.Combine(data_dir, relevant_items_file))));
			else
				relevant_items = training_data.Second.GetNonEmptyRowIDs();

			if (recommender != random)
				((Memory)recommender).SetCollaborativeData(training_data.First, training_data.Second);

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
            Pair<SparseBooleanMatrix, SparseBooleanMatrix> test_data = ItemRecommenderData.Read(testfile, user_mapping, item_mapping );

			TimeSpan time_span;

			/*
			if (hyper_split != -1)
			{
				time_span = Utils.MeasureTime( delegate() {
					if (recommender is BPR_Linear)
						FindGoodHyperparameters(recommender as BPR_Linear, hyper_split, hyper_criterion);
					else
						throw new NotImplementedException();
				});
				Console.Write("hyperparameters {0} ", time_span);
			}
			*/

			// TODO give out time for each iteration
			if (find_iter != 0)
			{
				IterativeModel iterative_recommender = (IterativeModel) recommender;
				Console.WriteLine(recommender.ToString() + " ");

				if (load_model_file.Equals(String.Empty))
					iterative_recommender.Train();
				else
					EngineStorage.LoadModel(iterative_recommender, data_dir, load_model_file);

				if (compute_fit)
					Console.Write("fit {0,0:0.#####} ", iterative_recommender.ComputeFit());

				var result = ItemRankingEval.EvaluateItemRecommender(recommender,
				                                 test_data.First,
					                             training_data.First,
					                             relevant_items,
				                                 !eval_new_users);
				DisplayResults(result);
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
							result = ItemRankingEval.EvaluateItemRecommender(
								recommender,
							    test_data.First,
								training_data.First,
								relevant_items,
							    !eval_new_users
							);
							DisplayResults(result);
							Console.WriteLine(" " + i);
						});
						eval_time_stats.Add(t.TotalSeconds);

						EngineStorage.SaveModel(recommender, data_dir, save_model_file, i);



						if (result["AUC"] < auc_cutoff || result["prec@5"] < prec_cutoff)
						{
								Console.Error.WriteLine("Reached cutoff after {0} iterations.", i);
								Console.Error.WriteLine("DONE");
								break;
						}
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
				Console.Error.Flush();
			}
			else
			{
				if (load_model_file.Equals(String.Empty))
				{
					Console.Write(recommender.ToString() + " ");
					time_span = Utils.MeasureTime( delegate() { recommender.Train(); } );
            		Console.Write("training_time " + time_span + " ");
				}
				else
				{
					EngineStorage.LoadModel(recommender, data_dir, load_model_file);
					Console.Write(recommender.ToString() + " ");
					// TODO is this the right time to load the model?
				}

				if (!predict_items_file.Equals(String.Empty))
				{

					if (predict_for_users_file.Equals(String.Empty))
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
					    	var result = ItemRankingEval.EvaluateItemRecommender(
						    	recommender,
								test_data.First,
					            training_data.First,
					            relevant_items,
						        !eval_new_users
							);
							DisplayResults(result);
				    	}
					);
					Console.Write(" testing_time " + time_span);
				}
				Console.WriteLine();
			}
			EngineStorage.SaveModel(recommender, data_dir, save_model_file);
		}

		// undo the void thing ...
		static void InitWRMF(WRMF engine, CommandLineParameters parameters)
		{
			engine.NumIter       = parameters.GetRemoveInt32( "num_iter",        engine.NumIter);
			engine.num_features   = parameters.GetRemoveInt32( "num_features",   engine.num_features);
   			engine.init_f_mean    = parameters.GetRemoveDouble("init_f_mean",    engine.init_f_mean);
   			engine.init_f_stdev   = parameters.GetRemoveDouble("init_f_stdev",   engine.init_f_stdev);
			engine.regularization = parameters.GetRemoveDouble("reg",            engine.regularization);
			engine.regularization = parameters.GetRemoveDouble("regularization", engine.regularization);
			engine.c_pos          = parameters.GetRemoveDouble("c_pos",          engine.c_pos);

			recommender = engine;
		}

		static void InitBPRMF(BPRMF engine, CommandLineParameters parameters)
		{
			engine.NumIter     = parameters.GetRemoveInt32( "num_iter",     engine.NumIter);
			engine.num_features = parameters.GetRemoveInt32( "num_features", engine.num_features);
			engine.init_f_mean  = parameters.GetRemoveDouble("init_f_mean",  engine.init_f_mean);
			engine.init_f_stdev = parameters.GetRemoveDouble("init_f_stdev", engine.init_f_stdev);
			engine.reg_u        = parameters.GetRemoveDouble("reg",   engine.reg_u);
			engine.reg_i        = parameters.GetRemoveDouble("reg",   engine.reg_i);
			engine.reg_j        = parameters.GetRemoveDouble("reg",   engine.reg_j);
			engine.reg_u        = parameters.GetRemoveDouble("reg_u", engine.reg_u);
			engine.reg_i        = parameters.GetRemoveDouble("reg_i", engine.reg_i);
			engine.reg_j        = parameters.GetRemoveDouble("reg_j", engine.reg_j);
			engine.learn_rate   = parameters.GetRemoveDouble("lr",         engine.learn_rate);
			engine.learn_rate   = parameters.GetRemoveDouble("learn_rate", engine.learn_rate);
			engine.fast_sampling_memory_limit = parameters.GetRemoveInt32( "fast_sampling_memory_limit", engine.fast_sampling_memory_limit);
			engine.item_bias    = parameters.GetRemoveBool(  "item_bias", engine.item_bias);

			recommender = engine;
		}

		static void InitBPR_Linear(BPR_Linear engine, CommandLineParameters parameters)
		{
			engine.NumIter     = parameters.GetRemoveInt32( "num_iter",     engine.NumIter);
			engine.init_f_mean  = parameters.GetRemoveDouble("init_f_mean",  engine.init_f_mean);
			engine.init_f_stdev = parameters.GetRemoveDouble("init_f_stdev", engine.init_f_stdev);
			engine.reg          = parameters.GetRemoveDouble("reg",   engine.reg);
			engine.learn_rate   = parameters.GetRemoveDouble("lr",         engine.learn_rate);
			engine.learn_rate   = parameters.GetRemoveDouble("learn_rate", engine.learn_rate);
			engine.fast_sampling_memory_limit = parameters.GetRemoveInt32( "fast_sampling_memory_limit", engine.fast_sampling_memory_limit);

			recommender = engine;
		}

		static void InitKNN(KNN engine, CommandLineParameters parameters)
		{
			engine.k = parameters.GetRemoveUInt32( "k", engine.k); // TODO handle "inf"

			recommender = engine;
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
		
		/*
        static void FindGoodHyperparameters(BPR_Linear recommender, int cross_validation, string criterion)
		{
			Console.Error.WriteLine();
			Console.Error.WriteLine("Hyperparameter search ...");

			double step = 1.0;

			double center = 0;
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
		 */

		/*
		static double FindGoodHyperparameters(BPR_Linear engine, uint half_size, double center, double step_size, int cross_validation, string criterion)  // TODO make the type of split configurable
		{
			IEntityRelationDataProvider backend = engine.EntityRelationDataProvider; // save for later use
			CrossvalidationSplit split = new CrossvalidationSplit((WP2Backend)backend, cross_validation, true); // TODO make configurable
			// TODO: the split should only be created once, not on every call of the function

			double best_log_reg = 0;
			double best_q       = double.MinValue;

            double[] log_reg = new double[2 * half_size + 1];

            log_reg[half_size] = center;
	        for (int i = 0; i <= half_size; i++) {
      	        log_reg[half_size - i] = center - step_size * i;
                log_reg[half_size + i] = center + step_size * i;
            }

			foreach (double exp in log_reg)
			{
				double reg = Math.Pow(reg_base, exp);
				engine.reg = reg;
				IList<double> quality = new List<double>();
				for (int j = 0; j < cross_validation; j++)
				{
					engine.EntityRelationDataProvider = split.GetTrainingSet(j);

					engine.Train();

					var eval_result = Evaluate.EvaluateItemRecommender(
                    	engine,
						split.GetTestSet(j),
        	            split.GetTrainingSet(j).GetRelation(RelationType.Viewed),
            	        relevant_items
				        // split.GetRelevantItems(j),
                        // TODO !eval_new_users
			    	);

					if (!eval_result.ContainsKey(criterion))
						throw new ArgumentException(string.Format("Unknown criterion {0}, valid criteria are {1}",
						                            	          criterion, String.Join( ", ", Evaluate.GetItemPredictionMeasures().ToArray() ) ));
					quality.Add(eval_result[criterion]);
					Console.Error.Write(".");
				}

				if (quality.Average() > best_q)
				{
					best_q = quality.Average();
					best_log_reg = exp;
				}

				Console.Error.WriteLine("reg={0}, {1}=({2}, {3}, {4})", reg, criterion, quality.Min(), quality.Average(), quality.Max());
			} //foreach

			engine.reg = Math.Pow(reg_base, best_log_reg);
			engine.EntityRelationDataProvider = backend; // reset
			Console.Error.WriteLine();
			return best_log_reg;
		}
		*/
	}
}
