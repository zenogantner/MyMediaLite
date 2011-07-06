using System;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;

public class RatingPrediction
{
	public static void Main(string[] args)
	{
		// load the data
		var user_mapping = new EntityMapping();
		var item_mapping = new EntityMapping();
		var training_data = MyMediaLite.IO.RatingPrediction.Read(args[0], user_mapping, item_mapping);
		var test_data = MyMediaLite.IO.RatingPrediction.Read(args[1], user_mapping, item_mapping);

		// set up the recommender
		var recommender = new UserItemBaseline();
		recommender.Ratings = training_data;
		recommender.Train();

		// measure the accuracy on the test data set
		var results = RatingEval.Evaluate(recommender, test_data);
		Console.WriteLine("RMSE={0} MAE={1}", results["RMSE"], results["MAE"]);

		// make a prediction for a certain user and item
		Console.WriteLine(recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1)));
	}
}
