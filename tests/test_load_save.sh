#!/bin/bash -e

PROGRAM="bin/rating_prediction"
DATA_DIR=data/ml-100k

echo "MyMediaLite load/save test script"
echo "This will take about 6 minutes ..."

echo
echo "rating predictors"
echo "-----------------"

for method in SlopeOne BipolarSlopeOne MatrixFactorization BiasedMatrixFactorization UserItemBaseline GlobalAverage UserAverage ItemAverage FactorWiseMatrixFactorization CoClustering
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time\s*\S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time\s*\S+//g" > output2.txt
     diff --ignore-space-change output1.txt output2.txt
     rm tmp.model*
done

for method in UserKNNCosine ItemKNNCosine
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output2.txt
     diff --ignore-space-change output1.txt output2.txt
     rm tmp.model*
done

for method in UserKNNPearson ItemKNNPearson
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20 shrinkage=10" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20 shrinkage=10" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20 shrinkage=10" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20 shrinkage=10" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output2.txt
     diff --ignore-space-change output1.txt output2.txt
     rm tmp.model*
done

#rm output1.txt output2.txt

echo
echo "item recommenders"
echo "-----------------"

PROGRAM="bin/item_recommendation"

for method in WRMF BPRMF MostPopular
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR
          $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output2.txt
     diff --ignore-all-space output1.txt output2.txt
     rm tmp.model*
done

for method in UserKNN ItemKNN WeightedUserKNN WeightedItemKNN
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time \S+//g" > output2.txt
     diff --ignore-all-space output1.txt output2.txt
     rm tmp.model*
done

for method in ItemAttributeKNN
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+_time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+_time \S+//g" > output2.txt
     diff --ignore-all-space output1.txt output2.txt
     rm tmp.model*
done

for method in BPRLinear
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+_time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR --item-attributes=item-attributes-genres.txt | perl -pe "s/\w+_time \S+//g" > output2.txt
     diff --ignore-all-space output1.txt output2.txt
     rm tmp.model*
done

rm output1.txt output2.txt

