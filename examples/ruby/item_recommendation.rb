#!/usr/bin/env ir

require 'MyMediaLite'

# load the data
user_mapping = MyMediaLite::Data::EntityMapping.new()
item_mapping = MyMediaLite::Data::EntityMapping.new()
train_data = MyMediaLite::IO::ItemData.Read("u1.base", user_mapping, item_mapping)
test_users = train_data.AllUsers
candidate_items = train_data.AllItems
test_data = MyMediaLite::IO::ItemData.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite::ItemRecommendation::MostPopular.new()
recommender.Feedback = train_data;
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::Items.Evaluate(recommender, test_data, train_data, test_users, candidate_items)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
