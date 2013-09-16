// Copyright (C) 2010, 2011, 2012, 2013 Zeno Gantner
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
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>
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

/// <summary>Item prediction program, see Usage() method for more information</summary>
class ItemRecommendation : CommandLineProgram<IRecommender>
{
	// data
	IPosOnlyFeedback training_data;
	IPosOnlyFeedback test_data;
	IList<int> test_users;
	IList<int> candidate_items;

	CandidateItems eval_item_mode = CandidateItems.UNION;

	// command-line parameters (data)
	ItemDataFileFormat file_format = ItemDataFileFormat.DEFAULT;
	string test_users_file;
	string candidate_items_file;

	// command-line parameters (other)
	float rating_threshold = float.NaN;
	int num_test_users = -1;
	int predict_items_number = -1;
	bool repeated_items;
	bool overlap_items;
	bool in_training_items;
	bool in_test_items;
	bool all_items;
	bool user_prediction;

	protected override ICollection<string> Measures { get { return Items.Measures; } }

	public ItemRecommendation()
	{
		cutoff  = double.MinValue;
		eval_measures = ItemRecommendationEvaluationResults.DefaultMeasuresToShow;
	}

	protected override void ShowVersion()
	{
		ShowVersion(
			"Item Recommendation from Positive-Only Feedback",
			"Copyright (C) 2011, 2012, 2013 Zeno Gantner\nCopyright (C) 2010 Zeno Gantner, Steffen Rendle, Christoph Freudenthaler"
		);
	}

