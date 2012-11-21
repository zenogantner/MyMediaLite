#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
from MyMediaLite import *

# load the data
train_data = IO.ItemData.Read("u1.base")
test_data = IO.ItemData.Read("u1.test")

# set up the recommender
recommender = ItemRecommendation.UserKNN() # don't forget ()
recommender.K = 20
recommender.Feedback = train_data
recommender.Train()

# measure the accuracy on the test data set
print Eval.Items.Evaluate(recommender, test_data, train_data)

# make a prediction for a certain user and item
print recommender.Predict(1, 1)
