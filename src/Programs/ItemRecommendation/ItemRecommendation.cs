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
class ItemRecommendation : CommandLineProgram
{
	// data
	IPosOnlyFeedback training_data;
	IPosOnlyFeedback test_data;
	IList<int> test_users;
	IList<int> candidate_items;
	SparseBooleanMatrix group_to_user; // rows: groups, columns: users
	ICollection<int> user_groups;

	CandidateItems eval_item_mode = CandidateItems.UNION;

	// recommenders
	IRecommender recommender = null;

	// ID mapping objects
	IEntityMapping user_mapping = new EntityMapping();
	IEntityMapping item_mapping = new EntityMapping();

	// user and item attributes
	SparseBooleanMatrix user_attributes;
	SparseBooleanMatrix item_attributes;

	// command-line parameters (data)
	ItemDataFileFormat file_format = ItemDataFileFormat.DEFAULT;
	string test_users_file;
	string candidate_items_file;
	string user_groups_file;
	string prediction_file;

	// command-line parameters (other)
	uint cross_validation;
	double test_ratio;
	float rating_threshold = float.NaN;
	int num_test_users;
	int predict_items_number = -1;
	bool online_eval;
	bool repeat_eval;
	string group_method;
	bool overlap_items;
	bool in_training_items;
	bool in_test_items;
	bool all_items;
	bool user_prediction;
	bool no_id_mapping = false;
	int random_seed = -1;
	int find_iter = 0;

