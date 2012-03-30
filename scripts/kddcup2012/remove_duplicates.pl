#!/usr/bin/perl

# Remove duplicates from validation dataset
#
# Script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.
#

use strict;
use warnings;

use English qw( -no_match_vars );
use Getopt::Long;
use POSIX qw(strftime);

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
        if ($result{$key} == $result) {
            $timestamp{$key} = $timestamp;
        }
    }
    else {
        $result{$key}    = $result;
        $timestamp{$key} = $timestamp;
    }
}

print STDERR "Sorting and printing to STDOUT ...\n";
foreach my $key (sort { $timestamp{$a} <=> $timestamp{$b} } keys %result) {
    print "$key\t$result{$key}\t$timestamp{$key}\n";
}
print STDERR "Done.\n";