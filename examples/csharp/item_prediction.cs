using System;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.Eval;
using MyMediaLite.IO;
using MyMediaLite.ItemRecommender;

namespace ItemPrediction
{
	public class ItemPrediction
	{
		public static void Main(string[] args)
		{
			// load the data
			var user_mapping = new EntityMapping();
			var item_mapping = new EntityMapping();
			var training_data = ItemRecommenderData.Read(args[0], user_mapping, item_mapping);
			var relevant_items = item_mapping.InternalIDs;
			var test_data = ItemRecommenderData.Read(args[1], user_mapping, item_mapping);

			// set up the recommender
			var recommender = new MostPopular();
			recommender.SetCollaborativeData(training_data);
			recommender.Train();

			// measure the accuracy on the test data set
			var results = ItemPredictionEval.Evaluate(recommender, test_data, training_data, relevant_items);
			Console.WriteLine("AUC={0}", results["AUC"]);

			// make a prediction for a certain user and item
			Console.WriteLine(recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1)));
		}
	}
}