	protected override void Usage(int exit_code)
	{
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite item recommendation from positive-only feedback {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine(@"
 usage:   item_recommendation --training-file=FILE --recommender=METHOD [OPTIONS]

   methods (plus arguments and their defaults):");

		Console.Write("   - ");
		Console.WriteLine(string.Join("\n   - ", "MyMediaLite.ItemRecommendation".ListRecommenders()));

		Console.WriteLine(@"  method ARGUMENTS have the form name=value

  general OPTIONS:
   --recommender=METHOD             use METHOD for recommendations (default: MostPopular)
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
   --save-model=FILE                        save computed model to FILE
   --load-model=FILE                        load model from FILE
   --save-user-mapping=FILE                 save user ID mapping to FILE
   --save-item-mapping=FILE                 save item ID mapping to FILE
   --load-user-mapping=FILE                 load user ID mapping from FILE
   --load-item-mapping=FILE                 load item ID mapping from FILE

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
   --repeated-items             items accessed by a user before may be in the recommendations (and are not ignored in the evaluation)

  prediction:
   --prediction-file=FILE       write ranked predictions to FILE, one user per line

  evaluation:
   --cross-validation=K         perform k-fold cross-validation on the training data
   --test-ratio=NUM             evaluate by splitting of a NUM part of the feedback
   --num-test-users=N           evaluate on only N randomly picked users (to save time)
   --online-evaluation          perform online evaluation (use every tested user-item combination for incremental training)
   --compute-fit                display fit on training data
   --measures=LIST              the evaluation measures to display (default is 'AUC, prec@5')
                                use --help-measures to get a list of all available measures

  finding the right number of iterations (iterative methods)
   --find-iter=N                give out statistics every N iterations
   --num-iter=N                 start measuring at N iterations
   --max-iter=N                 perform at most N iterations
   --epsilon=NUM                abort iterations if main measure is less than best result plus NUM
   --cutoff=NUM                 abort if main measure is below NUM
");
		Environment.Exit(exit_code);
	}

	static void Main(string[] args)
	{
		var program = new ItemRecommendation();
		program.Run(args);
	}

	protected override void SetupOptions()
	{
		options
			.Add("candidate-items=",     v => candidate_items_file   = v)
			.Add("test-users=",          v => test_users_file      = v)
			.Add("predict-items-number=", (int v) => predict_items_number = v)
			.Add("num-test-users=",       (int v) => num_test_users       = v)
			.Add("rating-threshold=",    (float v)  => rating_threshold = v)
			.Add("file-format=",         (ItemDataFileFormat v) => file_format = v)
			.Add("user-prediction",      v => user_prediction   = v != null)
			.Add("repeated-items",       v => repeated_items    = v != null)
			.Add("overlap-items",        v => overlap_items     = v != null)
			.Add("all-items",            v => all_items         = v != null)
			.Add("in-training-items",    v => in_training_items = v != null)
			.Add("in-test-items",        v => in_test_items     = v != null);
	}

	protected override void SetupRecommender()
	{
		if (load_model_file != null)
			recommender = Model.Load(load_model_file);
		else if (method != null)
			recommender = method.CreateItemRecommender();
		else
			recommender = "MostPopular".CreateItemRecommender();

		base.SetupRecommender();
	}

	private void Train()
	{
		recommender.Train();
		MyMediaLite.Random.Init(); // re-init to make sure eval results are the same after training and loading
	}

	protected override void Run(string[] args)
	{
		base.Run(args);

		Console.Write(training_data.Statistics(test_data, user_attributes, item_attributes));

		bool no_eval = true;
		if (test_ratio > 0 || test_file != null)
			no_eval = false;

		TimeSpan time_span;

		if (find_iter != 0)
		{
			if ( !(recommender is IIterativeModel) )
				Abort("Only iterative recommenders (interface IIterativeModel) support --find-iter=N.");

			var iterative_recommender = recommender as IIterativeModel;
			iterative_recommender.NumIter = num_iter;
			Console.WriteLine(recommender);
			var eval_stats = new List<double>();

			if (cross_validation > 1)
			{
				var repeated_events = repeated_items ? RepeatedEvents.Yes : RepeatedEvents.No;
				recommender.DoIterativeCrossValidation(
					cross_validation,
					test_users, candidate_items, eval_item_mode, repeated_events,
					max_iter, find_iter);
			}
			else
			{
				if (load_model_file == null)
					Train();

				if (compute_fit)
					Console.WriteLine("fit: {0} iteration {1} ", ComputeFit(), iterative_recommender.NumIter);

				EvaluationResults results = null;
				if (!no_eval)
				{
					results = Evaluate();
					Console.WriteLine("{0} iteration {1}", Render(results), iterative_recommender.NumIter);
				}

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

						if (!no_eval)
						{
							t = Wrap.MeasureTime(delegate() { results = Evaluate(); });
							eval_time_stats.Add(t.TotalSeconds);
							eval_stats.Add(results[eval_measures[0]]);
							Console.WriteLine("{0} iteration {1}", Render(results), it);
						}

						Model.Save(recommender, save_model_file, it);
						Predict(prediction_file, test_users_file, it);

						if (!no_eval)
						{
							if (epsilon > 0.0 && eval_stats.Max() - results[eval_measures[0]] > epsilon)
							{
								Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} >> {1}", results[eval_measures[0]], eval_stats.Min()));
								Console.Error.WriteLine("Reached convergence on training/validation data after {0} iterations.", it);
								break;
							}
							if (results[eval_measures[0]] < cutoff)
							{
									Console.Error.WriteLine("Reached cutoff after {0} iterations.", it);
									Console.Error.WriteLine("DONE");
									break;
							}
						}
					}
				} // for
				if (max_iter % find_iter != 0)
					Predict(prediction_file, test_users_file);
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
					Console.Write(Render(results));
					no_eval = true;
				}
				else
				{
					time_span = Wrap.MeasureTime( delegate() { Train(); } );
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
						Console.Write(Render(results));
					});
				else
					time_span = Wrap.MeasureTime( delegate() { Console.Write(Render(Evaluate())); });
				Console.Write(" testing_time " + time_span);
			}
			Console.WriteLine();
		}
		Model.Save(recommender, save_model_file);
		DisplayStats();
	}

	protected override void CheckParameters(IList<string> extra_args)
	{
		base.CheckParameters(extra_args);

		if (training_file == null)
			Usage("Parameter --training-file=FILE is missing.");

		if (online_eval && !(recommender is IIncrementalItemRecommender))
			Abort(string.Format("Recommender {0} does not support incremental updates, which are necessary for an online experiment.", recommender.GetType().Name));

		if (find_iter != 0 && test_file == null && test_ratio == 0 && cross_validation == 0 && prediction_file == null && !compute_fit)
			Abort("--find-iter=N must be combined with either --test-file=FILE, --test-ratio=NUM, --cross-validation=K, --compute-fit, or --prediction-file=FILE.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && save_model_file == null && prediction_file == null)
			Usage("Please provide either --test-file=FILE, --test-ratio=NUM, --cross-validation=K, --save-model=FILE, or --prediction-file=FILE.");

		if ((candidate_items_file != null ? 1 : 0) + (all_items ? 1 : 0) + (in_training_items ? 1 : 0) + (in_test_items ? 1 : 0) + (overlap_items ? 1 : 0) > 1)
			Abort("--candidate-items=FILE, --all-items, --in-training-items, --in-test-items, and --overlap-items are mutually exclusive.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && overlap_items)
			Abort("--overlap-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && in_test_items)
			Abort("--in-test-items only makes sense with either --test-file=FILE, --test-ratio=NUM, or cross-validation=K.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && prediction_file == null && in_training_items)
			Abort("--in-training-items only makes sense with either --test-file=FILE, --test-ratio=NUM, cross-validation=K, or --prediction-file=FILE.");

		if (user_prediction)
		{
			if (recommender is IUserAttributeAwareRecommender || recommender is IItemAttributeAwareRecommender ||
			    recommender is IUserRelationAwareRecommender  || recommender is IItemRelationAwareRecommender)
				Abort("--user-prediction is not (yet) supported in combination with attribute- or relation-aware recommenders.");
		}
	}

	protected override void LoadData()
	{
		TimeSpan loading_time = Wrap.MeasureTime(delegate() {
			base.LoadData();

			// training data
			training_data = double.IsNaN(rating_threshold)
				? ItemData.Read(training_file, user_mapping, item_mapping, file_format == ItemDataFileFormat.IGNORE_FIRST_LINE)
				: ItemDataRatingThreshold.Read(training_file, rating_threshold, user_mapping, item_mapping, file_format == ItemDataFileFormat.IGNORE_FIRST_LINE);

			// test data
			if (test_ratio == 0)
			{
				if (test_file != null)
				{
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
					int random_index = MyMediaLite.Random.GetInstance().Next(old_test_users.Count - 1);
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
		return recommender.Evaluate(training_data, training_data, test_users, candidate_items, eval_item_mode, RepeatedEvents.Yes, predict_items_number);
	}

	ItemRecommendationEvaluationResults Evaluate()
	{
		var repeated_events = repeated_items ? RepeatedEvents.Yes : RepeatedEvents.No;
		return recommender.Evaluate(test_data, training_data, test_users, candidate_items, eval_item_mode, repeated_events, predict_items_number);
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
				candidate_items, predict_items_number,
				prediction_file, user_list,
				user_mapping, item_mapping,
				repeated_items);
			if (user_list != null)
				Console.Error.Write("Wrote predictions for {0} users to file {1}.", user_list.Count, prediction_file);
			else
				Console.Error.Write("Wrote predictions to file {0}.", prediction_file);
		});
		Console.WriteLine(" prediction_time " + time_span);
	}
}
