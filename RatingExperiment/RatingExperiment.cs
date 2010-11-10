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


namespace MyMediaLite
{
	class MainClass
	{
		public static void Usage(string message)
		{
			Console.WriteLine(message);
			Console.WriteLine();
			Usage(-1);
		}

		public static void Usage(int exit_code)
		{
			Console.WriteLine("MyMediaLite rating prediction experimental toolbox; usage:");
			Console.WriteLine(" RatingPrediction.exe DATASET [ARGUMENTS] [OPTIONS]");
			Console.WriteLine("    - use '-' for either TRAINING_FILE or TEST_FILE to read the data from STDIN");
			Console.WriteLine("  - methods (plus arguments and their defaults):");
			Console.WriteLine("    - " + mf);
			Console.WriteLine("    - " + bmf);
			Console.WriteLine("    - " + uknn_p);
			Console.WriteLine("    - " + uknn_c);
			Console.WriteLine("    - " + iknn_p);
			Console.WriteLine("    - " + iknn_c);
			Console.WriteLine("    - " + iaknn + " (needs item_attributes)");
			Console.WriteLine("    - " + uib);
			Console.WriteLine("    - " + ga);
			Console.WriteLine("    - " + ua);
			Console.WriteLine("    - " + ia);
			Console.WriteLine("  - method ARGUMENTS have the form name=value");
			Console.WriteLine("  - general OPTIONS have the form name=value");
			Console.WriteLine("    - option_file=FILE           read options from FILE (line format KEY: VALUE)");
			Console.WriteLine("    - random_seed=N              random seed for each experiment");
			Console.WriteLine("    - data_dir=DIR               load all files from DIR");
			Console.WriteLine("    - item_attributes=FILE       file containing item attribute information");
			Console.WriteLine("    - min_rating=NUM             ");
			Console.WriteLine("    - max_rating=NUM             ");
			Console.WriteLine("    - predict_ratings_file=FILE  write the rating predictions to STDOUT");

			Environment.Exit(exit_code);
		}
		
		public static void Main (string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Handlers.UnhandledExceptionHandler);

			// check number of command line parameters
			if (args.Length < 2)
				Usage("Not enough arguments.");

			// read command line parameters
			string data_file = args[0];
			string method    = args[1];

			CommandLineParameters parameters = null;
			try	{ parameters = new CommandLineParameters(args, 2);	}
			catch (ArgumentException e) { Usage(e.Message);			}

			// collaborative data characteristics
			double min_rating           = parameters.GetRemoveDouble( "min_rating",  1);
			double max_rating           = parameters.GetRemoveDouble( "max_rating",  5);

			// other arguments
			string data_dir             = parameters.GetRemoveString( "data_dir");
			string user_attributes_file = parameters.GetRemoveString( "user_attributes");
			string item_attributes_file = parameters.GetRemoveString( "item_attributes");
			int random_seed             = parameters.GetRemoveInt32(  "random_seed",  -1);
			string predict_ratings_file = parameters.GetRemoveString( "predict_ratings_file");

			if (random_seed != -1)
				MyMediaLite.util.Random.InitInstance(random_seed);

			// set correct recommender
			MyMediaLite.rating_predictor.Memory recommender = null;
			switch (method)
			{
				case "matrix-factorization":
					recommender = InitMatrixFactorization(parameters, mf);
					break;
				case "biased-matrix-factorization":
					recommender = InitMatrixFactorization(parameters, bmf);
					break;
				case "user-knn-pearson":
				case "user-kNN-pearson":
					recommender = InitKNN(parameters, uknn_p);
					break;
				case "user-knn-cosine":
				case "user-kNN-cosine":
					recommender = InitKNN(parameters, uknn_c);
					break;
				case "item-knn-pearson":
				case "item-kNN-pearson":
					recommender = InitKNN(parameters, iknn_p);
					break;
				case "item-knn-cosine":
				case "item-kNN-cosine":
					recommender = InitKNN(parameters, iknn_c);
					break;
				case "item-attribute-knn":
				case "item-attribute-kNN":
					recommender = InitKNN(parameters, iaknn);
					break;
				case "user-item-baseline":
					recommender = InitUIB(parameters);
					break;
				case "global-average":
					recommender = ga;
					break;
				case "user-average":
					recommender = ua;
					break;
				case "item-average":
					recommender = ia;
					break;
				default:
					Usage(string.Format("Unknown method: '{0}'", method));
					break;
			}

			recommender.MinRatingValue = min_rating;
			recommender.MaxRatingValue = max_rating;
			Console.Error.WriteLine("ratings range: [{0}, {1}]", recommender.MinRatingValue, recommender.MaxRatingValue);

			// check command-line parameters
			if (parameters.CheckForLeftovers())
				Usage(-1);
			if (training_file.Equals("-") && testfile.Equals("-"))
				Usage("Either training or test data, not both, can be read from STDIN.");

			// ID mapping objects
			EntityMapping user_mapping = new EntityMapping();
			EntityMapping item_mapping = new EntityMapping();

			// read data
			RatingData data = RatingPredictionData.Read(Path.Combine(data_dir, training_file), min_rating, max_rating, user_mapping, item_mapping);

			// user attributes
			if (recommender is UserAttributeAwareRecommender)
				if (user_attributes_file.Equals(string.Empty))
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
				if (item_attributes_file.Equals(string.Empty) )
				{
					Usage("Recommender expects item_attributes.");
				}
				else
				{
					Pair<SparseBooleanMatrix, int> attr_data = AttributeData.Read(Path.Combine(data_dir, item_attributes_file), item_mapping);
					((ItemAttributeAwareRecommender)recommender).ItemAttributes    = attr_data.First;
					((ItemAttributeAwareRecommender)recommender).NumItemAttributes = attr_data.Second;
				}

			// leave-one-out evaluation
			
			recommender.Ratings = training_data;
			
			TimeSpan seconds = Utils.MeasureTime(
			    	delegate()
				    {
						DisplayResults(RatingEval.EvaluateRated(recommender, test_data));
					}
			);
			Console.Write(" eval_time " + seconds);

			Console.WriteLine();
		}
	}
}

