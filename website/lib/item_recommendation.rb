#!/usr/bin/env ir

require 'MyMediaLite'
using_clr_extensions MyMediaLite

# load the data
train_data = MyMediaLite::IO::ItemData.Read("u1.base")
test_data = MyMediaLite::IO::ItemData.Read("u1.test")

# set up the recommender
recommender = MyMediaLite::ItemRecommendation::MostPopular.new()
recommender.Feedback = train_data;
recommender.Train()

# measure the accuracy on the test data set
eval_results = MyMediaLite::Eval::Items.Evaluate(recommender, test_data, train_data)
eval_results.each do |entry|
	puts "#{entry}"
end

# make a prediction for a certain user and item
puts recommender.Predict(1, 1)
