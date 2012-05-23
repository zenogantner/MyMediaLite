#!/bin/sh

perl -ne '/(.*?)\t\[(.*)\]/; $u=$1; $s=$2; $s=~s/:.+?,/ /g; $s=~s/:.+//; print "$u\t$s\n"' | sort -k 1b,1 | join --check-order -t '	' -o '2.2 1.2' - user_order.txt | sort -g | cut -f 2
