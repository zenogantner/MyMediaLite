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
		var user_mapping = new EntityMapping();
		var item_mapping = new EntityMapping();
		var training_data = ItemRecommendation.Read(args[0], user_mapping, item_mapping);
		var relevant_users = training_data.AllUsers; // users that will be taken into account in the evaluation
		var relevant_items = training_data.AllItems; // items that will be taken into account in the evaluation
		var test_data = ItemRecommendation.Read(args[1], user_mapping, item_mapping);

		// set up the recommender
		var recommender = new MostPopular();
		recommender.Feedback = training_data;
		recommender.Train();

		// measure the accuracy on the test data set
		var results = ItemPredictionEval.Evaluate(recommender, test_data, training_data, relevant_users, relevant_items);
		foreach (var key in results.Keys)
			Console.WriteLine("{0}={1}", key, results[key]);

		// make a prediction for a certain user and item
		Console.WriteLine(recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1)));
	}
}

