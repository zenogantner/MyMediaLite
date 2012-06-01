#!/usr/bin/perl

# (c) 2012 by Zeno Gantner <zeno.gantner@gmail.com>
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
use 5.8.0;

use English qw( -no_match_vars );
use File::Slurp;
use Getopt::Long;

GetOptions(
	'help'           => \(my $help = 0),
	'n'              => \(my $n    = 500),
	'predictions=s'  => \(my $prediction_file   = ''),
	'ground-truth=s' => \(my $ground_truth_file = ''),
) or usage(-1);

usage(0) if $help;

my %songs_by_user = ();

# read in ground truth
my @lines = read_file($ground_truth_file);
foreach my $line (@lines) {
	chomp $line;
	my ($user_id, $item_id, $count) = split /\s+/, $line;

	if (!exists $songs_by_user{$user_id}) {
		$songs_by_user{$user_id} = {};
	}
	$songs_by_user{$user_id}->{$item_id} = $count;
}

# evaluate
my @pred_lines = read_file($prediction_file);
my $user_id = 1;
my $map_sum = 0;
my $num_zero_ap_users = 0;
foreach my $line (@pred_lines) {
	my @ranked_items = split / /, $line;
	$map_sum += average_precision(\@ranked_items, $songs_by_user{$user_id});
	$user_id++;
}
my $num_users = scalar @pred_lines;
my $map = $map_sum / $num_users;
print "$num_users users\n";
print "$num_zero_ap_users users with AP=0\n";
printf "MAP: %.6f\n", $map;


sub average_precision {
	my ($ranked_items_ref, $ground_truth_ref) = @_;

	my $hit_count = 0;
	my $position  = 0;
	my $sum       = 0.0;
	foreach my $item_id (@$ranked_items_ref) {
		$position++;

		if (exists $ground_truth_ref->{$item_id}) {
			$hit_count++;
			$sum += $hit_count / $position;
		}
	}

	my $num_of_ground_truth_items = scalar keys %$ground_truth_ref;
	if ($hit_count == 0) {
		$num_zero_ap_users++;
		return 0;
	}
	return $sum / $num_of_ground_truth_items;
}

sub usage {
	my ($return_code) = @_;

	print << "END";
$PROGRAM_NAME

Evaluate for the Million Song Dataset challenge

usage: $PROGRAM_NAME [OPTIONS] FILE

  options:
    --help               display this help
    --n                  the length of the item list per user (default is 500)
    --predictions=FILE   get predictions from FILE
    --ground-truth=FILE  get ground truth from FILE
END
	exit $return_code;
}
