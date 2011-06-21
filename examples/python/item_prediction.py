#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
from MyMediaLite import *

# load the data
user_mapping = Data.EntityMapping()
item_mapping = Data.EntityMapping()
train_data = IO.ItemRecommendation.Read("u1.base", user_mapping, item_mapping)
relevant_users = train_data.AllUsers; # users that will be taken into account in the evaluation
relevant_items = train_data.AllItems; # items that will be taken into account in the evaluation
test_data = IO.ItemRecommendation.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = ItemRecommendation.UserKNN()
recommender.K = 20
recommender.Feedback = train_data
recommender.Train()

# measure the accuracy on the test data set
print Eval.ItemPredictionEval.Evaluate(recommender, test_data, train_data, relevant_users, relevant_items)

# make a prediction for a certain user and item
print recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
