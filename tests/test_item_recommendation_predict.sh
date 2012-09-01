#!/bin/bash -e

PROGRAM="bin/item_recommendation"
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "MyMediaLite item recommendation prediction test script"
echo "This should take less than a minute ..."

echo
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=data/ml-100k

method=MostPopular
for item_arg in all-items overlap-items in-test-items in-training-items
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --$item_arg --prediction-file=pred
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --$item_arg --prediction-file=pred
done


for i in `seq 1 10`; do echo $i >> $DATA_DIR/first-10; done
for method in ItemKNN UserKNN
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10 --prediction-file=pred
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10 --prediction-file=pred
done
rm $DATA_DIR/first-10


rm pred
