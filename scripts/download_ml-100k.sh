#!/bin/sh

# to be run from the root directory of MyMediaLite

echo "The MovieLens datasets are for NON-COMMERCIAL use only."
echo "Refer to the README files for the details of the usage license."

cd data

wget http://files.grouplens.org/papers/ml-100k.zip
unzip -o ml-100k.zip
rm ml-100k.zip
../scripts/ml100k_genres.pl ml-100k/u.item > ml-100k/item-attributes-genres.txt

cd ..
