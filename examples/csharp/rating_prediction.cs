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
		var training_data = RatingData.Read(args[0]);
		var test_data = RatingData.Read(args[1]);

		// set up the recommender
		var recommender = new UserItemBaseline();
		recommender.Ratings = training_data;
		recommender.Train();

		// measure the accuracy on the test data set
		var results = recommender.Evaluate(test_data);
		Console.WriteLine("RMSE={0} MAE={1}", results["RMSE"], results["MAE"]);
		Console.WriteLine(results);

		// make a prediction for a certain user and item
		Console.WriteLine(recommender.Predict(1, 1));
		
		var bmf = new BiasedMatrixFactorization {Ratings = training_data};
		Console.WriteLine(bmf.DoCrossValidation());
	}
}
