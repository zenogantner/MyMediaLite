#!/bin/sh -e

PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 2 minutes ..."

echo ""
echo "MovieLens 100k"
echo "--------------"

DATA_DIR=../../../../data/ml100k


cd src/RatingPrediction/bin/Debug/

for method in MatrixFactorization BiasedMatrixFactorization UserItemBaseline
do
       echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=10" --data-dir=$DATA_DIR --online-evaluation
            $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=10" --data-dir=$DATA_DIR --online-evaluation
done


cd ../../../../
