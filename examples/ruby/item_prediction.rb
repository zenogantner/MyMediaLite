#!/usr/bin/env ir

require 'MyMediaLite'

# load the data
user_mapping = MyMediaLite::Data::EntityMapping.new()
item_mapping = MyMediaLite::Data::EntityMapping.new()
training_data = MyMediaLite::IO::ItemRecommenderData.Read("u1.base", user_mapping, item_mapping)
relevant_items = item_mapping.InternalIDs
test_data = MyMediaLite::IO::ItemRecommenderData.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite::ItemRecommender::MostPopular.new()
recommender.SetCollaborativeData(training_data);
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::ItemPredictionEval.EvaluateItemRecommender(recommender, test_data, training_data, relevant_items)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
