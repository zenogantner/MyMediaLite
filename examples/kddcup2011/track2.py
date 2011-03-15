#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
clr.AddReference("MyMediaLiteExperimental.dll")
from MyMediaLite import *

min_rating = 0;
max_rating = 100;

train_file = "trainIdx2.firstLines.txt"
test_file  = "testIdx2.firstLines.txt"

# load the data
train_data = IO.KDDCup2011.Ratings.Read(train_file)
test_data = IO.KDDCup2011.Track2Candidates.Read(test_file)
item_relations = IO.KDDCup2011.Items.ReadTrack2("trackData2.txt", "albumData2.txt", "artistData2.txt", "genreData2.txt");
print item_relations

# set up the recommender
recommender = RatingPrediction.UserItemBaseline()
recommender.MinRating = min_rating
recommender.MaxRating = max_rating
recommender.Ratings = train_data
print "Training ..."
recommender.Train()
print "done."

# predict on test data set
Eval.KDDCup.PredictTrack2(test_data, recommender, "output.txt")
