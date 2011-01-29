#!/bin/sh -e

# TODO use relevant_items

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
	echo $PROGRAM $THIS_DIR/example.train $THIS_DIR/example.test $method k=20
	     $PROGRAM $THIS_DIR/example.train $THIS_DIR/example.test $method k=20
done

echo
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=../../../../data/ml100k

for method in BPRMF WRMF
do
	echo $PROGRAM u1.base u1.test $method find_iter=1 max_iter=5 num_iter=1 data_dir=$DATA_DIR
   	     $PROGRAM u1.base u1.test $method find_iter=1 max_iter=5 num_iter=1 data_dir=$DATA_DIR
done


for method in Random MostPopular
do
	echo $PROGRAM u1.base u1.test $method data_dir=$DATA_DIR
	     $PROGRAM u1.base u1.test $method data_dir=$DATA_DIR
done

for method in ItemKNN WeightedItemKNN UserKNN WeightedUserKNN
do
	echo $PROGRAM u1.base u1.test $method k=20 data_dir=$DATA_DIR
	     $PROGRAM u1.base u1.test $method k=20 data_dir=$DATA_DIR
done

echo
echo "MovieLens 1M"
echo "------------"

DATA_DIR=../../../../data/ml1m

for method in ItemAttributeKNN
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20 data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20 data_dir=$DATA_DIR
done

for method in BPR_Linear
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt find_iter=1 max_iter=2 num_iter=1 data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt find_iter=1 max_iter=2 num_iter=1 data_dir=$DATA_DIR
done

for method in UserAttributeKNN
do
	echo $PROGRAM ml1m-new-user-0.train.txt ml1m-new-user-0.test.txt $method user_attributes=user-attributes-nozip.txt k=20 data_dir=$DATA_DIR
             $PROGRAM ml1m-new-user-0.train.txt ml1m-new-user-0.test.txt $method user_attributes=user-attributes-nozip.txt k=20 data_dir=$DATA_DIR
done

cd ../../../../
