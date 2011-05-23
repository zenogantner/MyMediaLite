#!/bin/bash -e

# TODO test ALL engines: attribute-aware, averages, etc.

PROGRAM="mono --debug RatingPrediction.exe"
DATA_DIR=../../../../data/ml100k

cd src/RatingPrediction/bin/Debug/

echo "This will take about 5 minutes ..."

echo
echo "rating predictors"
echo "-----------------"

# load/save currently not supported: global-average user-average item-average

for method in SlopeOne BipolarSlopeOne MatrixFactorization BiasedMatrixFactorization
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in UserKNNCosine ItemKNNCosine
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in UserKNNPearson ItemKNNPearson
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

rm tmp.model output1.txt output2.txt

echo
echo "item recommenders"
echo "-----------------"

PROGRAM="mono --debug ItemPrediction.exe"

cd ../../../ItemPrediction/bin/Debug/

for method in WRMF BPRMF MostPopular
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in UserKNN ItemKNN WeightedUserKNN WeightedItemKNN
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in ItemAttributeKNN
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

for method in BPR_Linear
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+ing_time \S+ ?//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+ing_time \S+ ?//g" > output2.txt
     diff output1.txt output2.txt
done

rm tmp.model output1.txt output2.txt

cd ../../../../
