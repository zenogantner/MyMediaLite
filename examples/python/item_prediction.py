#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
import MyMediaLite

# load the data
user_mapping = MyMediaLite.Data.EntityMapping()
item_mapping = MyMediaLite.Data.EntityMapping()
training_data = MyMediaLite.IO.ItemRecommenderData.Read("u1.base", user_mapping, item_mapping)
relevant_items = item_mapping.InternalIDs
test_data = MyMediaLite.IO.ItemRecommenderData.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite.ItemRecommender.MostPopular()
recommender.SetCollaborativeData(training_data);
recommender.Train()

# measure the accuracy on the test data set
print MyMediaLite.Eval.ItemPredictionEval.Evaluate(recommender, test_data, training_data, relevant_items)

# make a prediction for a certain user and item
print recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
