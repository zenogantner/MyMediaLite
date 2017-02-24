using System;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommendation;
using MyMediaLite.RatingRecommendation;

public class UGrowRecommendations
{
	public static void Main(string[] args)
	{
		// load the data
		var training_data = ItemData.Read(args[0]);
		var test_data = ItemData.Read(args[1]);
		var personality_data = ItemData.Read(args[2]);

		// fixed order recommender
		var ugrowrecommender = new MostPopular();
		recommender.Feedback = training_data;
		recommender.Train();

		// General Top N recommender
		var topnrecommender = new MostPopular();
		topnrecommender.Feedback = training_data;
		topnrecommender.Train();

		// General Top N recommender
		var BPRMFrecommender = new BPRMF();
		BPRMFrecommender.Feedback = training_data;
		BPRMFecommender.Train();

		// PersonalityBasedMF
		var PersonalityBasedrecommender = new PersonalityBasedMF();
		PersonalityBasedrecommender.Feedback = training_data;
		PersonalityBasedrecommender.UserAttributes = personality_data;


		// measure the accuracy on the test data set
		var results = recommender.Evaluate(test_data, training_data);
		foreach (var key in results.Keys)
			Console.WriteLine("{0}={1}", key, results[key]);
		Console.WriteLine(results);

		// make a score prediction for a certain user and item
		Console.WriteLine(recommender.Predict(1, 1));
	}
}
