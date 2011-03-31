#!/usr/bin/env ir

require 'MyMediaLite'

# load the data
user_mapping = MyMediaLite::Data::EntityMapping.new()
item_mapping = MyMediaLite::Data::EntityMapping.new()
train_data = MyMediaLite::IO::ItemRecommendation.Read("u1.base", user_mapping, item_mapping)
relevant_users = train_data.AllUsers # users that will be taken into account in the evaluation
relevant_items = train_data.AllItems # items that will be taken into account in the evaluation
test_data = MyMediaLite::IO::ItemRecommendation.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite::ItemRecommendation::MostPopular.new()
recommender.Feedback = train_data;
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::ItemPredictionEval.Evaluate(recommender, test_data, train_data, relevant_users, relevant_items)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
