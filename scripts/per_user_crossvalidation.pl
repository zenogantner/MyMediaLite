#!/usr/bin/perl

# (c) 2009, 2010, 2011, 2012 by Zeno Gantner <zeno.gantner@gmail.com>
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
use Getopt::Long;
use List::Util 'shuffle';

my $USER_COLUMN  = 0;
my $DEFAULT_K = 5;

GetOptions(
	'help'          => \(my $help        = 0),
	'filename=s' => \(my $name   = 'dataset'),
	'suffix=s'      => \(my $suffix      = '.txt'),
	'k=i'           => \(my $k           = $DEFAULT_K),
	'separator=s'   => \(my $separator   = '\\s+'),
	'user-column=i' => \(my $user_column = $USER_COLUMN),
) or usage(-1);

usage(0) if $help;
my $separator_regex = qr{$separator};


my $lines_by_user_ref = read_lines_by_user();

my %lines_to_fold_by_user = ();
my $last_fold = 0;
foreach my $user (sort keys %$lines_by_user_ref) {
		my $number_of_lines = scalar @{$lines_by_user_ref->{$user}};
		for (my $i = 0; $i < $number_of_lines; $i++) {
			$lines_to_fold_by_user{$user}->[$i] = $last_fold++ % $k;
		}
		$lines_to_fold_by_user{$user} = [ shuffle(@{$lines_to_fold_by_user{$user}}) ];
}


my @train_file_handles = ();
my @test_file_handles  = ();
for (my $i = 0; $i < $k; $i++) {
	open my $TRAIN_FH, '>', "${name}-${i}.train${suffix}";
	$train_file_handles[$i] = $TRAIN_FH;
	open my $TEST_FH,  '>', "${name}-${i}.test${suffix}";
	$test_file_handles[$i]  = $TEST_FH;
}

foreach my $user (sort keys %$lines_by_user_ref) {
	my $number_of_lines = scalar @{$lines_by_user_ref->{$user}};
	for (my $i = 0; $i < $number_of_lines; $i++) {
		for (my $j = 0; $j < $k; $j++) {
			my $FH = ($j == $lines_to_fold_by_user{$user}->[$i]) ? $test_file_handles[$j] : $train_file_handles[$j];
			print $FH $lines_by_user_ref->{$user}->[$i];
		}
	}
}

sub read_lines_by_user {
	my %lines_by_user = ();

	while (<>) {
		my $line = $_;

		next LINE if $line eq "\n"; # ignore empty lines

		my @fields = split $separator_regex, $line;
		die "Could not parse line: '$line'\n" if scalar @fields < 2;

		my $user = $fields[$user_column];
		push @{$lines_by_user{$user}}, $line;
	}
	return wantarray ? %lines_by_user : \%lines_by_user;
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
    --separator=REGEX   the separator regex used to split the lines into columns,
                        default is \\s+ (one or more whitespace characters)
    --user-column=N     specifies the user column (0-based), default is $USER_COLUMN
END
	exit $return_code;
}
