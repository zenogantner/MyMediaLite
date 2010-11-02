#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
import MyMediaLite

# load the data
user_mapping = MyMediaLite.data.EntityMapping()
item_mapping = MyMediaLite.data.EntityMapping()
training_data = MyMediaLite.io.ItemRecommenderData.Read("u1.base", user_mapping, item_mapping)
relevant_items = item_mapping.InternalIDs
test_data = MyMediaLite.io.ItemRecommenderData.Read("u1.test", user_mapping, item_mapping)

# set up the recommender
recommender = MyMediaLite.item_recommender.MostPopular()
recommender.SetCollaborativeData(training_data.First, training_data.Second);
recommender.Train()

# measure the accuracy on the test data set
print MyMediaLite.eval.ItemPredictionEval.EvaluateItemRecommender(recommender, test_data.First, training_data.First, relevant_items)

# make a prediction for a certain user and item
print recommender.Predict(user_mapping.ToInternalID(1), item_mapping.ToInternalID(1))
