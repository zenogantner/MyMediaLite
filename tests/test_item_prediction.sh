#!/bin/sh -e

PROGRAM="mono --debug ItemPrediction.exe"
THIS_DIR=`pwd`/tests

cd src/ItemPrediction/bin/Debug/

echo "This may take about 15 minutes ..."
echo "Do not take the results serious - we do not use the best hyperparameters here"

echo
echo "Tiny example dataset"
echo "--------------------"

for method in ItemKNN WeightedItemKNN UserKNN WeightedUserKNN
do
	echo $PROGRAM --training-file=$THIS_DIR/example.train --test-file=$THIS_DIR/example.test --recommender=$method --recommender-options="k=20"
	     $PROGRAM --training-file=$THIS_DIR/example.train --test-file=$THIS_DIR/example.test --recommender=$method --recommender-options="k=20"
done

echo
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=../../../../data/ml100k

for method in BPRMF WRMF
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --find-iter=1 --max-iter=5 --recommender-options="num_iter=1" --data-dir=$DATA_DIR
   	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --find-iter=1 --max-iter=5 --recommender-options="num_iter=1" --data-dir=$DATA_DIR
done


for method in Random MostPopular
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR
done

for i in `seq 1 10`; do echo $i >> $DATA_DIR/first-10; done
for method in ItemKNN WeightedItemKNN UserKNN WeightedUserKNN
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --data-dir=$DATA_DIR --relevant-users=first-10 --relevant-items=first-10
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --data-dir=$DATA_DIR --relevant-users=first-10 --relevant-items=first-10
done
rm $DATA_DIR/first-10

echo
echo "MovieLens 1M"
echo "------------"

DATA_DIR=../../../../data/ml1m

for method in ItemAttributeKNN
do
	echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=20" --data-dir=$DATA_DIR
	     $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=20" --data-dir=$DATA_DIR
done

for method in BPR_Linear
do
	echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --find-iter=1 --max-iter=2 --recommender-options="num_iter=1" --data-dir=$DATA_DIR
	     $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --find-iter=1 --max-iter=2 --recommender-options="num_iter=1" --data-dir=$DATA_DIR
done

for method in UserAttributeKNN
do
	echo $PROGRAM --training-file=ml1m-new-user-0.train.txt --test-file=ml1m-new-user-0.test.txt --recommender=$method --user-attributes=user-attributes-nozip.txt --recommender-options="k=20" --data-dir=$DATA_DIR
             $PROGRAM --training-file=ml1m-new-user-0.train.txt --test-file=ml1m-new-user-0.test.txt --recommender=$method --user-attributes=user-attributes-nozip.txt --recommender-options="k=20" --data-dir=$DATA_DIR
done

cd ../../../../
