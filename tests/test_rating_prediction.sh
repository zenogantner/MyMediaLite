#!/bin/sh -e

PROGRAM="bin/rating_prediction"
LANG=C

echo "MyMediaLite rating prediction test script"
echo "This will take about 3 minutes ..."

echo
echo "MovieLens 1M"
echo "------------"

DATA_DIR=data/ml-1m

for method in MatrixFactorization BiasedMatrixFactorization
do
       echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 num_factors=3" --compute-fit --data-dir=$DATA_DIR --no-id-mapping
            $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 num_factors=3" --compute-fit --data-dir=$DATA_DIR --no-id-mapping
done

method=BiasedMatrixFactorization
echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 max_threads=100 num_factors=3" --compute-fit --data-dir=$DATA_DIR
     $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 max_threads=100 num_factors=3" --compute-fit --data-dir=$DATA_DIR

for target in MAE LogisticLoss
do
       echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 loss=$target num_factors=3" --compute-fit --data-dir=$DATA_DIR --no-id-mapping
            $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=2 --recommender-options="num_iter=1 loss=$target num_factors=3" --compute-fit --data-dir=$DATA_DIR --no-id-mapping
done

touch $DATA_DIR/empty
for method in SocialMF
do
	echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=3 --recommender-options="num_iter=1 learn_rate=0.005 social_reg=0 num_factors=3" --compute-fit --data-dir=$DATA_DIR --user-relations=empty --no-id-mapping
	     $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --find-iter=1 --max-iter=3 --recommender-options="num_iter=1 learn_rate=0.005 social_reg=0 num_factors=3" --compute-fit --data-dir=$DATA_DIR --user-relations=empty --no-id-mapping
done
rm $DATA_DIR/empty

for method in UserItemBaseline ItemAverage Constant Random
do
	echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --data-dir=$DATA_DIR --no-id-mapping
	     $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --data-dir=$DATA_DIR --no-id-mapping
done

for method in GlobalAverage UserAverage
do
	echo $PROGRAM --training-file=ratings.dat --chronological-split=0.25 --recommender=$method --data-dir=$DATA_DIR --file-format=movielens_1m
	     $PROGRAM --training-file=ratings.dat --chronological-split=0.25 --recommender=$method --data-dir=$DATA_DIR --file-format=movielens_1m
done


for method in ItemAttributeKNN
do
	echo $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=5" --data-dir=$DATA_DIR --no-id-mapping
	     $PROGRAM --training-file=ml-1m-0.train.txt --test-file=ml-1m-0.test.txt --recommender=$method --item-attributes=item-attributes-genres.txt --recommender-options="k=5" --data-dir=$DATA_DIR --no-id-mapping
done


echo
echo "MovieLens 100K"
echo "--------------"

DATA_DIR=data/ml-100k

for method in UserItemBaseline SlopeOne BipolarSlopeOne
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --rating-type=float
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --rating-type=float
done

for method in FactorWiseMatrixFactorization CoClustering
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --rating-type=byte
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --rating-type=byte
done

for method in UserKNNPearson UserKNNCosine ItemKNNPearson ItemKNNCosine
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=5" --data-dir=$DATA_DIR --rating-type=float
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=5" --data-dir=$DATA_DIR --rating-type=float
done

