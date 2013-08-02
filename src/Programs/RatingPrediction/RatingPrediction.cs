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
using MyMediaLite.Data.Split;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.HyperParameter;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

/// <summary>Rating prediction program, see Usage() method for more information</summary>
public class RatingPrediction : CommandLineProgram<RatingPredictor>
{
	// data sets
	protected IInteractions training_data;
	protected IInteractions test_data;

	bool search_hp             = false;
	bool test_no_ratings       = false;
	string prediction_line     = "{0}\t{1}\t{2}";
	string prediction_header   = null;

	RatingFileFormat file_format = RatingFileFormat.DEFAULT;
	string chronological_split;
	double chronological_split_ratio = -1;
	DateTime chronological_split_time = DateTime.MinValue;

	protected virtual string DefaultMeasure { get { return "RMSE"; } }
	protected override ICollection<string> Measures { get { return MyMediaLite.Eval.Ratings.Measures; } }

	public RatingPrediction()
	{
		eval_measures = RatingPredictionEvaluationResults.DefaultMeasuresToShow;
		cutoff = double.MaxValue;
	}

	protected override void ShowVersion()
	{
		ShowVersion(
			"Rating Prediction",
			"Copyright (C) 2011, 2012, 2013 Zeno Gantner\nCopyright (C) 2010 Zeno Gantner, Steffen Rendle"
		);
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
			Console.WriteLine(string.Join("\n   - ", "MyMediaLite.RatingPrediction".ListRecommenders()));

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
   --test-no-ratings                      test data contains no rating column
                                          (needs both --prediction-file=FILE and --test-file=FILE)
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
                                  item ID, and predicted rating; default is {0}\t{1}\t{2};
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
   --measures=LIST                     comma- or space-separated list of evaluation measures to display (default is RMSE, MAE, CBD)
                                       use --help-measures to get a list of all available measures

  options for finding the right number of iterations (iterative methods)
   --find-iter=N                  give out statistics every N iterations
   --num-iter=N                   start measuring at N iterations
   --max-iter=N                   perform at most N iterations
   --epsilon=NUM                  abort iterations if main evaluation measure is more than best result plus NUM
   --cutoff=NUM                   abort if main evaluation measure is above NUM
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
			.Add("chronological-split=", v              => chronological_split  = v)
			.Add("file-format=",         (RatingFileFormat v) => file_format    = v)
			.Add("search-hp",            v => search_hp         = v != null)
			.Add("test-no-ratings",      v => test_no_ratings   = v != null);
	}

	protected override void SetupRecommender()
	{
		if (load_model_file != null)
			recommender = (RatingPredictor) Model.Load(load_model_file);
		else if (method != null)
			recommender = method.CreateRatingPredictor();
		else
			recommender = "BiasedMatrixFactorization".CreateRatingPredictor();

		base.SetupRecommender();
	}

