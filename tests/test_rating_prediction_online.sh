#!/bin/sh -e

PROGRAM="bin/rating_prediction"

echo "MyMediaLite online rating prediction test script"
echo "This will take less than 1 minute ..."

echo
echo "MovieLens 100k"
echo "--------------"

DATA_DIR=data/ml-100k

for method in GlobalAverage UserAverage ItemAverage Random Constant
do
       echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
            $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
done

method=UserItemBaseline
echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=3" --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=3" --data-dir=$DATA_DIR --online-evaluation --no-id-mapping

for method in MatrixFactorization BiasedMatrixFactorization
do
       echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=3 num_factors=3" --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
            $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=3 num_factors=3" --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
done
