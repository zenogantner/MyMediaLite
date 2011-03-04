#!/bin/sh

# to be run from the root directory of MyMediaLite

echo "The IMDB data is for NON-COMMERCIAL use only."
echo "Refer to http://www.imdb.com/Copyright for the details of the usage terms."
echo "Hit enter to continue, Ctrl-C to abort."
read DUMMY

echo "This may take a while ..."
echo

mkdir data/imdb

# download IMDB data
wget --output-document=data/imdb/german-aka-titles.list.gz ftp://ftp.fu-berlin.de/pub/misc/movies/database/german-aka-titles.list.gz

cd data/imdb

# unzip data
gunzip german-aka-titles.list.gz

cd ../..