	protected override void Run(string[] args)
	{
		base.Run(args);

		bool do_eval = false;
		if (test_ratio > 0 || chronological_split != null)
			do_eval = true;
		if (test_file != null && !test_no_ratings)
			do_eval = true;

		Console.Error.WriteLine(
			string.Format(CultureInfo.InvariantCulture,
			"ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));

		if (test_ratio > 0)
		{
			var split = new SimpleSplit(training_data, test_ratio);
			training_data = split.Train[0];
			test_data = split.Test[0];
			recommender.Interactions = training_data;
			Console.Error.WriteLine(string.Format( CultureInfo.InvariantCulture, "test ratio {0}", test_ratio));
		}
		if (chronological_split != null)
		{
			var split = chronological_split_ratio != -1
							? new ChronologicalSplit(training_data, chronological_split_ratio)
							: new ChronologicalSplit(training_data, chronological_split_time);
			training_data = split.Train[0];
			test_data = split.Test[0];
			recommender.Interactions = training_data;
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

			var iterative_recommender = recommender as IIterativeModel;
			iterative_recommender.NumIter = num_iter;
			Console.WriteLine(recommender);

			if (cross_validation > 1)
			{
				recommender.DoIterativeCrossValidation(cross_validation, max_iter, find_iter);
			}
			else
			{
				var eval_stats = new List<double>();

				if (load_model_file == null)
					recommender.Train();

				if (compute_fit)
					Console.WriteLine("fit {0} iteration {1}", Render(recommender.Evaluate(training_data)), iterative_recommender.NumIter);

				Console.WriteLine("{0} iteration {1}", Render(Evaluate()), iterative_recommender.NumIter);

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

						EvaluationResults results = null;
						time = Wrap.MeasureTime(delegate() { results = Evaluate(); });
						eval_time_stats.Add(time.TotalSeconds);
						eval_stats.Add(results[eval_measures[0]]);
						Console.WriteLine("{0} iteration {1}", Render(results), it);

						Model.Save(recommender, save_model_file, it);
						if (prediction_file != null)
							recommender.WritePredictions(test_data, prediction_file + "-it-" + it, user_mapping, item_mapping, prediction_line, prediction_header);

						if (epsilon > 0.0 && results[eval_measures[0]] - eval_stats.Min() > epsilon)
						{
							Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} >> {1}", results[eval_measures[0]], eval_stats.Min()));
							Console.Error.WriteLine("Reached convergence on training/validation data after {0} iterations.", it);
							break;
						}
						if (results[eval_measures[0]] > cutoff)
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
					var results = DoCrossValidation();
					Console.Write(Render(results));
					do_eval = false;
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

			if (do_eval)
			{
				/*
				if (online_eval)
					seconds = Wrap.MeasureTime(delegate() { Console.Write(Render(recommender.EvaluateOnline(test_data))); });
				else */
					seconds = Wrap.MeasureTime(delegate() { Console.Write(Render(Evaluate())); });

				Console.Write(" testing_time " + seconds);

				if (compute_fit)
				{
					Console.Write("\nfit ");
					seconds = Wrap.MeasureTime(delegate() {
						Console.Write(Render(recommender.Evaluate(training_data)));
					});
					Console.Write(" fit_time " + seconds);
				}
			}

			if (prediction_file != null)
			{
				Console.WriteLine();
				seconds = Wrap.MeasureTime(delegate() {
					recommender.WritePredictions(test_data, prediction_file, user_mapping, item_mapping, prediction_line, prediction_header);
				});
				Console.Error.WriteLine("prediction_time " + seconds);
			}

			Console.WriteLine();
		}
		Model.Save(recommender, save_model_file);
		DisplayStats();
	}

	protected override void CheckParameters(IList<string> extra_args)
	{
		base.CheckParameters(extra_args);

		//if (online_eval && !(recommender is IIncrementalRatingPredictor))
		//	Abort(string.Format("Recommender {0} does not support incremental updates, which are necessary for an online experiment.", recommender.GetType().Name));

		if (training_file == null && load_model_file == null)
			Abort("Please provide either --training-file=FILE or --load-model=FILE.");

		if (test_file == null && test_ratio == 0 && cross_validation == 0 && save_model_file == null && chronological_split == null)
			Abort("Please provide either --test-file=FILE, --test-ratio=NUM, --cross-validation=K, --chronological-split=NUM|DATETIME, or --save-model=FILE.");

		if (test_no_ratings && prediction_file == null)
			Abort("--test-no-ratings needs both --prediction-file=FILE and --test-file=FILE.");

		if (prediction_file != null && test_file == null)
			Abort("--prediction-file=FILE needs --test-file=FILE");

		if (find_iter != 0 && test_file == null && test_ratio == 0 && cross_validation == 0 && prediction_file == null && chronological_split == null && !compute_fit)
			Abort("--find-iter=N must be combined with either --test-file=FILE, --test-ratio=NUM, --cross-validation=K, --chronological-split=NUM|DATETIME, --compute-fit, or --prediction-file=FILE.");

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

	protected override void LoadData()
	{
		TimeSpan loading_time = Wrap.MeasureTime(delegate() {
			base.LoadData();

			// read training data
			training_data = Interactions.FromFile(training_file, user_mapping, item_mapping);
			if (training_data == null)
				Console.WriteLine("hey");
			else
				Console.WriteLine(training_data.Count);
			recommender.Interactions = training_data;

			// read test data
			if (test_file != null)
			{
				test_data = Interactions.FromFile(test_file, user_mapping, item_mapping);

				if (recommender is ITransductiveRatingPredictor)
					((ITransductiveRatingPredictor) recommender).AdditionalInteractions = test_data;
			}
		});
		Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "loading_time {0:0.##}", loading_time.TotalSeconds));
		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}

	protected virtual EvaluationResults Evaluate()
	{
		return recommender.Evaluate(test_data);
	}

	protected virtual EvaluationResults DoCrossValidation()
	{
		return recommender.DoCrossValidation(cross_validation, compute_fit, true);
	}
}
