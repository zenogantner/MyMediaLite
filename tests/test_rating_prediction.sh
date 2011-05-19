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
       echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=5 --recommender-options="num_iter=1" --compute-fit --data-dir=$DATA_DIR
            $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=5 --recommender-options="num_iter=1" --compute-fit --data-dir=$DATA_DIR
done
 
touch $DATA_DIR/empty
for method in SocialMF
do
	echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=15 --recommender-options="num_iter=1 learn_rate=0.005 social_reg=0" --compute-fit --data-dir=$DATA_DIR --user-relations=empty
	     $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=15 --recommender-options="num_iter=1 learn_rate=0.005 social_reg=0" --compute-fit --data-dir=$DATA_DIR --user-relations=empty
done
rm $DATA_DIR/empty

for method in UserItemBaseline GlobalAverage UserAverage ItemAverage
do
	echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --data-dir=$DATA_DIR
	     $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --data-dir=$DATA_DIR
done

for method in ItemAttributeKNN
do
	echo $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=20" --data-dir=$DATA_DIR
	     $PROGRAM --training-file=ml1m-0.train.txt --test-file=ml1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=20" --data-dir=$DATA_DIR
done


echo ""
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=../../../../data/ml100k

for method in UserItemBaseline SlopeOne BipolarSlopeOne
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR
done

for method in UserKNNPearson UserKNNCosine ItemKNNPearson ItemKNNCosine
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --data-dir=$DATA_DIR
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --data-dir=$DATA_DIR
done

cd ../../../../
