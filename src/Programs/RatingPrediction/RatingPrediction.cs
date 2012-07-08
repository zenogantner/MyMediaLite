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
using MyMediaLite.HyperParameter;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

/// <summary>Rating prediction program, see Usage() method for more information</summary>
public class RatingPrediction : CommandLineProgram<RatingPredictor>
{
	// data sets
	IRatings training_data;
	IRatings test_data;

	IList<int> test_users;
	IList<int> candidate_items;
	CandidateItems eval_item_mode = CandidateItems.UNION;

	string test_users_file;
	string candidate_items_file;
	bool overlap_items;
	bool in_training_items;
	bool in_test_items;
	bool all_items;

	bool search_hp             = false;
	string prediction_line     = "{0}\t{1}\t{2}";
	string prediction_header   = null;

	bool ranking_eval;
	RatingFileFormat file_format = RatingFileFormat.DEFAULT;
	RatingType rating_type       = RatingType.FLOAT;
	string chronological_split;
	double chronological_split_ratio = -1;
	DateTime chronological_split_time = DateTime.MinValue;
	bool online_eval   = false;

	protected virtual string DefaultMeasure { get { return "RMSE"; } }

	public RatingPrediction()
	{
		measure = "RMSE";
	}

	protected override void ShowVersion()
	{
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite Rating Prediction {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine("Copyright (C) 2011, 2012 Zeno Gantner");
		Console.WriteLine("Copyright (C) 2010 Zeno Gantner, Steffen Rendle");
		Console.WriteLine("This is free software; see the source for copying conditions.  There is NO");
		Console.WriteLine("warranty; not even for MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.");
		Environment.Exit(0);
	}

	// TODO generalize
	protected override void Usage(int exit_code)
	{
		var version = Assembly.GetEntryAssembly().GetName().Version;
		Console.WriteLine("MyMediaLite rating prediction {0}.{1:00}", version.Major, version.Minor);
		Console.WriteLine(@"
 usage:  rating_prediction --training-file=FILE --recommender=METHOD [OPTIONS]

  recommenders (plus options and their defaults):");

			Console.Write("   - ");
			Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.RatingPrediction")));

			Console.WriteLine(@"  method ARGUMENTS have the form name=value

  general OPTIONS:
   --recommender=METHOD             set recommender method (default BiasedMatrixFactorization)
   --recommender-options=OPTIONS    use OPTIONS as recommender options
   --help                           display this usage information and exit
   --version                        display version information and exit
   --random-seed=N                  initialize random number generator with N
   --rating-type=float|byte         store ratings internally as floats (default) or bytes
   --no-id-mapping                  do not map user and item IDs to internal IDs, keep original IDs

  files:
   --training-file=FILE                   read training data from FILE
   --test-file=FILE                       read test data from FILE
   --file-format=movielens_1m|kddcup_2011|ignore_first_line|default
   --data-dir=DIR                         load all files from DIR
   --user-attributes=FILE                 file with user attribute information, 1 tuple per line
   --item-attributes=FILE                 file with item attribute information, 1 tuple per line
   --user-relations=FILE                  file with user relation information, 1 tuple per line
   --item-relations=FILE                  file with item relation information, 1 tuple per line
   --save-model=FILE                      save computed model to FILE
   --load-model=FILE                      load model from FILE
   --save-user-mapping=FILE               save user ID mapping to FILE
   --save-item-mapping=FILE               save item ID mapping to FILE
   --load-user-mapping=FILE               load user ID mapping from FILE
   --load-item-mapping=FILE               load item ID mapping from FILE

  prediction options:
   --prediction-file=FILE         write the rating predictions to FILE
   --prediction-line=FORMAT       format of the prediction line; {0}, {1}, {2} refer to user ID,
                                  item ID, and predicted rating; default is {0}\\t{1}\\t{2};
                                  if set to 'ranking', each line contains a list of ranked items
                                  for one user
   --prediction-header=LINE       print LINE to the first line of the prediction file

  evaluation options:
   --cross-validation=K                perform k-fold cross-validation on the training data
   --test-ratio=NUM                    use a ratio of NUM of the training data for evaluation (simple split)
   --chronological-split=NUM|DATETIME  use the last ratio of NUM of the training data ratings for evaluation,
                                       or use the ratings from DATETIME on for evaluation (requires time information
                                       in the training data)
   --online-evaluation                 perform online evaluation (use every tested rating for incremental training)
   --search-hp                         search for good hyperparameter values (experimental feature)
   --compute-fit                       display fit on training data
   --ranking-evaluation                compute ranking evaluation measures

  options for finding the right number of iterations (iterative methods)
   --find-iter=N                  give out statistics every N iterations
   --max-iter=N                   perform at most N iterations
   --measure=RMSE|MAE|NMAE|CBD    evaluation measure to use for the abort conditions below (default is RMSE)
   --epsilon=NUM                  abort iterations if evaluation measure is more than best result plus NUM
   --cutoff=NUM                   abort if evaluation measure is above NUM
");
		Environment.Exit(exit_code);
	}

	static void Main(string[] args)
	{
		var program = new RatingPrediction();
		program.Run(args);
	}

	protected override void SetupOptions()
	{
		options
			.Add("prediction-line=",     v              => prediction_line      = v)
			.Add("prediction-header=",   v              => prediction_header    = v)
			.Add("test-users=",          v              => test_users_file      = v)
			.Add("candidate-items=",     v              => candidate_items_file = v)
			.Add("chronological-split=", v              => chronological_split  = v)
			.Add("rating-type=",         (RatingType v) => rating_type          = v)
			.Add("file-format=",         (RatingFileFormat v) => file_format    = v)
			.Add("ranking-evaluation",   v => ranking_eval      = v != null)
			.Add("online-evaluation",    v => online_eval       = v != null)
			.Add("search-hp",            v => search_hp         = v != null)
			.Add("overlap-items",        v => overlap_items     = v != null)
			.Add("all-items",            v => all_items         = v != null)
			.Add("in-training-items",    v => in_training_items = v != null)
			.Add("in-test-items",        v => in_test_items     = v != null);
	}

	protected override void Run(string[] args)
	{
		base.Run(args);

		// ... some more command line parameter actions ...
		bool no_eval = true;
		if (test_ratio > 0 || test_file != null || chronological_split != null)
			no_eval = false;

		// set up recommender
		if (load_model_file != null)
			recommender = (RatingPredictor) Model.Load(load_model_file);
		else if (method != null)
			recommender = Recommender.CreateRatingPredictor(method);
		else
			recommender = Recommender.CreateRatingPredictor("BiasedMatrixFactorization");
		// in case something went wrong ...
		if (recommender == null && method != null)
			Usage(string.Format("Unknown rating prediction method: '{0}'", method));
		if (recommender == null && load_model_file != null)
			Abort(string.Format("Could not load model from file {0}.", load_model_file));


		recommender.Configure(recommender_options, (string m) => { Console.Error.WriteLine(m); Environment.Exit(-1); });

		// ID mapping objects
		if (file_format == RatingFileFormat.KDDCUP_2011 || no_id_mapping)
		{
			user_mapping = new IdentityMapping();
			item_mapping = new IdentityMapping();
		}
		if (load_user_mapping_file != null)
			user_mapping = EntityMappingExtensions.LoadMapping(load_user_mapping_file);
		if (load_item_mapping_file != null)
			item_mapping = EntityMappingExtensions.LoadMapping(load_item_mapping_file);

		// load all the data
		LoadData(!online_eval);

		// if requested, save ID mappings
		if (save_user_mapping_file != null)
			user_mapping.SaveMapping(save_user_mapping_file);
		if (save_item_mapping_file != null)
			item_mapping.SaveMapping(save_item_mapping_file);

		Console.Error.WriteLine(
			string.Format(CultureInfo.InvariantCulture,
			"ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));

		if (test_ratio > 0)
		{
			var split = new RatingsSimpleSplit(training_data, test_ratio);
			recommender.Ratings = training_data = split.Train[0];
			test_data = split.Test[0];
			Console.Error.WriteLine(string.Format( CultureInfo.InvariantCulture, "test ratio {0}", test_ratio));
		}
		if (chronological_split != null)
		{
			var split = chronological_split_ratio != -1
							? new RatingsChronologicalSplit((ITimedRatings) training_data, chronological_split_ratio)
							: new RatingsChronologicalSplit((ITimedRatings) training_data, chronological_split_time);
			recommender.Ratings = training_data = split.Train[0];
			test_data = split.Test[0];
			if (test_ratio != -1)
				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "test ratio (chronological) {0}", chronological_split_ratio));
			else
				Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "split time {0}", chronological_split_time));
		}

		Console.Write(training_data.Statistics(test_data, user_attributes, item_attributes));

		if (find_iter != 0)
		{
			if ( !(recommender is IIterativeModel) )
				Abort("Only iterative recommenders (interface IIterativeModel) support --find-iter=N.");

			Console.WriteLine(recommender.ToString());

			if (cross_validation > 1)
			{
				recommender.DoIterativeCrossValidation(cross_validation, max_iter, find_iter);
			}
			else
			{
				var iterative_recommender = (IIterativeModel) recommender;
				var eval_stats = new List<double>();

				if (load_model_file == null)
					recommender.Train();

				if (compute_fit)
					Console.WriteLine("fit {0} iteration {1}", recommender.Evaluate(training_data), iterative_recommender.NumIter);

				Console.WriteLine("{0} iteration {1}", Evaluate(), iterative_recommender.NumIter);

				for (int it = (int) iterative_recommender.NumIter + 1; it <= max_iter; it++)
				{
					TimeSpan time = Wrap.MeasureTime(delegate() {
						iterative_recommender.Iterate();
					});
					training_time_stats.Add(time.TotalSeconds);

					if (it % find_iter == 0)
					{
						if (compute_fit)
						{
							time = Wrap.MeasureTime(delegate() {
								Console.WriteLine("fit {0} iteration {1}", recommender.Evaluate(training_data), it);
							});
							fit_time_stats.Add(time.TotalSeconds);
						}

						Dictionary<string, float> results = null;
						time = Wrap.MeasureTime(delegate() { results = Evaluate(); });
						eval_time_stats.Add(time.TotalSeconds);
						eval_stats.Add(results[measure]);
						Console.WriteLine("{0} iteration {1}", results, it);

						Model.Save(recommender, save_model_file, it);
						if (prediction_file != null)
							recommender.WritePredictions(test_data, prediction_file + "-it-" + it, user_mapping, item_mapping, prediction_line, prediction_header);

						if (epsilon > 0.0 && results[measure] - eval_stats.Min() > epsilon)
						{
							Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} >> {1}", results["RMSE"], eval_stats.Min()));
							Console.Error.WriteLine("Reached convergence on training/validation data after {0} iterations.", it);
							break;
						}
						if (results[measure] > cutoff)
						{
							Console.Error.WriteLine("Reached cutoff after {0} iterations.", it);
							break;
						}
					}
				} // for
			}
		}
		else
		{
			TimeSpan seconds;

			Console.Write(recommender + " ");

			if (load_model_file == null)
			{
				if (cross_validation > 1)
				{
					Console.WriteLine();
					var results = recommender.DoCrossValidation(cross_validation, compute_fit, true);
					Console.Write(results);
					no_eval = true;
				}
				else
				{
					if (search_hp)
					{
						double result = NelderMead.FindMinimum("RMSE", recommender);
						Console.Error.WriteLine("estimated quality (on split) {0}", result.ToString(CultureInfo.InvariantCulture));
					}

					seconds = Wrap.MeasureTime( delegate() { recommender.Train(); } );
					Console.Write(" training_time " + seconds + " ");
				}
			}

			if (!no_eval)
			{
				if (online_eval) // TODO also support ranking for this
					seconds = Wrap.MeasureTime(delegate() { Console.Write(recommender.EvaluateOnline(test_data)); });
				else
					seconds = Wrap.MeasureTime(delegate() { Console.Write(Evaluate()); });

				Console.Write(" testing_time " + seconds);

				if (compute_fit)
				{
					Console.Write("\nfit ");
					seconds = Wrap.MeasureTime(delegate() {
						Console.Write(recommender.Evaluate(training_data));
					});
					Console.Write(" fit_time " + seconds);
				}

				if (prediction_file != null)
				{
					Console.WriteLine();
					seconds = Wrap.MeasureTime(delegate() {
						recommender.WritePredictions(test_data, prediction_file, user_mapping, item_mapping, prediction_line, prediction_header);
					});
					Console.Error.Write("prediction_time " + seconds);
				}
			}

			Console.WriteLine();
		}
		Model.Save(recommender, save_model_file);
		DisplayStats();
	}

