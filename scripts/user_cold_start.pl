#!/usr/bin/perl

# (c) 2010 by Zeno Gantner <zeno.gantner@gmail.com>
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

use Carp;
use English qw( -no_match_vars );
use File::Slurp;
use Getopt::Long;
use List::Util 'shuffle';

my $SUFFIX   = '.txt';
my $FILENAME = 'fold';
my $K        = 10;

GetOptions(
	   'help'          => \(my $help        = 0),
	   'suffix=s'      => \(my $file_suffix = $SUFFIX),
	   'filename=s'    => \(my $file_name   = $FILENAME),
	   'k=i'           => \(my $k           = $K),
	   'filter-file=s' => \(my $filter_file = ''),
	   'separator=s'   => \(my $separator = '\\s+'),
	  ) or usage(-1);

usage(0) if $help;

my $separator_regex = qr{$separator};

my @filter_ids = ();
if ($filter_file) {
    my @filter_lines = split /\n/, read_file($filter_file);
    foreach my $line (@filter_lines) {
	my ($filter_id, $rest) = split /\s/, $line;
	push @filter_ids, $filter_id;
    }
}
my %filter_id = map { $_ => 1 } @filter_ids;

my %scores_by_user          = ();
my $num_ratings             = 0;
my %filtered_scores_by_user = ();
my $num_filtered_ratings    = 0;
while (<>) {
    my $line = $_;
    chomp $line;

    my @fields = split $separator_regex, $line;
    if (scalar @fields < 2) {
	croak "Could not parse line: '$line'";
    }

    my ($user_id, $item_id, $score) = @fields;
    
    if (!exists $scores_by_user{$user_id}) {
	$scores_by_user{$user_id} = { };
	if (exists $filter_id{$user_id}) {
	    $filtered_scores_by_user{$user_id} = { };
	}
    }	    
	    
    if (exists $scores_by_user{$user_id}->{$item_id}) {
	carp "combination $user_id, $item_id occurs several times\n";
    }

    $scores_by_user{$user_id}->{$item_id} = $score;
    if (exists $filter_id{$user_id}) {
	$filtered_scores_by_user{$user_id}->{$item_id} = $score;
	$num_filtered_ratings++;
    }
    $num_ratings++;
}
print "Done reading in: $num_ratings ratings and $num_filtered_ratings filtered ratings\n";

my @FH_TRAIN = ();
my @FH_TEST  = ();
for my $i (0 .. $k - 1) {
    open(my $FH_TRAIN,  '>', "${file_name}-${i}.train${file_suffix}") or croak $!;
    push @FH_TRAIN, $FH_TRAIN;
    open(my $FH_TEST,  '>', "${file_name}-${i}.test${file_suffix}") or croak $!;
    push @FH_TEST, $FH_TEST;
}

if (scalar keys %filtered_scores_by_user == 0) {
    %filtered_scores_by_user = %scores_by_user;
}
my @relevant_users = shuffle keys %filtered_scores_by_user;
#my @relevant_users = keys %filtered_scores_by_user;
ITEM:
for (my $i = 0; $i < scalar @relevant_users; $i++) {
    my $user_id = $relevant_users[$i];

    foreach my $item_id (sort { $a <=> $b} keys %{$scores_by_user{$user_id}}) {
	for my $j (0 .. $k - 1) {
	    my $FH = $FH_TRAIN[$j];
	    if ($i % $k == $j) {
		$FH = $FH_TEST[$j];
	    }
	    print $FH "$user_id\t$item_id\t$scores_by_user{$user_id}->{$item_id}\n";
	}
    }
}
print "Done writing $k folds.\n";


sub usage {
    my ($return_code) = @_;

    print << "END";
$PROGRAM_NAME

Generate a new item cold-start train/test partition (k-fold cross validation over the relevant items)

usage: $PROGRAM_NAME [OPTIONS] [INPUT]

    --help                   display this usage information
    --file-suffix=SUFFIX     suffix used for the output files, default is $SUFFIX
    --filename=NAME          body of the file name used for the output files, default is $FILENAME
    --k=N                    number of folds, default is $K
    --filter-file=FILE       only select item IDs that are present in this file, e.g. the attribute description
    --separator=REGEX        the separator regex used to split the lines into columns,
                             default is \\s+ (one or more whitespace characters)
END
    exit $return_code;
}
