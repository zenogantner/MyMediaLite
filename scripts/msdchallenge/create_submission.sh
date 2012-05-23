#!/bin/sh

# create submission for the Million Song Dataset Challenge:
# http://www.kaggle.com/c/msdchallenge/

cat kaggle_users.txt | perl -ne 'chomp; print "$_\t" . ++$l . "\n"' | sort -k 1b,1 > user_order.txt
perl -ne '/(.*?)\t\[(.*)\]/; $u=$1; $s=$2; $s=~s/:.+?,/ /g; $s=~s/:.+//; print "$u\t$s\n"' | sort -k 1b,1 | join --check-order -t '	' -o '2.2 1.2' - user_order.txt | sort -g | cut -f 2
rm user_order.txt
