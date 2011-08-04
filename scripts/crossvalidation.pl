#!/usr/bin/perl

# (c) 2009, 2010, 2011 by Zeno Gantner <zeno.gantner@gmail.com>
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

# notice: In most cases, you will not need this script. You can just use
#         the --cross-validation=K feature of the command-line tools

use strict;
use warnings;
use 5.8.0;

use Carp;
use English qw( -no_match_vars );
use File::Slurp;
use Getopt::Long;
use List::Util 'shuffle';

my $DEFAULT_K = 10;

GetOptions(
	   'help'       => \(my $help   = 0),
	   'filename=s' => \(my $name   = 'dataset'),
	   'suffix=s'   => \(my $suffix = '.txt'),
	   'k=i'        => \(my $k      = $DEFAULT_K),
	  ) or usage(-1);

usage(0) if $help;

my @lines           = read_lines();
my $number_of_lines = scalar @lines;

my @Line_To_Fold = ();

for (my $i = 0; $i < $number_of_lines; $i++) {
    $Line_To_Fold[$i] = $i % $k;
}
@Line_To_Fold = shuffle(@Line_To_Fold);

my @train_file_handles = ();
my @test_file_handles  = ();
for (my $i = 0; $i < $k; $i++) {
    open my $TRAIN_FH, '>', "${name}-${i}.train${suffix}";
    $train_file_handles[$i] = $TRAIN_FH;
    open my $TEST_FH,  '>', "${name}-${i}.test${suffix}";
    $test_file_handles[$i]  = $TEST_FH;
}

for (my $i = 0; $i < $number_of_lines; $i++) {
    for (my $j = 0; $j < $k; $j++) {
	my $FH;
	if ($j == $Line_To_Fold[$i]) {
	    $FH = $test_file_handles[$j];
	}
	else {
	    $FH = $train_file_handles[$j];
	}
	print $FH $lines[$i];
    }
}

sub read_lines {
    my @lines = ();

    if (scalar @ARGV > 0) {
        foreach my $file (@ARGV) {
            push @lines, read_file($file);
        }
    }
    else {
        @lines = read_file(\*STDIN);
    }
    return wantarray ? @lines : join '', @lines;
}

sub usage {
    my ($return_code) = @_;

    print << "END";
$PROGRAM_NAME

split file for crossvalidation

usage: $PROGRAM_NAME [OPTIONS] FILE

  options:
    --help              display this help
    --filename=NAME     (prefix of the) name of the output files
    --suffix=.SUFFIX    suffix of the output files
    --k=K               the number of folds (default $DEFAULT_K)
END
    exit $return_code;
}
