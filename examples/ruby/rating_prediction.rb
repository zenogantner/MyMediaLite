#!/usr/bin/env ir

require 'MyMediaLite'

min_rating = 1
max_rating = 5

# load the data
user_mapping = MyMediaLite::Data::EntityMapping.new()
item_mapping = MyMediaLite::Data::EntityMapping.new()

training_data = MyMediaLite::IO::RatingPredictionData.Read("u1.base", min_rating, max_rating, user_mapping, item_mapping)
test_data = MyMediaLite::IO::RatingPredictionData.Read("u1.test", min_rating, max_rating, user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite::RatingPrediction::UserItemBaseline.new()
recommender.MinRating = min_rating
recommender.MaxRating = max_rating
recommender.Ratings = training_data
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::RatingEval::Evaluate(recommender, test_data)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
