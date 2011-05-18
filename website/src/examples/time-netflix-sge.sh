#!/bin/bash -e

prog="mono --debug RatingPrediction.exe"

qsub_args="-b y -j n -cwd"
if [[ -n $SGE_PRIORITY ]]; then
        qsub_args="$qsub_args -p ${SGE_PRIORITY}"
fi
if [[ -n $EMAIL ]]; then
        qsub_args="$qsub_args -m sa -M ${EMAIL}"
fi

qsub_args="${qsub_args} -pe serial 4 -R y -l s_cpu=23:00:00"

dataset=netflix
data_dir=data/$dataset

method=BiasedMatrixFactorization

# single split
for k in 0 5 10 20 25 30 35 40 45 50 55 60 65 70 75 80 85 90 95 100 105 110 115 120
do
  params="--training-file=train-byuser-nd.txt --test-file=test-byuser-nd.txt --data-dir=$data_dir --recommender-options=\"bold_driver=true num_factors=$k num_iter=0 learn_rate=0.05 regularization=0.1 bias_reg=0.005\" --rating-type=byte --random-seed=1 --find-iter=1 --max-iter=10"
  command="qsub ${qsub_args} -N recsys2011-split-$dataset-$k $prog --recommender=$method $params"
  if [ "$1" == "--start" ]; then
      $command
  else
      echo $command
  fi
done

