// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
using MyMediaLite.GroupRecommendation;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.Util;

/// <summary>Item prediction program, see Usage() method for more information</summary>
class ItemRecommendation
{
	// data
	static IPosOnlyFeedback training_data;
	static IPosOnlyFeedback test_data;
	static IList<int> test_users;
	static IList<int> candidate_items;
	static SparseBooleanMatrix group_to_user; // rows: groups, columns: users
	static ICollection<int> user_groups;

	static CandidateItems eval_item_mode = CandidateItems.UNION;

	// recommenders
	static IRecommender recommender = null;

	// ID mapping objects
	static IEntityMapping user_mapping = new EntityMapping();
	static IEntityMapping item_mapping = new EntityMapping();

	// user and item attributes
	static SparseBooleanMatrix user_attributes;
	static SparseBooleanMatrix item_attributes;

	// command-line parameters (data)
	static string training_file;
	static string test_file;
	static ItemDataFileFormat file_format = ItemDataFileFormat.DEFAULT;
	static string data_dir = string.Empty;
	static string test_users_file;
	static string candidate_items_file;
	static string user_attributes_file;
	static string item_attributes_file;
	static string user_relations_file;
	static string item_relations_file;
	static string save_model_file = null;
	static string load_model_file = null;
	static string user_groups_file;
	static string prediction_file;

	// command-line parameters (other)
	static bool compute_fit;
	static uint cross_validation;
	static bool show_fold_results;
	static double test_ratio;
	static double rating_threshold = double.NaN;
	static int num_test_users;
	static int predict_items_number = -1;
	static bool online_eval;
	static bool filtered_eval;
	static bool repeat_eval;
	static string group_method;
	static bool overlap_items;
	static bool in_training_items;
	static bool in_test_items;
	static bool all_items;
	static bool user_prediction;
	static int random_seed = -1;
	static int find_iter = 0;

	// time statistics
	static List<double> training_time_stats = new List<double>();
	static List<double> fit_time_stats      = new List<double>();
	static List<double> eval_time_stats     = new List<double>();

