#!/bin/sh -e

PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 7 minutes ..."

echo ""
echo "MovieLens 1M"
echo "------------"

DATA_DIR=../../../../data/ml1m


cd src/RatingPrediction/bin/Debug/
 
for method in MatrixFactorization BiasedMatrixFactorization UserItemBaseline
do
       echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --recommender-options="num_iter=10" --data-dir=$DATA_DIR --online-evaluation
            $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --recommender-options="num_iter=10" --data-dir=$DATA_DIR --online-evaluation
done

 
cd ../../../../
