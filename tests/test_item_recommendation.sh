#!/bin/bash -e

PROGRAM="bin/item_recommendation"
K=3
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "MyMediaLite item recommendation test script"
echo "This may take about 1 minute ..."

echo
echo "Tiny example dataset"
echo "--------------------"

for method in ItemKNN UserKNN
do
	for c in Cosine ConditionalProbability Jaccard Cooccurrence
	do
		echo $PROGRAM --training-file=$DIR/example.train --test-file=$DIR/example.test --recommender=$method --recommender-options="k=$K c=$c"
		     $PROGRAM --training-file=$DIR/example.train --test-file=$DIR/example.test --recommender=$method --recommender-options="k=$K c=$c"
	done
done

echo
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=data/ml-100k

for method in BPRMF WRMF
do
	echo $DATA_DIR
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 num_factors=$K" --data-dir=$DATA_DIR
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 num_factors=$K" --data-dir=$DATA_DIR
done

method=MostPopular
for item_arg in all-items overlap-items in-test-items in-training-items
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --$item_arg
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --$item_arg
done

for i in `seq 1 10`; do echo $i >> $DATA_DIR/first-10; done
for method in ItemKNN UserKNN
do
	for c in Cosine ConditionalProbability Jaccard Cooccurrence
	do
		echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=$K c=$c" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10
		     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=$K c=$c" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10
	done
done


method=LeastSquareSLIM
echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=$K num_iter=$K" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10
     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=$K num_iter=$K" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10


method=BPRSLIM
echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=$K" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10
     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=$K" --data-dir=$DATA_DIR --test-users=first-10 --candidate-items=first-10

rm $DATA_DIR/first-10

echo
echo "MovieLens 1M"
echo "------------"

DATA_DIR=data/ml-1m

for method in ItemAttributeKNN
do
	echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=$K" --data-dir=$DATA_DIR --num-test-users=15
	     $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=$K" --data-dir=$DATA_DIR --num-test-users=15
done

for method in BPRLinear
do
	echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --find-iter=1 --max-iter=2 --recommender-options="num_iter=0" --data-dir=$DATA_DIR --num-test-users=15
	     $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --find-iter=1 --max-iter=2 --recommender-options="num_iter=0" --data-dir=$DATA_DIR --num-test-users=15
done

for method in UserAttributeKNN
do
	echo $PROGRAM --training-file=ml-1m-new-user-0.train.txt --test-file=ml-1m-new-user-0.test.txt --recommender=$method --user-attributes=user-attributes-nozip.txt --recommender-options="k=$K" --data-dir=$DATA_DIR --num-test-users=5
	     $PROGRAM --training-file=ml-1m-new-user-0.train.txt --test-file=ml-1m-new-user-0.test.txt --recommender=$method --user-attributes=user-attributes-nozip.txt --recommender-options="k=$K" --data-dir=$DATA_DIR --num-test-users=5
done

