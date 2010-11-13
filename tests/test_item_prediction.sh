#!/bin/sh -e

# don't expect this to work, this is (currently) for internal testing purposes

# TODO test save/store
# TODO use revant_items

LANG=C
PROGRAM="mono --debug ItemPrediction.exe"

echo "This may take about 15 minutes"
echo "Do not take the results serious - we do not use the best hyperparameters here"

cd src/ItemPrediction/bin/Debug

echo "MovieLens 100K"
echo "--------------"

for method in bpr-mf wr-mf
do
	echo $PROGRAM u1.base u1.test $method find_iter=1 max_iter=5 num_iter=1
   	     $PROGRAM u1.base u1.test $method find_iter=1 max_iter=5 num_iter=1
done


for method in random most-popular
do
	echo $PROGRAM u1.base u1.test $method
	     $PROGRAM u1.base u1.test $method
done

for method in item-kNN user-kNN weighted-user-kNN
do
	echo $PROGRAM u1.base u1.test $method k=20
	     $PROGRAM u1.base u1.test $method k=20
done

echo "MovieLens 1M"
echo "------------"

for method in item-attribute-knn
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt k=20
done

for method in bpr-linear
do
	echo $PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt  find_iter=1 max_iter=2 num_iter=1
	$PROGRAM ml1m-0.train.txt ml1m-0.test.txt $method item_attributes=item-attributes-genres.txt  find_iter=1 max_iter=2 num_iter=1
done

for method in user-attribute-knn
do
	echo $PROGRAM ml1m-new-user-0.train.txt ml1m-new-user-0.test.txt $method user_attributes=user-attributes-nozip.txt k=20
	$PROGRAM ml1m-new-user-0.train.txt ml1m-new-user-0.test.txt $method user_attributes=user-attributes-nozip.txt k=20
done
