#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
from MyMediaLite import *

# load the data
user_mapping = Data.EntityMapping()
item_mapping = Data.EntityMapping()
train_data = IO.ItemData.Read("u1.base", user_mapping, item_mapping)
test_users = train_data.AllUsers;
candidate_items = train_data.AllItems;
test_data = IO.ItemData.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = ItemRecommendation.UserKNN()
recommender.K = 20
recommender.Feedback = train_data
recommender.Train()

# measure the accuracy on the test data set
print Eval.Items.Evaluate(recommender, test_data, train_data, test_users, candidate_items)

# make a prediction for a certain user and item
print recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
