#!/bin/bash -e

PROGRAM="bin/rating_prediction"
DATA_DIR=data/ml-100k
LANG=C

echo "MyMediaLite random splitting test script"
echo "This will take less than 1 minute ..."

echo
echo "rating predictors"
echo "-----------------"

echo $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=GlobalAverage --data-dir=$DATA_DIR --random-seed=1 > log1
     $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=GlobalAverage --data-dir=$DATA_DIR --random-seed=1 > log1

echo $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=MatrixFactorization --data-dir=$DATA_DIR --random-seed=1 > log2
     $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=MatrixFactorization --data-dir=$DATA_DIR --random-seed=1 > log2

grep "test data" log1 > log1.grep
grep "test data" log2 > log2.grep

diff log1.grep log2.grep

rm log1 log2 log1.grep log2.grep

echo
echo "item recommenders"
echo "-----------------"

PROGRAM="bin/item_recommendation"

echo $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=MostPopular --data-dir=$DATA_DIR --random-seed=1 > log1
     $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=MostPopular --data-dir=$DATA_DIR --random-seed=1 > log1

echo $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=WRMF --data-dir=$DATA_DIR --random-seed=1 > log2
     $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=WRMF --data-dir=$DATA_DIR --random-seed=1 > log2

grep "test data" log1 > log1.grep
grep "test data" log2 > log2.grep

diff log1.grep log2.grep

rm -f log1 log2 log1.grep log2.grep

echo $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=MostPopular --data-dir=$DATA_DIR --random-seed=1 --num-test-users=100 > log1
     $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=MostPopular --data-dir=$DATA_DIR --random-seed=1 --num-test-users=100 > log1

echo $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=WRMF --data-dir=$DATA_DIR --random-seed=1 --num-test-users=100 > log2
     $PROGRAM --training-file=u.data --test-ratio=0.1 --recommender=WRMF --data-dir=$DATA_DIR --random-seed=1 --num-test-users=100 > log2

grep "test data" log1 > log1.grep
grep "test data" log2 > log2.grep

diff log1.grep log2.grep

rm -f log1 log2 log1.grep log2.grep

