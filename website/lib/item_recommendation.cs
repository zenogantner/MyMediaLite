using System;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;

public class ItemPrediction
{
	public static void Main(string[] args)
	{
		// load the data
		var training_data = ItemData.Read(args[0]);
		var test_data = ItemData.Read(args[1]);

		// set up the recommender
		var recommender = new MostPopular();
		recommender.Feedback = training_data;
		recommender.Train();

		// measure the accuracy on the test data set
		var results = recommender.Evaluate(test_data, training_data);
		foreach (var key in results.Keys)
			Console.WriteLine("{0}={1}", key, results[key]);
		Console.WriteLine(results);

		// make a score prediction for a certain user and item
		Console.WriteLine(recommender.Predict(1, 1));
	}
}

