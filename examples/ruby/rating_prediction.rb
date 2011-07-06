#!/usr/bin/env ir

require 'MyMediaLite'

# load the data
user_mapping = MyMediaLite::Data::EntityMapping.new()
item_mapping = MyMediaLite::Data::EntityMapping.new()

train_data = MyMediaLite::IO::RatingPrediction.Read("u1.base", user_mapping, item_mapping)
test_data = MyMediaLite::IO::RatingPrediction.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite::RatingPrediction::UserItemBaseline.new()
recommender.Ratings = train_data
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::RatingEval::Evaluate(recommender, test_data)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
