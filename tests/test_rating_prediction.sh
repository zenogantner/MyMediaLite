#!/bin/sh -e

PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 7 minutes ..."

echo ""
echo "MovieLens 1M"
echo "------------"

DATA_DIR=../../../../data/ml1m

cd src/RatingPrediction/bin/Debug/

for method in matrix-factorization biased-matrix-factorization
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

for method in user-item-baseline global-average user-average item-average
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method data_dir=$DATA_DIR
done

for method in item-attribute-knn
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20 data_dir=$DATA_DIR
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20 data_dir=$DATA_DIR
done


echo ""
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=../../../../data/ml100k

for method in user-item-baseline slope-one bipolar-slope-one
echo $PROGRAM u1.base u1.test $method data_dir=$DATA_DIR
     $PROGRAM u1.base u1.test $method data_dir=$DATA_DIR

for method in user-kNN-pearson user-kNN-cosine item-kNN-pearson item-kNN-cosine
do
	echo $PROGRAM u1.base u1.test $method k=20 data_dir=$DATA_DIR
	     $PROGRAM u1.base u1.test $method k=20 data_dir=$DATA_DIR
done

cd ../../../../
