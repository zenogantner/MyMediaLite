using System;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public class RatingPrediction
{
	public static void Main(string[] args)
	{
		double min_rating = 1;
		double max_rating = 5;
		
		// load the data
		var user_mapping = new EntityMapping();
		var item_mapping = new EntityMapping();
		var training_data = RatingPredictionData.Read(args[0], min_rating, max_rating, user_mapping, item_mapping);
		var test_data = RatingPredictionData.Read(args[1], min_rating, max_rating, user_mapping, item_mapping);

		// set up the recommender
		var recommender = new UserItemBaseline();
		recommender.MinRating = min_rating;
		recommender.MaxRating = max_rating;
		recommender.Ratings = training_data;
		recommender.Train();

		// measure the accuracy on the test data set
		var results = RatingEval.Evaluate(recommender, test_data);
		Console.WriteLine("RMSE={0} MAE={1}", results["RMSE"], results["MAE"]);

		// make a prediction for a certain user and item
		Console.WriteLine(recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1)));
	}
}
