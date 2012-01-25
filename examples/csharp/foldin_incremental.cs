using System;
using System.Collections.Generic;
using System.Globalization;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public class FoldInAndIncrementalTraining
{
	public static void Main(string[] args)
	{
		// TODO user-wise leave-one out
		// TODO use entirely new users
		// TODO make recommender type configurable
		// TODO report per-user times

		// load the data
		var training_data = RatingData.Read(args[0]);
		var test_data = RatingData.Read(args[1]);
		var update_indices = new List<int>();
		var eval_indices = new List<int>();
		foreach (int user_id in test_data.AllUsers)
			if (test_data.ByUser[user_id].Count > 1)
			{
				var user_indices = test_data.ByUser[user_id];
				for (int i = 0; i < user_indices.Count - 1; i++)
					update_indices.Add(user_indices[i]);
				for (int i = user_indices.Count - 1; i < user_indices.Count; i++)
					eval_indices.Add(user_indices[i]);
			}

		var update_data = new RatingsProxy(test_data, update_indices);
		var eval_data   = new RatingsProxy(test_data, eval_indices);

		Console.Write(training_data.Statistics());
		Console.Write(update_data.Statistics());
        Console.Write(eval_data.Statistics());

		// I. complete retraining
		// Ia. prepare
		var recommender = new MatrixFactorization();
		recommender.Ratings = new CombinedRatings(training_data, update_data);
		Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "ratings range: [{0}, {1}]", recommender.MinRating, recommender.MaxRating));
		Console.WriteLine("recommender: {0}", recommender);
		recommender.Train();
		// Ib. evaluate
		Console.WriteLine("complete training: {0}", recommender.EvaluateFoldInCompleteRetraining(update_data, eval_data));

		// II. online updates
		// IIa. prepare
		recommender = new MatrixFactorization();
		recommender.Ratings = training_data;
		recommender.Train();
		// Ib. train and evaluate
		Console.WriteLine("incremental training: {0}", recommender.EvaluateFoldInIncrementalTraining(update_data, eval_data));

		// III. fold-in
		// IIIa. prepare
		var foldin_recommender = new MatrixFactorization();
		training_data = RatingData.Read(args[0]);
		test_data = RatingData.Read(args[1]);
		update_data = new RatingsProxy(test_data, update_indices);
		eval_data   = new RatingsProxy(test_data, eval_indices);
		foldin_recommender.Ratings = training_data;
		Console.Write("normal training ... ");
		foldin_recommender.Train();
		Console.WriteLine("done.");
		// IIIb. fold-in evaluate
		Console.Write("fold-in: ");
		Console.WriteLine(foldin_recommender.EvaluateFoldIn(update_data, eval_data));
	}
}
