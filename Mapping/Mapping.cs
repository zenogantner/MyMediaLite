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
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public class Mapping
	{
		static HashSet<int> relevant_items;
		static bool eval_new_users;
		/*
		static uint half_size;
		static uint reg_base = 2;
		*/

		// TODO ItemRecommenderMF_Mapping<MF_Engine> ...
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
			Console.WriteLine("    - eval_new_users=BOOL    also evaluate users not present in the training data");
			Console.WriteLine("    - compute_fit=N          compute fit every N iterations");
			/*
			Console.WriteLine("  - options for hyperparameter search:");
			Console.WriteLine("    - cross_validation=N     ");
			Console.WriteLine("    - half_size=N            ");
			*/

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
			     eval_new_users         = parameters.GetRemoveBool(   "eval_new_users", false);
			/*
			int  cross_validation       = parameters.GetRemoveInt32(  "cross_validation", -1);
			     half_size              = parameters.GetRemoveUInt32( "half_size", 2);
			*/
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
			Pair<SparseBooleanMatrix, SparseBooleanMatrix> training_data = ItemRecommenderData.Read(Path.Combine(data_dir, trainfile), user_mapping, item_mapping);
			recommender.SetCollaborativeData(training_data.First, training_data.Second);

			// relevant items
			if (! relevant_items_file.Equals(String.Empty) )
				relevant_items = new HashSet<int>( Utils.ReadIntegers( Path.Combine(data_dir, relevant_items_file) ) );
			else
				relevant_items = training_data.Second.GetNonEmptyRowIDs();

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
            Pair<SparseBooleanMatrix, SparseBooleanMatrix> test_data = ItemRecommenderData.Read( Path.Combine(data_dir, testfile), user_mapping, item_mapping );

			TimeSpan seconds;

			EngineStorage.LoadModel(recommender, data_dir, load_model_file);

			/*
			if (cross_validation != -1)
			{
				seconds = ISMLL.Utils.MeasureTime( delegate() {
					// TODO handle via multiple dispatch
					if (recommender is BPRMF_ItemMapping_kNN)
					{
						FindGoodHyperparameters(recommender as BPRMF_ItemMapping_kNN, cross_validation);
					}
					else if (recommender is BPRMF_ItemMapping_SVR)
					{
						FindGoodHyperparameters(recommender as BPRMF_ItemMapping_SVR, cross_validation);
					}
					else
					{   // TODO combine those two
						FindGoodLearnRate(recommender);
						FindGoodHyperparameters(recommender, cross_validation);
					}
				});
				Console.Write("hyperparameters {0} ", seconds);
			}
			*/

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
			Console.Write("mapping " + seconds + " ");

			if (!no_eval)
			{
				seconds = EvaluateRecommender(recommender, test_data.First, training_data.First);
			}
			Console.WriteLine();
		}

        static TimeSpan EvaluateRecommender(BPRMF_Mapping recommender, SparseBooleanMatrix test_user_items, SparseBooleanMatrix train_user_items)
		{
			Console.Error.WriteLine("fit {0}", recommender.ComputeFit());

			TimeSpan seconds = Utils.MeasureTime( delegate()
		    	{
		    		var result = ItemRankingEval.EvaluateItemRecommender(
	                                recommender,
									test_user_items,
            	                    train_user_items,
                	                relevant_items,
	                                !eval_new_users
				    );
					Console.Write("AUC {0} prec@5 {1} prec@10 {2}", result["AUC"], result["prec@5"], result["prec@10"]);
		    	} );
			Console.Write(" testing " + seconds);

			return seconds;
		}

		/*
        static void FindGoodHyperparameters(BPRMF_ItemMapping_kNN recommender, int cross_validation)
		{
			Console.Error.WriteLine();
			Console.Error.WriteLine("Hyperparameter search ...");

			// TODO speed-up using some kind of heuristic search
			string criterion = "prec@5"; // TODO make this configurable
			uint[] k_values  = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 50, 60, 70, 80, 200, 400, 800 }; // TODO: make this configurable

			//uint[] k_values  = { 1, 5, 10, 15, 20, 25, 50, 100, 200, 400, 800 }; // TODO make this configurable


			// TODO speed up by not recomputing the cosine similarity every time

			IEntityRelationDataProvider backend = recommender.EntityRelationDataProvider; // save for later use
			CrossvalidationSplit split = new CrossvalidationSplit((WP2Backend)backend, cross_validation, false); // TODO: make the type of split configurable

			uint best_k   = 0;
			double best_q = double.MinValue;

			foreach (var k in k_values)
			{
				recommender.k = k;
				IList<double> quality = new List<double>();
				for (int j = 0; j < cross_validation; j++)
				{
					recommender.EntityRelationDataProvider = split.GetTrainingSet(j);
					recommender.LearnAttributeToFactorMapping();

					Dictionary<string, double> eval_result = Evaluate.EvaluateItemRecommender(
                    	recommender,
						split.GetTestSet(j),
        	            split.GetTrainingSet(j).GetRelation(relationType),
            	        //relevant_items.GetEntity(EntityType.CatalogItem),
						// TODO: do this if there is an overlap, otherwise not
				        split.GetRelevantItems(j),
                        !eval_new_users
			    	);

					double q = eval_result[criterion];
					quality.Add(q);
					Console.Error.Write(".");
				}

				if (quality.Average() > best_q)
				{
					best_q = quality.Average();
					best_k = k;
				}

				Console.Error.WriteLine("k={0}, {1}=({2}, {3}, {4})", k, criterion, quality.Min(), quality.Average(), quality.Max());
			} //foreach

			recommender.k = best_k;
			recommender.EntityRelationDataProvider = backend; // reset
			Console.Error.WriteLine();
		}

		// TODO: generalize and put into helper class
        static void FindGoodHyperparameters(BPRMF_Mapping recommender, int cross_validation)
		{
			Console.Error.WriteLine();
			Console.Error.WriteLine("Hyperparameter search ...");
			//int num_iter_mapping = recommender.num_iter_mapping; // store
			//recommender.num_iter_mapping = 10;

			double step = 1.0;

			double center = 0;
			double new_center;

			double upper_limit = 0; // highest (log) hyperparameter tried so far
			double lower_limit = 0; // lowest (log) hyperparameter tried so far

			while (step > 0.125)
			{
				upper_limit = Math.Max(upper_limit, center + half_size * step);
				lower_limit = Math.Min(lower_limit, center - half_size * step);
				new_center = FindGoodHyperparameters(recommender, half_size, center, step,  cross_validation);
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

		// TODO make it wider ...
		static double FindGoodHyperparameters(BPRMF_Mapping engine, uint half_size, double center, double step_size, int cross_validation)  // TODO: make the type of split configurable
		{
			string criterion = "prec@5";

			IEntityRelationDataProvider backend = engine.EntityRelationDataProvider; // save for later use
			CrossvalidationSplit split = new CrossvalidationSplit((WP2Backend)backend, cross_validation, false);
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
				engine.reg_mapping = reg;
				IList<double> quality = new List<double>();
				for (int j = 0; j < cross_validation; j++)
				{
					engine.EntityRelationDataProvider = split.GetTrainingSet(j);

					engine.LearnAttributeToFactorMapping();

					var eval_result = Evaluate.EvaluateItemRecommender(
                    	engine,
						split.GetTestSet(j),
        	            split.GetTrainingSet(j).GetRelation(relationType),
            	        //relevant_items.GetEntity(EntityType.CatalogItem),
						// TODO: do this if there is an overlap, otherwise not
				        split.GetRelevantItems(j),
                        !eval_new_users
			    	);

					double q = eval_result[criterion];
					quality.Add(q);
					Console.Error.Write(".");
				}

				if (quality.Average() > best_q)
				{
					best_q = quality.Average();
					best_log_reg = exp;
				}

				Console.Error.WriteLine("reg={0}, {1}=({2}, {3}, {4})", reg, criterion, quality.Min(), quality.Average(), quality.Max());
			} //foreach

			engine.reg_mapping = Math.Pow(reg_base, best_log_reg);
			engine.EntityRelationDataProvider = backend; // reset
			Console.Error.WriteLine();
			return best_log_reg;
		}

		static void FindGoodLearnRate(BPRMF_Mapping engine)
		{
			Console.Error.WriteLine("Finding good learn rate ...");

			double best_fit = double.MinValue;
			double best_lr  = 0;

			engine.reg_mapping = 0;

			double[] learn_rates = new double[]{ 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 5 };

			foreach (var lr in learn_rates)
			{
				engine.learn_rate_mapping = lr;

				engine.LearnAttributeToFactorMapping();
				double fit = engine.ComputeFit();

				if (fit > best_fit)
				{
					best_fit = fit;
					best_lr  = lr;
				}

				Console.Error.WriteLine("lr={0}, fit={1}", lr, fit);
			} //foreach

			engine.learn_rate_mapping = best_lr;
			Console.Error.WriteLine("Pick {0}", best_lr);
		}
		*/

		/*
		static double Log2(double x)
		{
			return Math.Log(x) / Math.Log(2);
		}

		// HP search for SVR
		static void FindGoodHyperparameters(BPRMF_ItemMapping_SVR engine,
		                                      int cross_validation)
		{
			double coarse_grid_size =   (ParameterSelection.MAX_C - ParameterSelection.MIN_C)
				                      * (ParameterSelection.MAX_G - ParameterSelection.MIN_G)
					                  / (ParameterSelection.C_STEP * ParameterSelection.G_STEP);
			Console.Error.WriteLine("Coarse grid size {0}", coarse_grid_size);
			double fine_grid_size = 4 * 4;
			Console.Error.WriteLine("Fine grid size {0}", fine_grid_size);
			FindGoodHyperparameters(engine,
			                        ParameterSelection.GetList(ParameterSelection.MIN_C, ParameterSelection.MAX_C, ParameterSelection.C_STEP),
			                        ParameterSelection.GetList(ParameterSelection.MIN_G, ParameterSelection.MAX_G, ParameterSelection.G_STEP),
			                        cross_validation);

			FindGoodHyperparameters(engine,
			                        ParameterSelection.GetList(Log2(engine.C)     - 2, Log2(engine.C)     + 2, 1),
									ParameterSelection.GetList(Log2(engine.Gamma) - 2, Log2(engine.Gamma) + 2, 1),
			                        cross_validation);
			// TODO better handling of corner cases
		}

		static void FindGoodHyperparameters(BPRMF_ItemMapping_SVR engine,
									          List<double> CValues,
            							      List<double> GammaValues,
		                                      int cross_validation)  // TODO make the type of split configurable
		{
			string criterion = "prec@5";

			IEntityRelationDataProvider backend = engine.EntityRelationDataProvider; // save for later use
			CrossvalidationSplit split = new CrossvalidationSplit((WP2Backend)backend, cross_validation, false);
			// TODO the split should only be created once, not on every call of the function

			double best_C     = 0;
			double best_Gamma = 0;
			double best_q     = 0;

			foreach (double C in CValues)
			{
				engine.C = C;
				foreach (double Gamma in GammaValues)
				{
					engine.Gamma = Gamma;
					IList<double> quality = new List<double>();
					for (int j = 0; j < cross_validation; j++)
					{
						engine.EntityRelationDataProvider = split.GetTrainingSet(j);

						engine.LearnAttributeToFactorMapping();

						var eval_result = Evaluate.EvaluateItemRecommender(
                    		engine,
							split.GetTestSet(j),
        	            	split.GetTrainingSet(j).GetRelation(relationType),
            	        	//relevant_items.GetEntity(EntityType.CatalogItem),
							// TODO do this if there is an overlap, otherwise not
				        	split.GetRelevantItems(j),
                        	!eval_new_users
			    		);

						double q = eval_result[criterion];
						quality.Add(q);
						Console.Error.Write(".");
					}

					if (quality.Average() > best_q)
					{
						best_q     = quality.Average();
						best_C     = C;
						best_Gamma = Gamma;
					}

					Console.Error.WriteLine("C={0}, Gamma={1}, {2}=({3}, {4}, {5})", C, Gamma, criterion, quality.Min(), quality.Average(), quality.Max());
				} //foreach
			} //foreach

			engine.C     = best_C;
			engine.Gamma = best_Gamma;

			engine.EntityRelationDataProvider = backend; // reset
			Console.Error.WriteLine();
		}
		*/

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
	}
}
