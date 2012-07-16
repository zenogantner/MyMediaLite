#!/bin/sh -e

PROGRAM="bin/rating_prediction"
K=2
IT=1

echo "MyMediaLite online rating prediction test script"
echo "This will take about 2 minutes ..."

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
echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=$IT" --data-dir=$DATA_DIR --online-evaluation
     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=$IT" --data-dir=$DATA_DIR --online-evaluation

for method in MatrixFactorization BiasedMatrixFactorization
do
	echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=$IT num_factors=$K" --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
	     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=$IT num_factors=$K" --data-dir=$DATA_DIR --online-evaluation --no-id-mapping
done

method=ItemAttributeKNN
echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=$K" --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=$K" --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+_time \S+//g" > output1.txt

method=NaiveBayes
echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt --online-evaluation
     $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt --online-evaluation