	protected override void CheckParameters(IList<string> extra_args)
	{
		base.CheckParameters(extra_args);
		
		if (online_eval && !(recommender is IIncrementalRatingPredictor))
			Abort(string.Format("Recommender {0} does not support incremental updates, which are necessary for an online experiment.", recommender.GetType().Name));

		if (training_file == null && load_model_file == null)
			Usage("Please provide either --training-file=FILE or --load-model=FILE.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && save_model_file == null && chronological_split == null)
			Usage("Please provide either test-file=FILE, --test-ratio=NUM, --cross-validation=K, --chronological-split=NUM|DATETIME, or --save-model=FILE.");

		// handling of --chronological-split
		if (chronological_split != null)
		{
			try
			{
				chronological_split_ratio = double.Parse(chronological_split, CultureInfo.InvariantCulture);
			}
			catch { }
			if (chronological_split_ratio == -1)
				try
				{
					chronological_split_time = DateTime.Parse(chronological_split, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					Abort(string.Format("Could not interpret argument of --chronological-split as number or date and time: '{0}'", chronological_split));
				}

			// check for conflicts
			if (cross_validation > 1)
				Abort("--cross-validation=K and --chronological-split=NUM|DATETIME are mutually exclusive.");

			if (test_ratio > 1)
				Abort("--test-ratio=NUM and --chronological-split=NUM|DATETIME are mutually exclusive.");
		}
	}

	void LoadData(bool static_data)
	{
		training_file = Path.Combine(data_dir, training_file);

		TimeSpan loading_time = Wrap.MeasureTime(delegate() {
			// read training data
			if ((recommender is TimeAwareRatingPredictor || chronological_split != null) && file_format != RatingFileFormat.MOVIELENS_1M)
			{
				training_data = TimedRatingData.Read(training_file, user_mapping, item_mapping);
			}
			else
			{
				if (file_format == RatingFileFormat.DEFAULT)
					training_data = static_data
						? StaticRatingData.Read(training_file, user_mapping, item_mapping, rating_type)
						: RatingData.Read(training_file, user_mapping, item_mapping);
				else if(file_format == RatingFileFormat.IGNORE_FIRST_LINE)
					training_data = static_data
						? StaticRatingData.Read(training_file, user_mapping, item_mapping, rating_type, true)
						: RatingData.Read(training_file, user_mapping, item_mapping, true);
				else if (file_format == RatingFileFormat.MOVIELENS_1M)
					training_data = MovieLensRatingData.Read(training_file, user_mapping, item_mapping);
				else if (file_format == RatingFileFormat.KDDCUP_2011)
					training_data = MyMediaLite.IO.KDDCup2011.Ratings.Read(training_file);
			}
			recommender.Ratings = training_data;

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

			// read test data
			if (test_file != null)
			{
				test_file = Path.Combine(data_dir, test_file);

				if (recommender is TimeAwareRatingPredictor && file_format != RatingFileFormat.MOVIELENS_1M)
					test_data = TimedRatingData.Read(test_file, user_mapping, item_mapping);
				else if (file_format == RatingFileFormat.MOVIELENS_1M)
					test_data = MovieLensRatingData.Read(test_file, user_mapping, item_mapping);
				else if (file_format == RatingFileFormat.KDDCUP_2011)
					test_data = MyMediaLite.IO.KDDCup2011.Ratings.Read(test_file);
				else
					test_data = StaticRatingData.Read(test_file, user_mapping, item_mapping, rating_type, file_format == RatingFileFormat.IGNORE_FIRST_LINE);

				if (recommender is ITransductiveRatingPredictor)
					((ITransductiveRatingPredictor) recommender).AdditionalFeedback = test_data;
			}

			// test users
			if (test_users_file != null)
				test_users = user_mapping.ToInternalID( File.ReadLines(Path.Combine(data_dir, test_users_file)).ToArray() );
			else
				test_users = test_data != null ? test_data.AllUsers : training_data.AllUsers;

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
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loading_time {0:0.##}", loading_time.TotalSeconds));
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}

	Dictionary<string, float> Evaluate()
	{
		if (ranking_eval)
		{
			// TODO make more configurable
			bool repeat_eval = false;
			int predict_items_number = -1;
			var test_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(test_data);
			var training_data_posonly = new PosOnlyFeedback<SparseBooleanMatrix>(training_data);
			return recommender.Evaluate(test_data_posonly, training_data_posonly,
			                            test_users, candidate_items,
			                            eval_item_mode, repeat_eval, predict_items_number);
		}
		else
		{
			return recommender.Evaluate(test_data, training_data);
		}
	}
}
