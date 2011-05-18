#!/bin/bash -e

prog="mono --debug RatingPrediction.exe"

dataset=ml100k
data_dir=data/$dataset

method=BiasedMatrixFactorization

# single split
for k in 0 5 10 20 25 30 35 40 45 50 55 60 65 70 75 80 85 90 95 100 105 110 115 120
do
  options="bold_driver=true num_factors=$k num_iter=0 learn_rate=0.05 regularization=0.1 bias_reg=0.005"
  params="--training-file=u1.base --test-file=u1.test --data-dir=$data_dir --rating-type=byte --random-seed=1 --find-iter=1 --max-iter=100"
  command="$prog --recommender=$method $params"
  if [ "$1" == "--start" ]; then
      $command --recommender-options="$options" &> time-ml100k-$k.log
  else
      echo $command --recommender-options=\"$options\"
  fi
done
