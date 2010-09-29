#!/bin/sh -e

# don't expect this to work, this is (currently) for internal testing purposes

# TODO test save/store
# TODO use revant_items

LANG=C
PROGRAM="mono --debug ItemPrediction.exe"

echo "This may take up to 60 minutes"
echo "Do not take the results serious - we do not use the best hyperparameters here"

cd ItemPrediction/bin/Debug

for method in bpr-mf wr-mf
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method find_iter=1 max_iter=2 num_iter=1
done

for method in random most-popular item-kNN user-kNN weighted-user-kNN
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method k=20
done

for method in item-attribute-knn
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20
done

for method in bpr-linear
do
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt  find_iter=1 max_iter=2 num_iter=1
done

for method in user-attribute-knn
do
	$PROGRAM ml1m-new-user-0.train.txt ml1m-new-user-0.test.txt $method user_attributes=user-attributes-nozip.txt k=20
done
