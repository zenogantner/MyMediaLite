#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
from MyMediaLite import *

# load the data
train_data = IO.RatingData.Read("u1.base")
test_data  = IO.RatingData.Read("u1.test")

# set up the recommender
recommender = RatingPrediction.UserItemBaseline() # don't forget ()
recommender.Ratings = train_data
recommender.Train()

# measure the accuracy on the test data set
print Eval.Ratings.Evaluate(recommender, test_data)

# make a prediction for a certain user and item
print recommender.Predict(1, 1)
