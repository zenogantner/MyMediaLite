#!/usr/bin/perl

# Remove duplicates from validation dataset
#
# Script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.
#

use strict;
use warnings;

use Getopt::Long;
GetOptions(
    'sorted-output'    => \(my $sorted_output    = 0),
    'write-timestamps' => \(my $write_timestamps = 0),
) or die "Did not understand command line parameters.\n";

my $remember_timestamps = $sorted_output || $write_timestamps;

my $separator_regex = qr{\t};

my $event_count = 0;
my %result    = ();
my %timestamp = ();

LINE:
while (<>) {
    my $line = $_;
    chomp $line;

    my ($user, $item, $result, $timestamp) = split $separator_regex, $line;

    $event_count++;

    my $key = "$user\t$item";
    if (exists $result{$key} && $result != 1) {
        if ($result{$key} == $result && $remember_timestamps) {
            $timestamp{$key} = $timestamp;
        }
    }
    else {
        $result{$key}    = $result;
        $timestamp{$key} = $timestamp if $remember_timestamps;
    }
}

if ($sorted_output) {
    print STDERR "Sorting and printing to STDOUT ...\n";
    foreach my $key (sort { $timestamp{$a} <=> $timestamp{$b} } keys %result) {
	print "$key\t$result{$key}\t$timestamp{$key}\n";
    }
}
else {
    print STDERR "Printing to STDOUT ...\n";
    foreach my $key (keys %result) {
	if ($write_timestamps) {
	    print "$key\t$result{$key}\t$timestamp{$key}\n";
	}
	else {
	    print "$key\t$result{$key}\n";
	}
    }
}
print STDERR "Done.\n";
