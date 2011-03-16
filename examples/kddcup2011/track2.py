#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
clr.AddReference("MyMediaLiteExperimental.dll")
from MyMediaLite import *

train_file = "trainIdx2.firstLines.txt"
test_file  = "testIdx2.firstLines.txt"

# load the data
train_data = IO.KDDCup2011.Ratings.Read(train_file)
test_data = IO.KDDCup2011.Track2Candidates.Read(test_file)
item_relations = IO.KDDCup2011.Items.Read("trackData2.txt", "albumData2.txt", "artistData2.txt", "genreData2.txt", 2);
print item_relations

# set up the recommender
recommender = RatingPrediction.UserItemBaseline()
recommender.MinRating = 0
recommender.MaxRating = 100
recommender.Ratings = train_data
print "Training ..."
recommender.Train()
print "done."

# predict on the test set
print "Predicting ..."
Eval.KDDCup.PredictTrack2(recommender, test_data, "track2-output.txt")
print "done."