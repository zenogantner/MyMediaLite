#!/bin/sh

# to be run from the root directory of MyMediaLite

set -e

echo "The IMDB data is for NON-COMMERCIAL use only."
echo "Refer to http://www.imdb.com/Copyright for the details of the usage terms."
echo "Hit enter to continue, Ctrl-C to abort."
read DUMMY

echo "This may take a while ..."
echo

mkdir -p data/imdb
cd data/imdb

# download IMDB data
wget https://datasets.imdbws.com/title.akas.tsv.gz

# unzip data
gunzip title.akas.tsv.gz

cd ../..
