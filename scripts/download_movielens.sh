#!/bin/sh

# to be run from the root directory of MyMediaLite

echo "The MovieLens datasets are for NON-COMMERCIAL use only."
echo "Refer to the README files for the details of the usage license."

cd data

# download the other 2 MovieLens datasets
wget http://grouplens.org/system/files/ml-1m.zip
wget http://files.grouplens.org/papers/ml-10m.zip

# unzip data
unzip ml-1m.zip
unzip ml-10m.zip

mv ml-10M100K ml-10m

# remove downloaded archives
rm ml-1m.zip ml-10m.zip


# create attribute files for MovieLens 1M
../scripts/ml1m_genres.pl ml-1m/movies.dat > ml-1m/item-attributes-genres.txt
../scripts/ml1m_user_attributes.pl ml-1m/users.dat > ml-1m/user-attributes-nozip.txt

# create tab-separated file and evaluation splits for MovieLens 1M
../scripts/import_dataset.pl --separator=:: ml-1m/ratings.dat > ml-1m/ratings.txt
../scripts/import_dataset.pl --separator=:: ml-1m/ratings.dat | ../scripts/crossvalidation.pl --k=5 --filename=ml-1m/ml-1m --suffix=.txt
../scripts/crossvalidation.pl --k=5 --filename=ml-1m/ml-1m --suffix=.dat < ml-1m/ratings.dat
../scripts/user_cold_start.pl ml-1m/ratings.dat --separator=:: --filename=ml-1m/ml-1m-new-user --k=5 --suffix=.txt

# create tab-separated file for MovieLens 10M
../scripts/import_dataset.pl --separator=:: ml-10m/ratings.dat > ml-10m/ratings.txt
../scripts/ml1m_genres.pl ml-10m/movies.dat > ml-10m/item-attributes-genres.txt

cd ..
