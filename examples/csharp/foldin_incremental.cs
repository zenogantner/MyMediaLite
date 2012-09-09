using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public class FoldInAndIncrementalTraining
{
	public static void Main(string[] args)
	{
		// TODO add random seed
		// TODO report per-user times

		string data_file = args[0];
		string method    = args[1];
		string options   = args[2];
		int num_test_users = int.Parse(args[3]);

		// load the data
		var all_data = RatingData.Read(data_file);

		// TODO randomize
		var test_users = new HashSet<int>(Enumerable.Range(0, num_test_users));

		var update_indices = new List<int>();
		var eval_indices = new List<int>();
		foreach (int user_id in test_users)
			if (all_data.ByUser[user_id].Count > 1)
			{
				var user_indices = all_data.ByUser[user_id];
				for (int i = 0; i < user_indices.Count - 1; i++)
					update_indices.Add(user_indices[i]);
				for (int i = user_indices.Count - 1; i < user_indices.Count; i++)
					eval_indices.Add(user_indices[i]);
			}

		var training_indices = new List<int>();
		for (int i = 0; i < all_data.Count; i++)
			if (!test_users.Contains(all_data.Users[i]))
				training_indices.Add(i);
		var training_data = new MyMediaLite.Data.Ratings();
		foreach (int i in training_indices)
			training_data.Add(all_data.Users[i], all_data.Items[i], all_data[i]);

		var update_data = new RatingsProxy(all_data, update_indices);
		var eval_data   = new RatingsProxy(all_data, eval_indices);

		Console.Write(training_data.Statistics());
		Console.Write(update_data.Statistics());
		Console.Write(eval_data.Statistics());

		// prepare recommender
		RatingPredictor recommender = method.CreateRatingPredictor();
		recommender.Configure(options);
		recommender.Ratings = training_data;
		Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));
		Console.WriteLine("recommender: {0}", recommender);
		recommender.Train();

		// I. complete retraining
		Console.WriteLine(
			"complete training: {0}",
			recommender.EvaluateFoldInCompleteRetraining(update_data, eval_data));

		// II. online updates
		Console.WriteLine(
			"incremental training: {0}",
			((IncrementalRatingPredictor)recommender).EvaluateFoldInIncrementalTraining(update_data, eval_data));

		// III. fold-in
		Console.WriteLine(
			"fold-in: {0}",
			((IFoldInRatingPredictor)recommender).EvaluateFoldIn(update_data, eval_data));
	}
}
