#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
from MyMediaLite import *

# load the data
user_mapping = Data.EntityMapping()
item_mapping = Data.EntityMapping()
train_data = IO.ItemRecommenderData.Read("u1.base", user_mapping, item_mapping)
relevant_items = train_data.NonEmptyColumnIDs;
test_data = IO.ItemRecommenderData.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = ItemRecommendation.ItemKNN()
recommender.K = 2000
recommender.SetCollaborativeData(train_data)
recommender.Train()

# measure the accuracy on the test data set
print Eval.ItemPredictionEval.Evaluate(recommender, test_data, train_data, relevant_items)

# make a prediction for a certain user and item
print recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
