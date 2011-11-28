#!/bin/bash -e

PROGRAM="bin/rating_prediction"
DATA_DIR=data/ml-100k

echo "MyMediaLite load/save test script"
echo "This will take about 5 minutes ..."

echo
echo "rating predictors"
echo "-----------------"

for method in SlopeOne BipolarSlopeOne MatrixFactorization BiasedMatrixFactorization UserItemBaseline GlobalAverage UserAverage ItemAverage FactorWiseMatrixFactorization CoClustering
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR
          #$PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time\s*\S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR
          #$PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+_time\s*\S+//g" > output2.txt
     #diff --ignore-space-change output1.txt output2.txt
done

for method in ItemKNNCosine
#for method in UserKNNCosine ItemKNNCosine
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\wtime \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="k=20" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+time \S+//g" > output2.txt
     diff --ignore-space-change output1.txt output2.txt
done

for method in UserKNNPearson ItemKNNPearson
do
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --save-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --save-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+time \S+//g" > output1.txt
     echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --load-model=tmp.model --data-dir=$DATA_DIR
	  $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method "k=20 shrinkage=10" --load-model=tmp.model --data-dir=$DATA_DIR | perl -pe "s/\w+time \S+//g" > output2.txt
     diff --ignore-space-change output1.txt output2.txt
done

rm tmp.model output1.txt output2.txt

echo
echo "item recommenders"
echo "-----------------"

PROGRAM="bin/item_recommendation"

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

