#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
from MyMediaLite import *

min_rating = 1
max_rating = 5

# load the data
user_mapping = Data.EntityMapping()
item_mapping = Data.EntityMapping()
train_data = IO.RatingPredictionData.Read("u1.base", min_rating, max_rating, user_mapping, item_mapping)
test_data = IO.RatingPredictionData.Read("u1.test", min_rating, max_rating, user_mapping, item_mapping)

# set up the recommender
recommender = RatingPrediction.UserItemBaseline()
recommender.MinRating = min_rating
recommender.MaxRating = max_rating
recommender.Ratings = train_data
recommender.Train()

# measure the accuracy on the test data set
print Eval.RatingEval.Evaluate(recommender, test_data)

# make a prediction for a certain user and item
print recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