	void ShowVersion()
	{
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite Item Prediction from Positive-Only Feedback {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine("Copyright (C) 2011, 2012 Zeno Gantner");
		Console.WriteLine("Copyright (C) 2010 Zeno Gantner, Steffen Rendle, Christoph Freudenthaler");
		Console.WriteLine("This is free software; see the source for copying conditions.  There is NO");
		Console.WriteLine("warranty; not even for MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.");
		Environment.Exit(0);
	}

	void Usage(string message)
	{
		Console.WriteLine(message);
		Console.WriteLine();
		Usage(-1);
	}

	void Usage(int exit_code)
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
  The default is to use both the items in the training and the test set as candidate items.

  choosing the users for evaluation/prediction
   --test-users=FILE            predict items for users specified in FILE (one user per line)

  prediction and evaluation:
   --predict-items-number=N     predict N items per user
   --repeat-evaluation          items accessed by a user before may be in the recommendations (and are not ignored in the evaluation)

  prediction:
   --prediction-file=FILE       write ranked predictions to FILE, one user per line

  evaluation:
   --cross-validation=K         perform k-fold cross-validation on the training data
   --test-ratio=NUM             evaluate by splitting of a NUM part of the feedback
   --num-test-users=N           evaluate on only N randomly picked users (to save time)
   --online-evaluation          perform online evaluation (use every tested user-item combination for incremental training)
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

	static void Main(string[] args)
	{
		var program = new ItemRecommendation();
		program.Run(args);
	}

	public void Run(string[] args)
	{
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
			{ "save-user-mapping=",   v => save_user_mapping_file = v },
			{ "save-item-mapping=",   v => save_item_mapping_file = v },
			{ "load-user-mapping=",   v => load_user_mapping_file = v },
			{ "load-item-mapping=",   v => load_item_mapping_file = v },
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
			// floating point options
			{ "epsilon=",             (double v)     => epsilon      = v },
			{ "cutoff=",              (double v)     => cutoff       = v },
			{ "test-ratio=",          (double v) => test_ratio       = v },
			{ "rating-threshold=",    (float v)  => rating_threshold = v },
			// enum options
			{ "file-format=",         (ItemDataFileFormat v) => file_format = v },
			// boolean options
			{ "user-prediction",      v => user_prediction   = v != null },
			{ "compute-fit",          v => compute_fit       = v != null },
			{ "online-evaluation",    v => online_eval       = v != null },
			{ "repeat-evaluation",    v => repeat_eval       = v != null },
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
			Abort(string.Format("Could not load model from file {0}.", load_model_file));

		CheckParameters(extra_args);

		recommender.Configure(recommender_options, (string m) => { Console.Error.WriteLine(m); Environment.Exit(-1); });

		if (no_id_mapping)
		{
			user_mapping = new IdentityMapping();
			item_mapping = new IdentityMapping();
		}
		if (load_user_mapping_file != null)
			user_mapping = EntityMappingExtensions.LoadMapping(load_user_mapping_file);
		if (load_item_mapping_file != null)
			item_mapping = EntityMappingExtensions.LoadMapping(load_item_mapping_file);

		// load all the data
		LoadData();
		Console.Write(training_data.Statistics(test_data, user_attributes, item_attributes));

		// if requested, save ID mappings
		if (save_user_mapping_file != null)
			user_mapping.SaveMapping(save_user_mapping_file);
		if (save_item_mapping_file != null)
			item_mapping.SaveMapping(save_item_mapping_file);

		TimeSpan time_span;

		if (find_iter != 0)
		{
			if ( !(recommender is IIterativeModel) )
				Abort("Only iterative recommenders (interface IIterativeModel) support --find-iter=N.");

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
					var results = recommender.DoCrossValidation(cross_validation, test_users, candidate_items, eval_item_mode, compute_fit, true);
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
						Usage("Unknown group recommendation strategy in --group-recommender=METHOD");

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

	void CheckParameters(IList<string> extra_args)
	{
		if (training_file == null)
			Usage("Parameter --training-file=FILE is missing.");

		if (online_eval && !(recommender is IIncrementalItemRecommender))
			Abort(string.Format("Recommender {0} does not support incremental updates, which are necessary for an online experiment.", recommender.GetType().Name));

		if (cross_validation == 1)
			Abort("--cross-validation=K requires K to be at least 2.");

		if (cross_validation > 1 && test_ratio != 0)
			Abort("--cross-validation=K and --test-ratio=NUM are mutually exclusive.");

		if (cross_validation > 1 && prediction_file != null)
			Abort("--cross-validation=K and --prediction-file=FILE are mutually exclusive.");

		if (cross_validation > 1 && save_model_file != null)
			Abort("--cross-validation=K and --save-model=FILE are mutually exclusive.");

		if (cross_validation > 1 && load_model_file != null)
			Abort("--cross-validation=K and --load-model=FILE are mutually exclusive.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && save_model_file == null && test_users_file == null)
			Usage("Please provide either test-file=FILE, --test-ratio=NUM, --cross-validation=K, --save-model=FILE, or --test-users=FILE.");

		if ((candidate_items_file != null ? 1 : 0) + (all_items ? 1 : 0) + (in_training_items ? 1 : 0) + (in_test_items ? 1 : 0) + (overlap_items ? 1 : 0) > 1)
			Abort("--candidate-items=FILE, --all-items, --in-training-items, --in-test-items, and --overlap-items are mutually exclusive.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && overlap_items)
			Abort("--overlap-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && in_test_items)
			Abort("--in-test-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && in_training_items)
			Abort("--in-training-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (group_method != null && user_groups_file == null)
			Abort("--group-recommender needs --user-groups=FILE.");

		if (user_prediction)
		{
			if (recommender is IUserAttributeAwareRecommender || recommender is IItemAttributeAwareRecommender ||
			    recommender is IUserRelationAwareRecommender  || recommender is IItemRelationAwareRecommender)
				Abort("--user-prediction is not (yet) supported in combination with attribute- or relation-aware recommenders.");
			if (user_groups_file != null)
				Abort("--user-prediction is not (yet) supported in combination with --user-groups=FILE.");
		}

		if (recommender is IUserAttributeAwareRecommender && user_attributes_file == null)
			Abort("Recommender expects --user-attributes=FILE.");

		if (recommender is IItemAttributeAwareRecommender && item_attributes_file == null)
			Abort("Recommender expects --item-attributes=FILE.");

		if (recommender is IUserRelationAwareRecommender && user_relations_file == null)
			Abort("Recommender expects --user-relations=FILE.");

		if (recommender is IItemRelationAwareRecommender && user_relations_file == null)
			Abort("Recommender expects --item-relations=FILE.");

		if (no_id_mapping)
		{
			if (save_user_mapping_file != null)
				Abort("--save-user-mapping=FILE and --no-id-mapping are mutually exclusive.");
			if (save_item_mapping_file != null)
				Abort("--save-item-mapping=FILE and --no-id-mapping are mutually exclusive.");
			if (load_user_mapping_file != null)
				Abort("--load-user-mapping=FILE and --no-id-mapping are mutually exclusive.");
			if (load_item_mapping_file != null)
				Abort("--load-item-mapping=FILE and --no-id-mapping are mutually exclusive.");
		}

		if (extra_args.Count > 0)
			Usage("Did not understand " + extra_args[0]);
	}

	void LoadData()
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

	ItemRecommendationEvaluationResults ComputeFit()
	{
		return recommender.Evaluate(training_data, training_data, test_users, candidate_items, eval_item_mode, true, predict_items_number);
	}

	ItemRecommendationEvaluationResults Evaluate()
	{
		return recommender.Evaluate(test_data, training_data, test_users, candidate_items, eval_item_mode, repeat_eval, predict_items_number);
	}

	void Predict(string prediction_file, string predict_for_users_file, int iteration)
	{
		if (prediction_file == null)
			return;

		Predict(prediction_file + "-it-" + iteration, predict_for_users_file);
	}

	void Predict(string prediction_file, string predict_for_users_file)
	{
		if (candidate_items == null)
			candidate_items = training_data.AllItems;

		IList<int> user_list = null;
		if (predict_for_users_file != null)
			user_list = user_mapping.ToInternalID( File.ReadLines(Path.Combine(data_dir, predict_for_users_file)).ToArray() );

		TimeSpan time_span = Wrap.MeasureTime( delegate() {
			recommender.WritePredictions(
				training_data,
				candidate_items, predict_items_number, // TODO move this argument to the end of the list
				prediction_file, user_list,
				user_mapping, item_mapping);
			if (user_list != null)
				Console.Error.WriteLine("Wrote predictions for {0} users to file {1}.", user_list.Count, prediction_file);
			else
				Console.Error.WriteLine("Wrote predictions to file {0}.", prediction_file);
		});
		Console.Write(" prediction_time " + time_span);
	}
}
