#!/bin/sh -e

# don't expect this to work, this is (currently) for internal testing purposes

# TODO test save/store

LANG=C
PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 5 minutes"

cd RatingPrediction/bin/Debug

for method in matrix-factorization biased-matrix-factorization
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=5 num_iter=1 compute_fit=true
done

for method in user-item-baseline global-average user-average item-average
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method
done

for method in item-attribute-knn
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20
done

for method in user-kNN-pearson user-kNN-cosine item-kNN-pearson item-kNN-cosine
do
	$PROGRAM u1.base u1.test $method k=20
done
