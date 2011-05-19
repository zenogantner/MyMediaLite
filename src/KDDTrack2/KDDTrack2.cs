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
using System.Reflection;
using System.Text;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.IO.KDDCup2011;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

/// <summary>Rating prediction program, see Usage() method for more information</summary>
class KDDTrack2
{
	static NumberFormatInfo ni = new NumberFormatInfo();

	// data sets
	//  training
	static IRatings training_ratings;
	static IRatings complete_ratings;

	//  validation
	static IRatings validation_ratings;
	static Dictionary<int, IList<int>> validation_candidates;
	static Dictionary<int, IList<int>> validation_hits;

	//  test
	static Dictionary<int, IList<int>> test_candidates;

	// recommenders
	static ItemRecommender recommender_validate = null;
	static ItemRecommender recommender_final    = null;

	// time statistics
	static List<double> training_time_stats = new List<double>();
	static List<double> eval_time_stats     = new List<double>();
	static List<double> err_eval_stats      = new List<double>();

	// global command line parameters
	static string save_model_file;
	static string load_model_file;
	static int max_iter;
	static int find_iter;
	static double epsilon;
	static double err_cutoff;
	static string prediction_file;
	static bool predict_score;
	static bool sample_data;
	static bool predict_rated;

	static void Usage(string message)
	{
		Console.WriteLine(message);
		Console.WriteLine();
		Usage(-1);
	}

	// TODO add compute_fit again

