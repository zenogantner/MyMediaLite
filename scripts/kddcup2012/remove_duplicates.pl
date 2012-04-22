#!/usr/bin/perl

# Remove duplicates from validation dataset
#
# Script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.

use strict;
use warnings;

use Getopt::Long;
GetOptions(
    'write-timestamps' => \(my $write_timestamps = 0),
) or die "Did not understand command line parameters.\n";

my $remember_timestamps = $write_timestamps;

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

    if (!exists $result{$user}) {
	$result{$user} = {};
	$timestamp{$user} = {};
    }

    if (exists $result{$user}->{$item} && $result != 1) {
        if ($result{$user}->{$item} == $result && $remember_timestamps) {
            $timestamp{$user}->{$item} = $timestamp;
        }
    }
    else {
        $result{$user}->{$item}    = $result;
        $timestamp{$user}->{$item} = $timestamp if $remember_timestamps;
    }
}

print STDERR "Printing to STDOUT ...\n";
foreach my $user (keys %result) {
    foreach my $item (keys %{$result{$user}}) {
        if ($write_timestamps) {
            print "$user\t$item\t$result{$user}->{$item}\t$timestamp{$user}->{$item}\n";
        }
        else {
            print "$user\t$item\t$result{$user}->{$item}\n";
        }
    }
}
print STDERR "Done.\n";
