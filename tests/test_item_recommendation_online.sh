#!/bin/sh -e

PROGRAM="bin/item_recommendation"

echo "MyMediaLite item recommendation online eval test script"
echo "This may take about 4 minutes ..."

echo
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=data/ml-100k

for method in MostPopular BPRMF
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --online-evaluation --num-test-users=10
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --online-evaluation --num-test-users=10
done

