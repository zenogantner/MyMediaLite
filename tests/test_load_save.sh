#!/bin/sh

# don't expect this to work, this is (currently) for internal testing purposes

# TODO test ALL engines: attribute-aware, averages, etc.

PROGRAM="mono --debug RatingPrediction.exe"

echo "This will take about 10 minutes ..."

echo "rating prediction engines"
echo "-------------------------"

cd src/RatingPrediction/bin/Debug

# load/save currently not supported: global-average user-average item-average

for method in user-item-baseline matrix-factorization biased-matrix-factorization
do
     echo $PROGRAM u1.base u1.test $method save_model=tmp.model
          $PROGRAM u1.base u1.test $method save_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM u1.base u1.test $method load_model=tmp.model
          $PROGRAM u1.base u1.test $method load_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in user-kNN-pearson user-kNN-cosine item-kNN-pearson item-kNN-cosine
do
     echo $PROGRAM u1.base u1.test $method k=20 save_model=tmp.model
	  $PROGRAM u1.base u1.test $method k=20 save_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM u1.base u1.test $method k=20 load_model=tmp.model
	  $PROGRAM u1.base u1.test $method k=20 load_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

rm tmp.model output1.txt output2.txt

echo "item prediction engines"
echo "-----------------------"

PROGRAM="mono --debug ItemPrediction.exe"

cd ../../../ItemPrediction/bin/Debug

for method in wr-mf bpr-mf most-popular
do
     echo $PROGRAM u1.base u1.test $method save_model=tmp.model
          $PROGRAM u1.base u1.test $method save_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM u1.base u1.test $method load_model=tmp.model
          $PROGRAM u1.base u1.test $method load_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in user-kNN item-kNN weighted-user-kNN weighted-item-kNN
do
     echo $PROGRAM u1.base u1.test $method k=20 save_model=tmp.model
	  $PROGRAM u1.base u1.test $method k=20 save_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM u1.base u1.test $method k=20 load_model=tmp.model
	  $PROGRAM u1.base u1.test $method k=20 load_model=tmp.model | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done
