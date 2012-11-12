#!/usr/bin/env ir

require 'MyMediaLite'

# load the data
train_data = MyMediaLite::IO::RatingData.Read("u1.base")
test_data = MyMediaLite::IO::RatingData.Read("u1.test")

# set up the recommender
recommender = MyMediaLite::RatingPrediction::UserItemBaseline.new()
recommender.Ratings = train_data
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::Ratings::Evaluate(recommender, test_data)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(1, 1)
