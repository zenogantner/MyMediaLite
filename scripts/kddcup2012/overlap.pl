#!/usr/bin/perl

#
# Validation script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.
#

use strict;
use warnings;

use Array::Utils qw(:all);

my $example_file = $ARGV[0];
my $submission_file = $ARGV[1];

open my $fh1, "<", $example_file 
	or die "cannot open < $example_file: $!";
open my $fh2, "<", $submission_file 
	or die "cannot open < $submission_file: $!";

<$fh1>; # header

my $line = 0;
my $overlap = 0;
while (1) {
	$line++;
	my ($example_uid,    $example_items)    = split /,/, <$fh1>;
	my ($submission_uid, $submission_items) = split /,/, <$fh2>;
	
	if ($example_uid ne $submission_uid) {
		die "Different user IDs in line $line: $example_uid vs. $submission_uid\n";
	}
	
	my @example_list    = split / /, $example_items;
	my @submission_list = split / /, $submission_items;
	my @intersect = intersect(@example_list, @submission_list);
	$overlap += scalar @intersect;
	
	if ($line == 1340127) {
		last;
	}
}
$overlap /= $line;
print "Overlap: $overlap\n";