#!/bin/sh

# to be run from the root directory of MyMediaLite

echo "The MovieLens datasets are for NON-COMMERCIAL use only."
echo "Refer to the README file for the details of the usage license."
echo "Hit enter to continue, Ctrl-C to abort."
read DUMMY

echo "This may take a while ..."
echo

# download MovieLens data
wget --output-document=data/ml-data.tar.gz         http://www.grouplens.org/system/files/ml-data.tar__0.gz
wget --output-document=data/million-ml-data.tar.gz http://www.grouplens.org/system/files/million-ml-data.tar__0.gz
wget --output-document=data/ml-data-10M100K.tar.gz http://www.grouplens.org/system/files/ml-data-10M100K.tar.gz

cd data

# unzip data
tar -zxf ml-data.tar.gz
mv ml-data ml100k

tar -zxf million-ml-data.tar.gz
mkdir ml1m
mv README movies.dat ratings.dat users.dat ml1m

tar -zxf ml-data-10M100K.tar.gz
mkdir ml10m
mv movies.dat ratings.dat tags.dat ml10m
mv allbut.pl README.html split_ratings.sh ml10m

# remove downloaded archives
rm ml-data.tar.gz million-ml-data.tar.gz ml-data-10M100K.tar.gz

# create attribute files for MovieLens 100k
../scripts/ml100k_genres.pl ml100k/u.item > ml100k/item-attributes-genres.txt
# TODO user attributes

# create attribute files for MovieLens 1M
../scripts/ml1m_genres.pl ml1m/movies.dat > ml1m/item-attributes-genres.txt
../scripts/ml1m_user_attributes.pl ml1m/users.dat > ml1m/user-attributes-nozip.txt

# create tab-separated file and evaluation splits for MovieLens 1M
../scripts/import_dataset.pl --separator=:: ml1m/ratings.dat > ml1m/ratings.txt
../scripts/import_dataset.pl --separator=:: ml1m/ratings.dat | ../scripts/crossvalidation.pl --k=5 --filename=ml1m/ml1m --suffix=.txt
../scripts/user_cold_start.pl ml1m/ratings.dat --separator=:: --filename=ml1m/ml1m-new-user --k=5 --suffix=.txt

# create tab-separated file for MovieLens 10M
../scripts/import_dataset.pl --separator=:: ml10m/ratings.dat > ml10m/ratings.txt

cd ..