	static void Usage(int exit_code)
	{
		Console.WriteLine(@"
MyMediaLite KDD Cup 2011 Track 2 tool

 usage:  KDDTrack2.exe METHOD [ARGUMENTS] [OPTIONS]

  use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN

  methods (plus arguments and their defaults):");

			Console.Write("   - ");
			Console.WriteLine(string.Join("\n   - ", Recommender.List("MyMediaLite.ItemRecommendation")));

			Console.WriteLine(@"method ARGUMENTS have the form name=value

  general OPTIONS have the form name=value
   - option_file=FILE           read options from FILE (line format KEY: VALUE)
   - random_seed=N              set random seed to N
   - data_dir=DIR               load all files from DIR
   - save_model=FILE            save computed model to FILE
   - load_model=FILE            load model from FILE
   - prediction_file=FILE       write the predictions to  FILE ('-' for STDOUT)
   - sample_data=BOOL           assume the sample data set instead of the real one
   - predict_score=BOOL         predict scores (double precision) instead of 0/1 decisions
   - predict_rated=BOOL         instead of predicting what received a good rating, try to predict what received a rating at all
                                (implies predict_score)

  options for finding the right number of iterations (MF methods)
   - find_iter=N                give out statistics every N iterations
   - max_iter=N                 perform at most N iterations
   - epsilon=NUM                abort iterations if error is more than best result plus NUM
   - err_cutoff=NUM             abort if error is above NUM");

		Environment.Exit(exit_code);
	}

    static void Main(string[] args)
    {
		Assembly assembly = Assembly.GetExecutingAssembly();
		Assembly.LoadFile(Path.GetDirectoryName(assembly.Location) + Path.DirectorySeparatorChar + "MyMediaLiteExperimental.dll");

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);
		Console.CancelKeyPress += new ConsoleCancelEventHandler(AbortHandler);
		ni.NumberDecimalDigits = '.';

		// check number of command line parameters
		if (args.Length < 1)
			Usage("Not enough arguments.");

		// read command line parameters
		string method = args[0];

		RecommenderParameters parameters = null;
		try	{ parameters = new RecommenderParameters(args, 1); }
		catch (ArgumentException e) { Usage(e.Message);	}

		// arguments for iteration search
		find_iter   = parameters.GetRemoveInt32(  "find_iter",   0);
		max_iter    = parameters.GetRemoveInt32(  "max_iter",    500);
		epsilon     = parameters.GetRemoveDouble( "epsilon",     1);
		err_cutoff  = parameters.GetRemoveDouble( "err_cutoff",  2);

		// data arguments
		string data_dir  = parameters.GetRemoveString( "data_dir");
		if (data_dir != string.Empty)
			data_dir = data_dir + "/mml-track2";
		else
			data_dir = "mml-track2";
		sample_data      = parameters.GetRemoveBool(   "sample_data",   false);
		predict_rated    = parameters.GetRemoveBool(   "predict_rated", false); 
		predict_score    = parameters.GetRemoveBool(   "predict_score", false); 

		// other arguments
		save_model_file  = parameters.GetRemoveString( "save_model");
		load_model_file  = parameters.GetRemoveString( "load_model");
		int random_seed  = parameters.GetRemoveInt32(  "random_seed",  -1);
		prediction_file  = parameters.GetRemoveString( "prediction_file");

		if (predict_rated)
			predict_score = true;
		
		Console.Error.WriteLine("predict_score={0}", predict_score);
		
		if (random_seed != -1)
			MyMediaLite.Util.Random.InitInstance(random_seed);

		recommender_validate = Recommender.CreateItemRecommender(method);
		if (recommender_validate == null)
			Usage(string.Format("Unknown method: '{0}'", method));

 		Recommender.Configure(recommender_validate, parameters, Usage);
		recommender_final = recommender_validate.Clone() as ItemRecommender;

		if (parameters.CheckForLeftovers())
			Usage(-1);

		// load all the data
		TimeSpan loading_time = Utils.MeasureTime(delegate() {
			LoadData(data_dir);
		});
		Console.WriteLine("loading_time {0:0.##}", loading_time.TotalSeconds.ToString(ni));

		if (load_model_file != string.Empty)
		{
			Recommender.LoadModel(recommender_validate, load_model_file + "-validate");
			Recommender.LoadModel(recommender_final,    load_model_file + "-final");
		}

		Console.Write(recommender_validate.ToString());

		DoTrack2();

		Console.Error.WriteLine("memory {0}", Memory.Usage);
	}

	static void DoTrack2()
	{
		TimeSpan seconds;

		if (find_iter != 0)
		{
			if ( !(recommender_validate is IIterativeModel) )
				Usage("Only iterative recommenders support find_iter.");

			IIterativeModel iterative_recommender_validate = (IIterativeModel) recommender_validate;
			IIterativeModel iterative_recommender_final    = (IIterativeModel) recommender_final;
			Console.WriteLine();

			if (load_model_file == string.Empty)
			{
				recommender_validate.Train(); // TODO parallelize
				if (prediction_file != string.Empty)
					recommender_final.Train();
			}

			// evaluate and display results
			double error = KDDCup.EvaluateTrack2(recommender_validate, validation_candidates, validation_hits);
			Console.WriteLine(string.Format(ni, "ERR {0:0.######} {1}", error, iterative_recommender_validate.NumIter));

			for (int i = iterative_recommender_validate.NumIter + 1; i <= max_iter; i++)
			{
				TimeSpan time = Utils.MeasureTime(delegate() {
					iterative_recommender_validate.Iterate(); // TODO parallelize
					if (prediction_file != string.Empty)
						iterative_recommender_final.Iterate();
				});
				training_time_stats.Add(time.TotalSeconds);

				if (i % find_iter == 0)
				{
					time = Utils.MeasureTime(delegate() { // TODO parallelize
						// evaluate
						error = KDDCup.EvaluateTrack2(recommender_validate, validation_candidates, validation_hits);
						err_eval_stats.Add(error);
						Console.WriteLine(string.Format(ni, "ERR {0:0.######} {1}", error, i));

						if (prediction_file != string.Empty)
						{
							if (predict_score)
							{
								KDDCup.PredictScoresTrack2(recommender_validate, validation_candidates, prediction_file + "-validate-it-" + i);
								KDDCup.PredictScoresTrack2(recommender_final, test_candidates, prediction_file + "-it-" + i);								
							}							
							else
							{
								KDDCup.PredictTrack2(recommender_validate, validation_candidates, prediction_file + "-validate-it-" + i);
								KDDCup.PredictTrack2(recommender_final, test_candidates, prediction_file + "-it-" + i);
							}
						}
					});
					eval_time_stats.Add(time.TotalSeconds);

					if (save_model_file != string.Empty)
					{
						Recommender.SaveModel(recommender_validate, save_model_file + "-validate", i);
						if (prediction_file != string.Empty)
							Recommender.SaveModel(recommender_final, save_model_file, i);
					}

					if (err_eval_stats.Last() > err_cutoff)
					{
						Console.Error.WriteLine("Reached cutoff after {0} iterations.", i);
						break;
					}

					if (err_eval_stats.Last() > err_eval_stats.Min() + epsilon)
					{
						Console.Error.WriteLine(string.Format(ni, "Reached convergence (eps={0:0.######}) on training/validation data after {1} iterations.", epsilon, i));
						break;
					}
				}
			} // for

			DisplayIterationStats();
		}
		else
		{
			if (load_model_file == string.Empty)
			{
				seconds = Utils.MeasureTime(delegate() { // TODO parallelize
					recommender_validate.Train();
					if (prediction_file != string.Empty)
						recommender_final.Train();
				});
				Console.Write(" training_time " + seconds + " ");
			}

			seconds = Utils.MeasureTime(delegate() {
					// evaluate
					double error = KDDCup.EvaluateTrack2(recommender_validate, validation_candidates, validation_hits);
					Console.Write(string.Format(ni, "ERR {0:0.######}", error));

					if (prediction_file != string.Empty)
					{
						if (predict_score)
						{
							KDDCup.PredictScoresTrack2(recommender_validate, validation_candidates, prediction_file + "-validate");
							KDDCup.PredictScoresTrack2(recommender_final, test_candidates, prediction_file);
						}						
						else
						{
							KDDCup.PredictTrack2(recommender_validate, validation_candidates, prediction_file + "-validate");
							KDDCup.PredictTrack2(recommender_final, test_candidates, prediction_file);
						}
					}
			});
			Console.Write(" evaluation_time " + seconds + " ");

			if (save_model_file != string.Empty)
			{
				Recommender.SaveModel(recommender_validate, save_model_file + "-validate");
				if (prediction_file != string.Empty)
					Recommender.SaveModel(recommender_final,    save_model_file);
			}
		}

		Console.WriteLine();
	}

    static void LoadData(string data_dir)
	{
		string training_file              = Path.Combine(data_dir, "trainIdx2.txt");
		string test_file                  = Path.Combine(data_dir, "testIdx2.txt");
		string validation_candidates_file = Path.Combine(data_dir, "validationCandidatesIdx2.txt");
		string validation_ratings_file    = Path.Combine(data_dir, "validationRatingsIdx2.txt");
		string validation_hits_file       = Path.Combine(data_dir, "validationHitsIdx2.txt");
		string track_file                 = Path.Combine(data_dir, "trackData2.txt");
		string album_file                 = Path.Combine(data_dir, "albumData2.txt");
		string artist_file                = Path.Combine(data_dir, "artistData2.txt");
		string genre_file                 = Path.Combine(data_dir, "genreData2.txt");

		if (sample_data)
		{
			training_file              = Path.Combine(data_dir, "trainIdx2.firstLines.txt");
			test_file                  = Path.Combine(data_dir, "testIdx2.firstLines.txt");
			validation_candidates_file = Path.Combine(data_dir, "validationCandidatesIdx2.firstLines.txt");
			validation_ratings_file    = Path.Combine(data_dir, "validationRatingsIdx2.firstLines.txt");
			validation_hits_file       = Path.Combine(data_dir, "validationHitsIdx2.firstLines.txt");
		}

		// read training data
		training_ratings = MyMediaLite.IO.KDDCup2011.Ratings.Read(training_file);

		// read validation data
		validation_candidates = Track2Items.Read(validation_candidates_file);
		validation_hits       = Track2Items.Read(validation_hits_file);

		if (validation_hits.Count != validation_candidates.Count)
			throw new Exception("inconsistent number of users in hits and candidates");
		validation_ratings = MyMediaLite.IO.KDDCup2011.Ratings.Read(validation_ratings_file);

		complete_ratings = new CombinedRatings(training_ratings, validation_ratings);

		// read test data
		test_candidates = Track2Items.Read(test_file);

		// read item data
		if (recommender_validate is IKDDCupRecommender)
		{
			var kddcup_recommender = recommender_validate as IKDDCupRecommender;
			kddcup_recommender.ItemInfo = Items.Read(track_file, album_file, artist_file, genre_file, 2);
		}

		// connect data and recommenders
		if (predict_rated)
		{
			recommender_validate.Feedback = CreateFeedback(training_ratings);
			recommender_final.Feedback    = CreateFeedback(complete_ratings);
		}
		else
		{
			// normal item recommenders
			recommender_validate.Feedback = CreateFeedback(training_ratings, 80);
			recommender_final.Feedback    = CreateFeedback(complete_ratings, 80);
		}
		if (recommender_validate is ISemiSupervisedItemRecommender)
		{
			// add additional data to semi-supervised models
			//   for the validation recommender
			((ISemiSupervisedItemRecommender) recommender_validate).TestUsers = new HashSet<int>(validation_candidates.Keys);
			var validation_items = new HashSet<int>();
			foreach (var l in validation_candidates.Values)
				foreach (var i in l)
					validation_items.Add(i);
			((ISemiSupervisedItemRecommender) recommender_validate).TestItems = validation_items;

			//   for the test/final recommender
			((ISemiSupervisedItemRecommender) recommender_final).TestUsers = new HashSet<int>(test_candidates.Keys);
			var test_items = new HashSet<int>();
			foreach (var l in test_candidates.Values)
				foreach (var i in l)
					test_items.Add(i);
			((ISemiSupervisedItemRecommender) recommender_final).TestItems = test_items;
		}

		Console.Error.WriteLine("memory before deleting ratings: {0}", Memory.Usage);
		training_ratings = null;
		complete_ratings = null;
		Console.Error.WriteLine("memory after deleting ratings:  {0}", Memory.Usage);

		Utils.DisplayDataStats(recommender_final.Feedback, null, recommender_final);
	}

	static PosOnlyFeedback CreateFeedback(IRatings ratings)
	{
		return CreateFeedback(ratings, 0);
	}
	
	static PosOnlyFeedback CreateFeedback(IRatings ratings, double threshold)
	{
		var feedback = new PosOnlyFeedback();

		for (int i = 0; i < ratings.Count; i++)
			if (ratings[i] >= threshold)
				feedback.Add(ratings.Users[i], ratings.Items[i]);

		Console.Error.WriteLine("{0} ratings > {1}", feedback.Count, threshold);

		return feedback;
	}	

	static void AbortHandler(object sender, ConsoleCancelEventArgs args)
	{
		DisplayIterationStats();
		Console.Error.WriteLine("memory {0}", Memory.Usage);
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
	}
}