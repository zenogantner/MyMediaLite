#!/bin/sh

# to be run from the root directory of MyMediaLite

# download MovieLens data
wget --output-document=data/ml-data.tar.gz         http://www.grouplens.org/system/files/ml-data.tar__0.gz
wget --output-document=data/million-ml-data.tar.gz http://www.grouplens.org/system/files/million-ml-data.tar__0.gz

cd data

# unzip data
tar -zxf ml-data.tar.gz
mv ml-data ml100k

tar -zxf million-ml-data.tar.gz
mkdir ml1m
mv README movies.dat ratings.dat users.dat ml1m

# remove downloaded archives
rm ml-data.tar.gz million-ml-data.tar.gz

# create attribute files

cd ..
