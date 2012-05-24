#!/bin/sh

# create submission for the Million Song Dataset Challenge:
# http://www.kaggle.com/c/msdchallenge/

sort -g | perl -ne '/(.*?)\t\[(.*)\]/; $u=$1; $s=$2; $s=~s/:.+?,/ /g; $s=~s/:.+//; print "$u\t$s\n"' | cut -f 2

