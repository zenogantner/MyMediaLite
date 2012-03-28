#!/usr/bin/perl

#
# Validation script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.
#

use strict;
use warnings;

my $example_file = $ARGV[0];
my $submission_file = $ARGV[1];

open my $fh1, "<", $example_file 
	or die "cannot open < $example_file: $!";
open my $fh2, "<", $submission_file 
	or die "cannot open < $submission_file: $!";

<$fh1>; # header
<$fh2>; # header

my $line = 0;
while (1) {
	$line++;
	my ($example_uid)                       = split /,/, <$fh1> or die "End of example file\n";
	my ($submission_uid, $submission_items) = split /,/, <$fh2> or die "End of submission file\n";
	
	my @submission_items = split / /, $submission_items;
	my $num_items = scalar @submission_items;
	if ($num_items > 3) {
		chomp $submission_items;
		die "More than three items in line $line: '$submission_items'\n";
	}
	
	if ($example_uid != $submission_uid) {
		die "Different user IDs in line $line: $example_uid vs. $submission_uid\n";
	}
	if ($line == 1340127) {
		last;
	}	
}
print "Everything seems to be fine.\n";