	static void ShowVersion()
	{
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite Item Prediction from Positive-Only Feedback {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine("Copyright (C) 2010 Zeno Gantner, Steffen Rendle, Christoph Freudenthaler");
		Console.WriteLine("Copyright (C) 2011, 2012 Zeno Gantner");
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
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite item recommendation from positive-only feedback {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine(@"
 usage:   item_recommendation --training-file=FILE --recommender=METHOD [OPTIONS]

   methods (plus arguments and their defaults):");

		Console.Write("   - ");
		Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.ItemRecommendation")));

		Console.WriteLine(@"  method ARGUMENTS have the form name=value

  general OPTIONS:
   --recommender=METHOD             use METHOD for recommendations (default: MostPopular)
   --group-recommender=METHOD       use METHOD to combine the predictions for several users
   --recommender-options=OPTIONS    use OPTIONS as recommender options
   --help                           display this usage information and exit
   --version                        display version information and exit
   --random-seed=N                  initialize random number generator with N

  files:
   --training-file=FILE                     read training data from FILE
   --test-file=FILE                         read test data from FILE
   --file-format=ignore_first_line|default
   --no-id-mapping                          do not map user and item IDs to internal IDs, keep the original IDs
   --data-dir=DIR                           load all files from DIR
   --user-attributes=FILE                   file with user attribute information, 1 tuple per line
   --item-attributes=FILE                   file with item attribute information, 1 tuple per line
   --user-relations=FILE                    file with user relation information, 1 tuple per line
   --item-relations=FILE                    file with item relation information, 1 tuple per line
   --user-groups=FILE                       file with group-to-user mappings, 1 tuple per line
   --save-model=FILE                        save computed model to FILE
   --load-model=FILE                        load model from FILE

  data interpretation:
   --user-prediction            transpose the user-item matrix and perform user prediction instead of item prediction
   --rating-threshold=NUM       (for rating data) interpret rating >= NUM as positive feedback

  choosing the items for evaluation/prediction (mutually exclusive):
   --candidate-items=FILE       use items in FILE (one per line) as candidate items
   --overlap-items              use only items that are both in the training and the test set as candidate items
   --in-training-items          use only items in the training set as candidate items
   --in-test-items              use only items in the test set as candidate items
   --all-items                  use all known items as candidate items

  choosing the users for evaluation/prediction
   --test-users=FILE            predict items for users specified in FILE (one user per line)

  prediction options:
   --prediction-file=FILE       write ranked predictions to FILE, one user per line
   --predict-items-number=N     predict N items per user (needs --prediction-file)

  evaluation options:
   --cross-validation=K         perform k-fold cross-validation on the training data
   --show-fold-results          show results for individual folds in cross-validation
   --test-ratio=NUM             evaluate by splitting of a NUM part of the feedback
   --num-test-users=N           evaluate on only N randomly picked users (to save time)
   --online-evaluation          perform online evaluation (use every tested user-item combination for incremental training)
   --filtered-evaluation        perform evaluation filtered by item attribute (requires --item-attributes=FILE)
   --repeat-evaluation          items accessed by a user before may be in the recommendations (and are not ignored in the evaluation)
   --compute-fit                display fit on training data

  finding the right number of iterations (iterative methods)
   --find-iter=N                give out statistics every N iterations
   --max-iter=N                 perform at most N iterations
   --measure=MEASURE            the evaluation measure to use for the abort conditions below (default is AUC)
   --epsilon=NUM                abort iterations if MEASURE is less than best result plus NUM
   --cutoff=NUM                 abort if MEASURE is below NUM
");
		Environment.Exit(exit_code);
	}

    public static void Main(string[] args)
    {
		Assembly assembly = Assembly.GetExecutingAssembly();
		Assembly.LoadFile(Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar + "MyMediaLiteExperimental.dll");

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyMediaLite.Util.Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);

		// recommender arguments
		string method              = null;
		string recommender_options = string.Empty;

		// help/version
		bool show_help    = false;
		bool show_version = false;

		// variables for iteration search
		int max_iter   = 500;
		double cutoff  = 0;
		double epsilon = 0;
		string measure = "AUC";

		compute_fit         = false;

		// other parameters
		bool no_id_mapping = false;
		test_ratio         = 0;
		num_test_users     = -1;
		repeat_eval        = false;

		var p = new OptionSet() {
			// string-valued options
			{ "training-file=",       v => training_file          = v },
			{ "test-file=",           v => test_file              = v },
			{ "recommender=",         v => method                 = v },
			{ "group-recommender=",   v => group_method           = v },
			{ "recommender-options=", v => recommender_options   += " " + v },
			{ "data-dir=",            v => data_dir               = v },
			{ "user-attributes=",     v => user_attributes_file   = v },
			{ "item-attributes=",     v => item_attributes_file   = v },
			{ "user-relations=",      v => user_relations_file    = v },
			{ "item-relations=",      v => item_relations_file    = v },
			{ "save-model=",          v => save_model_file        = v },
			{ "load-model=",          v => load_model_file        = v },
			{ "prediction-file=",     v => prediction_file        = v },
			{ "test-users=",          v => test_users_file        = v },
			{ "candidate-items=",     v => candidate_items_file   = v },
			{ "user-groups=",         v => user_groups_file       = v },
			{ "measure=",             v => measure                = v },
			// integer-valued options
			{ "find-iter=",            (int v) => find_iter            = v },
			{ "max-iter=",             (int v) => max_iter             = v },
			{ "random-seed=",          (int v) => random_seed          = v },
			{ "predict-items-number=", (int v) => predict_items_number = v },
			{ "num-test-users=",       (int v) => num_test_users       = v },
			{ "cross-validation=",     (uint v) => cross_validation    = v },
			// double-valued options
			{ "epsilon=",             (double v)     => epsilon      = v },
			{ "cutoff=",              (double v)     => cutoff       = v },
			{ "test-ratio=",          (double v) => test_ratio       = v },
			{ "rating-threshold=",    (double v) => rating_threshold = v },
			// enum options
			{ "file-format=",         (ItemDataFileFormat v) => file_format = v },
			// boolean options
			{ "user-prediction",      v => user_prediction   = v != null },
			{ "compute-fit",          v => compute_fit       = v != null },
			{ "online-evaluation",    v => online_eval       = v != null },
			{ "filtered-evaluation",  v => filtered_eval     = v != null },
			{ "repeat-evaluation",    v => repeat_eval       = v != null },
			{ "show-fold-results",    v => show_fold_results = v != null },
			{ "no-id-mapping",        v => no_id_mapping     = v != null },
			{ "overlap-items",        v => overlap_items     = v != null },
			{ "all-items",            v => all_items         = v != null },
			{ "in-training-items",    v => in_training_items = v != null },
			{ "in-test-items",        v => in_test_items     = v != null },
			{ "help",                 v => show_help         = v != null },
			{ "version",              v => show_version      = v != null },
		};
		IList<string> extra_args = p.Parse(args);

		bool no_eval = true;
		if (test_ratio > 0 || test_file != null)
			no_eval = false;

		if (show_version)
			ShowVersion();
		if (show_help)
			Usage(0);

		if (random_seed != -1)
			MyMediaLite.Util.Random.Seed = random_seed;

		// set up recommender
 		if (load_model_file != null)
			recommender = Model.Load(load_model_file);
		else if (method != null)
			recommender = Recommender.CreateItemRecommender(method);
		else
			recommender = Recommender.CreateItemRecommender("MostPopular");
		// in case something went wrong ...
		if (recommender == null && method != null)
			Usage(string.Format("Unknown recommendation method: '{0}'", method));
		if (recommender == null && load_model_file != null)
			Usage(string.Format("Could not load model from file {0}.", load_model_file));

		CheckParameters(extra_args);

		recommender.Configure(recommender_options, Usage);

		if (no_id_mapping)
		{
			user_mapping = new IdentityMapping();
			item_mapping = new IdentityMapping();
		}

		// load all the data
		LoadData();
		Console.Write(training_data.Statistics(test_data, user_attributes, item_attributes));

		TimeSpan time_span;

		if (find_iter != 0)
		{
			if ( !(recommender is IIterativeModel) )
				Usage("Only iterative recommenders (interface IIterativeModel) support --find-iter=N.");

			var iterative_recommender = (IIterativeModel) recommender;
			Console.WriteLine(recommender);
			var eval_stats = new List<double>();

			if (cross_validation > 1)
			{
				recommender.DoIterativeCrossValidation(cross_validation, test_users, candidate_items, eval_item_mode, repeat_eval, max_iter, find_iter);
			}
			else
			{
				if (load_model_file == null)
					recommender.Train();

				if (compute_fit)
					Console.WriteLine("fit: {0} iteration {1} ", ComputeFit(), iterative_recommender.NumIter);

				var results = Evaluate();
				Console.WriteLine("{0} iteration {1}", results, iterative_recommender.NumIter);

				for (int it = (int) iterative_recommender.NumIter + 1; it <= max_iter; it++)
				{
					TimeSpan t = Wrap.MeasureTime(delegate() {
						iterative_recommender.Iterate();
					});
					training_time_stats.Add(t.TotalSeconds);

					if (it % find_iter == 0)
					{
						if (compute_fit)
						{
							t = Wrap.MeasureTime(delegate() {
								Console.WriteLine("fit: {0} iteration {1} ", ComputeFit(), it);
							});
							fit_time_stats.Add(t.TotalSeconds);
						}

						t = Wrap.MeasureTime(delegate() { results = Evaluate(); });
						eval_time_stats.Add(t.TotalSeconds);
						eval_stats.Add(results[measure]);
						Console.WriteLine("{0} iteration {1}", results, it);

						Model.Save(recommender, save_model_file, it);
						Predict(prediction_file, test_users_file, it);

						if (epsilon > 0.0 && eval_stats.Max() - results[measure] > epsilon)
						{
							Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} >> {1}", results["RMSE"], eval_stats.Min()));
							Console.Error.WriteLine("Reached convergence on training/validation data after {0} iterations.", it);
							break;
						}
						if (results[measure] < cutoff)
						{
								Console.Error.WriteLine("Reached cutoff after {0} iterations.", it);
								Console.Error.WriteLine("DONE");
								break;
						}
					}
				} // for
			}
		}
		else
		{
			Console.WriteLine(recommender + " ");

			if (load_model_file == null)
			{
				if (cross_validation > 1)
				{
					var results = recommender.DoCrossValidation(cross_validation, test_users, candidate_items, eval_item_mode, compute_fit, show_fold_results);
					Console.Write(results);
					no_eval = true;
				}
				else
				{
					time_span = Wrap.MeasureTime( delegate() { recommender.Train(); } );
					Console.Write("training_time " + time_span + " ");
				}
			}

			if (prediction_file != null)
			{
				Predict(prediction_file, test_users_file);
			}
			else if (!no_eval)
			{
				if (compute_fit)
					Console.WriteLine("fit: {0}", ComputeFit());

				if (online_eval)
					time_span = Wrap.MeasureTime( delegate() {
						var results = recommender.EvaluateOnline(test_data, training_data, test_users, candidate_items, eval_item_mode);
						Console.Write(results);
					});
				else if (group_method != null)
				{
					GroupRecommender group_recommender = null;

					Console.Write("group recommendation strategy: {0} ", group_method);
					// TODO GroupUtils.CreateGroupRecommender(group_method, recommender);
					if (group_method == "Average")
						group_recommender = new Average(recommender);
					else if (group_method == "Minimum")
						group_recommender = new Minimum(recommender);
					else if (group_method == "Maximum")
						group_recommender = new Maximum(recommender);
					else
						Usage("Unknown method in --group-recommender=METHOD");

					time_span = Wrap.MeasureTime( delegate() {
						var result = group_recommender.Evaluate(test_data, training_data, group_to_user, candidate_items);
						Console.Write(result);
					});
				}
				else
					time_span = Wrap.MeasureTime( delegate() { Console.Write(Evaluate()); });
				Console.Write(" testing_time " + time_span);
			}
			Console.WriteLine();
		}
		Model.Save(recommender, save_model_file);
		DisplayStats();
	}

	static void CheckParameters(IList<string> extra_args)
	{
		// TODO block group vs. filter/online, etc.

		if (training_file == null)
			Usage("Parameter --training-file=FILE is missing.");

		if (online_eval && filtered_eval)
			Usage("Combination of --online-eval and --filtered-eval is not (yet) supported.");

		if (online_eval && !(recommender is IIncrementalItemRecommender))
			Usage(string.Format("Recommender {0} does not support incremental updates, which are necessary for an online experiment.", recommender.GetType().Name));

		if (cross_validation == 1)
			Usage("--cross-validation=K requires K to be at least 2.");

		if (show_fold_results && cross_validation == 0)
			Usage("--show-fold-results only works with --cross-validation=K.");

		if (cross_validation > 1 && test_ratio != 0)
			Usage("--cross-validation=K and --test-ratio=NUM are mutually exclusive.");

		if (cross_validation > 1 && prediction_file != null)
			Usage("--cross-validation=K and --prediction-file=FILE are mutually exclusive.");

		if (test_file == null && test_ratio == 0 &&  cross_validation == 0 && save_model_file == null && test_users_file == null)
			Usage("Please provide either test-file=FILE, --test-ratio=NUM, --cross-validation=K, --save-model=FILE, or --test-users=FILE.");

		if ((candidate_items_file != null ? 1 : 0) + (all_items ? 1 : 0) + (in_training_items ? 1 : 0) + (in_test_items ? 1 : 0) + (overlap_items ? 1 : 0) > 1)
			Usage("--candidate-items=FILE, --all-items, --in-training-items, --in-test-items, and --overlap-items are mutually exclusive.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && overlap_items)
			Usage("--overlap-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && in_test_items)
			Usage("--in-test-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && in_training_items)
			Usage("--in-training-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (group_method != null && user_groups_file == null)
			Usage("--group-recommender needs --user-groups=FILE.");

		if (user_prediction)
		{
			if (recommender is IUserAttributeAwareRecommender || recommender is IItemAttributeAwareRecommender ||
			    recommender is IUserRelationAwareRecommender  || recommender is IItemRelationAwareRecommender)
				Usage("--user-prediction is not (yet) supported in combination with attribute- or relation-aware recommenders.");
			if (filtered_eval)
				Usage("--user-prediction is not (yet) supported in combination with --filtered-evaluation.");
			if (user_groups_file != null)
				Usage("--user-prediction is not (yet) supported in combination with --user-groups=FILE.");
		}

		if (recommender is IUserAttributeAwareRecommender && user_attributes_file == null)
			Usage("Recommender expects --user-attributes=FILE.");

		if (recommender is IItemAttributeAwareRecommender && item_attributes_file == null)
			Usage("Recommender expects --item-attributes=FILE.");

		if (filtered_eval && item_attributes_file == null)
			Usage("--filtered-evaluation expects --item-attributes=FILE.");

		if (recommender is IUserRelationAwareRecommender && user_relations_file == null)
			Usage("Recommender expects --user-relations=FILE.");

		if (recommender is IItemRelationAwareRecommender && user_relations_file == null)
			Usage("Recommender expects --item-relations=FILE.");

		if (extra_args.Count > 0)
			Usage("Did not understand " + extra_args[0]);
	}

	static void LoadData()
	{
		TimeSpan loading_time = Wrap.MeasureTime(delegate() {
			// training data
			training_file = Path.Combine(data_dir, training_file);
			training_data = double.IsNaN(rating_threshold)
				? ItemData.Read(training_file, user_mapping, item_mapping, file_format == ItemDataFileFormat.IGNORE_FIRST_LINE)
				: ItemDataRatingThreshold.Read(training_file, rating_threshold, user_mapping, item_mapping, file_format == ItemDataFileFormat.IGNORE_FIRST_LINE);

			// user attributes
			if (user_attributes_file != null)
				user_attributes = AttributeData.Read(Path.Combine(data_dir, user_attributes_file), user_mapping);
			if (recommender is IUserAttributeAwareRecommender)
				((IUserAttributeAwareRecommender)recommender).UserAttributes = user_attributes;

			// item attributes
			if (item_attributes_file != null)
				item_attributes = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
			if (recommender is IItemAttributeAwareRecommender)
				((IItemAttributeAwareRecommender)recommender).ItemAttributes = item_attributes;

			// user relation
			if (recommender is IUserRelationAwareRecommender)
			{
				((IUserRelationAwareRecommender)recommender).UserRelation = RelationData.Read(Path.Combine(data_dir, user_relations_file), user_mapping);
				Console.WriteLine("relation over {0} users", ((IUserRelationAwareRecommender)recommender).NumUsers);
			}

			// item relation
			if (recommender is IItemRelationAwareRecommender)
			{
				((IItemRelationAwareRecommender)recommender).ItemRelation = RelationData.Read(Path.Combine(data_dir, item_relations_file), item_mapping);
				Console.WriteLine("relation over {0} items", ((IItemRelationAwareRecommender)recommender).NumItems);
			}

			// user groups
			if (user_groups_file != null)
			{
				group_to_user = RelationData.Read(Path.Combine(data_dir, user_groups_file), user_mapping); // assumption: user and user group IDs are disjoint
				user_groups = group_to_user.NonEmptyRowIDs;
				Console.WriteLine("{0} user groups", user_groups.Count);
			}

			// test data
			if (test_ratio == 0)
			{
				if (test_file != null)
				{
					test_file = Path.Combine(data_dir, test_file);
					test_data = double.IsNaN(rating_threshold)
						? ItemData.Read(test_file, user_mapping, item_mapping, file_format == ItemDataFileFormat.IGNORE_FIRST_LINE)
						: ItemDataRatingThreshold.Read(test_file, rating_threshold, user_mapping, item_mapping, file_format == ItemDataFileFormat.IGNORE_FIRST_LINE);
				}
			}
			else
			{
				var split = new PosOnlyFeedbackSimpleSplit<PosOnlyFeedback<SparseBooleanMatrix>>(training_data, test_ratio);
				training_data = split.Train[0];
				test_data     = split.Test[0];
			}

			if (group_method == "GroupsAsUsers")
			{
				Console.WriteLine("group recommendation strategy: {0}", group_method);
				// TODO verify what is going on here

				//var training_data_group = new PosOnlyFeedback<SparseBooleanMatrix>();
				// transform groups to users
				foreach (int group_id in group_to_user.NonEmptyRowIDs)
					foreach (int user_id in group_to_user[group_id])
						foreach (int item_id in training_data.UserMatrix.GetEntriesByRow(user_id))
							training_data.Add(group_id, item_id);
				// add the users that do not belong to groups

				//training_data = training_data_group;

				// transform groups to users
				var test_data_group = new PosOnlyFeedback<SparseBooleanMatrix>();
				foreach (int group_id in group_to_user.NonEmptyRowIDs)
					foreach (int user_id in group_to_user[group_id])
						foreach (int item_id in test_data.UserMatrix.GetEntriesByRow(user_id))
							test_data_group.Add(group_id, item_id);

				test_data = test_data_group;

				group_method = null; // deactivate s.t. the normal eval routines are used
			}

			if (user_prediction)
			{
				// swap file names for test users and candidate items
				var ruf = test_users_file;
				var rif = candidate_items_file;
				test_users_file = rif;
				candidate_items_file = ruf;

				// swap user and item mappings
				var um = user_mapping;
				var im = item_mapping;
				user_mapping = im;
				item_mapping = um;

				// transpose training and test data
				training_data = training_data.Transpose();

				// transpose test data
				if (test_data != null)
					test_data = test_data.Transpose();
			}

			if (recommender is MyMediaLite.ItemRecommendation.ItemRecommender)
				((ItemRecommender)recommender).Feedback = training_data;

			// test users
			if (test_users_file != null)
				test_users = user_mapping.ToInternalID( File.ReadLines(Path.Combine(data_dir, test_users_file)).ToArray() );
			else
				test_users = test_data != null ? test_data.AllUsers : training_data.AllUsers;

			// if necessary, perform user sampling
			if (num_test_users > 0 && num_test_users < test_users.Count)
			{
				var old_test_users = new HashSet<int>(test_users);
				var new_test_users = new int[num_test_users];
				for (int i = 0; i < num_test_users; i++)
				{
					int random_index = MyMediaLite.Util.Random.GetInstance().Next(old_test_users.Count - 1);
					new_test_users[i] = old_test_users.ElementAt(random_index);
					old_test_users.Remove(new_test_users[i]);
				}
				test_users = new_test_users;
			}

			// candidate items
			if (candidate_items_file != null)
				candidate_items = item_mapping.ToInternalID( File.ReadLines(Path.Combine(data_dir, candidate_items_file)).ToArray() );
			else if (all_items)
				candidate_items = Enumerable.Range(0, item_mapping.InternalIDs.Max() + 1).ToArray();

			if (candidate_items != null)
				eval_item_mode = CandidateItems.EXPLICIT;
			else if (in_training_items)
				eval_item_mode = CandidateItems.TRAINING;
			else if (in_test_items)
				eval_item_mode = CandidateItems.TEST;
			else if (overlap_items)
				eval_item_mode = CandidateItems.OVERLAP;
			else
				eval_item_mode = CandidateItems.UNION;
		});
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loading_time {0,0:0.##}", loading_time.TotalSeconds));
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}

	static ItemRecommendationEvaluationResults ComputeFit()
	{
		if (filtered_eval)
			return recommender.EvaluateFiltered(training_data, training_data, item_attributes, test_users, candidate_items, true);
		else
			return recommender.Evaluate(training_data, training_data, test_users, candidate_items, eval_item_mode, true);
	}

	static ItemRecommendationEvaluationResults Evaluate()
	{
		if (filtered_eval)
			return recommender.EvaluateFiltered(test_data, training_data, item_attributes, test_users, candidate_items, repeat_eval);
		else
			return recommender.Evaluate(test_data, training_data, test_users, candidate_items, eval_item_mode, repeat_eval);
	}

	static void Predict(string prediction_file, string predict_for_users_file, int iteration)
	{
		if (prediction_file == null)
			return;

		Predict(prediction_file + "-it-" + iteration, predict_for_users_file);
	}

	static void Predict(string prediction_file, string predict_for_users_file)
	{
		if (candidate_items == null)
			candidate_items = training_data.AllItems;

		IList<int> user_list = null;
		if (predict_for_users_file != null)
			user_list = user_mapping.ToInternalID( File.ReadLines(predict_for_users_file).ToArray() );

		TimeSpan time_span = Wrap.MeasureTime( delegate() {
			recommender.WritePredictions(
				training_data,
				candidate_items, predict_items_number,
				prediction_file, user_list,
				user_mapping, item_mapping);
			Console.Error.WriteLine("Wrote predictions to {0}", prediction_file);
		});
		Console.Write(" prediction_time " + time_span);
	}

	static void AbortHandler(object sender, ConsoleCancelEventArgs args)
	{
		DisplayStats();
	}

	static void DisplayStats()
	{
		if (training_time_stats.Count > 0)
			Console.Error.WriteLine(
				string.Format(
					CultureInfo.InvariantCulture,
					"iteration_time: min={0:0.##}, max={1:0.##}, avg={2:0.##}",
					training_time_stats.Min(), training_time_stats.Max(), training_time_stats.Average()));
		if (eval_time_stats.Count > 0)
			Console.Error.WriteLine(
				string.Format(
					CultureInfo.InvariantCulture,
					"eval_time: min={0:0.###}, max={1:0.###}, avg={2:0.###}",
					eval_time_stats.Min(), eval_time_stats.Max(), eval_time_stats.Average()));
		if (compute_fit && fit_time_stats.Count > 0)
			Console.Error.WriteLine(
				string.Format(
					CultureInfo.InvariantCulture,
					"fit_time: min={0:0.##}, max={1:0.##}, avg={2:0.##}",
					fit_time_stats.Min(), fit_time_stats.Max(), fit_time_stats.Average()));
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}
}
