#!/usr/bin/perl

# Measure the overlap in terms of users, items, and interactions
# between two files.

# (c) 2010, 2011, 2012 by Zeno Gantner <zeno.gantner@gmail.com>
#
# This file is part of MyMediaLite.
#
# MyMediaLite is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# MyMediaLite is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

use strict;
use warnings;

use English qw( -no_match_vars );
use Getopt::Long;

GetOptions(
    'help'           => \( my $help         = 0 ),
    'verbose'        => \( my $verbose      = 0 ),
    'ignore-lines=i' => \( my $ignore_lines = 0 ),
    'separator=s'    => \( my $separator    = '\\s+' ),
    'filter'         => \( my $filter       = 0 ),
    'show'           => \( my $show         = 0 ),
) or usage(-1);

usage(0) if $help;

if ( $filter && $show ) {
    print "Please pick either --filter or --show\n";
    usage(-1);
}

my $separator_regex = qr{$separator};

my $filename1 = shift;
my $filename2 = shift;

my %user_known = ();
my %item_known = ();
my %comb_known = ();

# skip lines
for ( my $i = 0 ; $i < $ignore_lines ; $i++ ) { <>; }

open( my $FH1, '<', $filename1 ) or die $!;
while ( my $line = <$FH1> ) {
    chomp $line;

    # ignore empty lines
    next LINE if $line eq '';

    my ( $user_id, $item_id, $rest ) = split $separator_regex, $line, 3;
    $user_known{$user_id}            = 1;
    $item_known{$item_id}            = 1;
    $comb_known{"$user_id $item_id"} = 1;
}
close $FH1;
print STDERR "Finished with file '$filename1'\n" if $verbose;

my %user_overlap = ();
my %item_overlap = ();
my %comb_overlap = ();

open( my $FH2, '<', $filename2 ) or die $!;
while ( my $line = <$FH2> ) {
    chomp $line;

    my ( $user_id, $item_id, $rest ) = split $separator_regex, $line, 3;
    if ( exists $user_known{$user_id} ) {
        $user_overlap{$user_id} = 1;
        print "user $user_id\n" if $show;
    }
    if ( exists $item_known{$item_id} ) {
        $item_overlap{$item_id} = 1;
        print "item $item_id\n" if $show;
    }

    if ( exists $comb_known{"$user_id $item_id"} ) {
        $comb_overlap{"$user_id $item_id"} = 1;
        print "user $user_id, item $item_id\n" if $show;
    }
    else {
        print "$user_id\t$item_id\n" if $filter;
    }
}
print STDERR "Finished with file '$filename2'\n" if $verbose;

my $user_overlap_counter = scalar keys %user_overlap;
my $item_overlap_counter = scalar keys %item_overlap;
my $comb_overlap_counter = scalar keys %comb_overlap;

print STDERR "The files $filename1 and $filename2 ";
print STDERR "share $user_overlap_counter users, ";
print STDERR "$item_overlap_counter items, ";
print STDERR "and $comb_overlap_counter user-item combinations.\n";

sub usage {
    my ($return_code) = @_;

    print << "END";
$PROGRAM_NAME

computes the overlap between two files

usage: $PROGRAM_NAME [OPTIONS] FILE1 FILE2

    --help                   display this usage information
    --verbose
    --ignore-lines=N         ignore the first N lines (in both files)
    --separator=REGEX        the separator regex used to split the lines into columns,
                             default is \\s+ (one or more whitespace characters)
    --filter                 give out the FILE2, omitting events that also occur in FILE1
    --show                   display the overlaps
END
    exit $return_code;
}
