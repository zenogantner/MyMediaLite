#!/bin/sh -e

PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 7 minutes ..."

echo ""
echo "MovieLens 1M"
echo "------------"

DATA_DIR=../../../../data/ml1m

cd src/RatingPrediction/bin/Debug/

for method in MatrixFactorization BiasedMatrixFactorization
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=5 num_iter=1 compute_fit=true data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=5 num_iter=1 compute_fit=true data_dir=$DATA_DIR
done

touch $DATA_DIR/empty
for method in SocialMF
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=15 num_iter=1 learn_rate=0.005 compute_fit=true social_reg=0 data_dir=$DATA_DIR user_relation=empty
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=15 num_iter=1 learn_rate=0.005 compute_fit=true social_reg=0 data_dir=$DATA_DIR user_relation=empty
done
rm $DATA_DIR/empty

for method in UserItemBaseline GlobalAverage UserAverage ItemAverage
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method data_dir=$DATA_DIR
done

for method in ItemAttributeKNN
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20 data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20 data_dir=$DATA_DIR
done


echo ""
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=../../../../data/ml100k

for method in UserItemBaseline SlopeOne BipolarSlopeOne
do
	echo $PROGRAM u1.base u1.test $method data_dir=$DATA_DIR
	     $PROGRAM u1.base u1.test $method data_dir=$DATA_DIR
done

for method in UserKNNPearson UserKNNCosine ItemKNNPearson ItemKNNCosine
do
	echo $PROGRAM u1.base u1.test $method k=20 data_dir=$DATA_DIR
	     $PROGRAM u1.base u1.test $method k=20 data_dir=$DATA_DIR
done

cd ../../../../
