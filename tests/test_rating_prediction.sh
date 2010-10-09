#!/bin/sh -e

# don't expect this to work, this is (currently) for internal testing purposes

# TODO test save/store

LANG=C
PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 5 minutes"

cd RatingPrediction/bin/Debug

echo "MovieLens 1M"
echo "------------"

for method in matrix-factorization biased-matrix-factorization
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=5 num_iter=1 compute_fit=true
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=5 num_iter=1 compute_fit=true
done

for method in user-item-baseline global-average user-average item-average
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method
done

for method in item-attribute-knn
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20
	     $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20
done

echo "MovieLens 100K"
echo "--------------"

method=user-item-baseline
echo $PROGRAM u1.base u1.test $method
     $PROGRAM u1.base u1.test $method

for method in user-kNN-pearson user-kNN-cosine item-kNN-pearson item-kNN-cosine
do
	echo $PROGRAM u1.base u1.test $method k=20
	     $PROGRAM u1.base u1.test $method k=20
done
