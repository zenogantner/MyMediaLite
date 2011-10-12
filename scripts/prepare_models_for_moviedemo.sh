#!/bin/sh

# to be run from the root directory of MyMediaLite

mkdir data/saved_ratings
mkdir data/models

# ML1M
time mono --debug src/RatingPrediction/bin/Debug/RatingPrediction.exe data/ml-1m/ratings.txt data/ml-1m/ratings.txt BiasedMatrixFactorization bias_reg=0.001 reg=0.045 num_iter=60 save_model=data/models/ml-1m-bmf.model min_rating=1

# ML10M
time mono --debug src/RatingPrediction/bin/Debug/RatingPrediction.exe data/ml-10m/ratings.txt data/ml-10m/ratings.txt BiasedMatrixFactorization bias_reg=0.001 reg=0.045 num_iter=60 save_model=data/models/ml-10m-bmf.model min_rating=0.5
# TODO find better hyperparameters
