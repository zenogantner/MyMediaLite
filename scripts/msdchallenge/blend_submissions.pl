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

# TODO
#  - add support for weights
#  - give out overlap
#  - different blending strategies
#  - differentiate between input-n and output-n

use strict;
use warnings;
use 5.8.0;

use English qw( -no_match_vars );
use Getopt::Long;

GetOptions(
	'help' => \(my $help = 0),
	'n'    => \(my $n    = 500),
	'strategy=s' => \(my $strategy = 'count_arank'),
) or usage(-1);

usage(0) if $help;

my @filenames = @ARGV;
my @file_handles = map { open my $fh, '<', $_ or die "Could not open $_\n"; $fh } @filenames;

my %count     = ();
my %avg_rank  = ();
my %best_rank = ();

my %sort_func = (
	count_arank => sub {
		my ($a, $b) = @_;
		if ($count{$a} == $count{$b}) {
			return $avg_rank{$a} <=> $avg_rank{$b};
		}
		return $count{$b} <=> $count{$a};
	},
	arank_count => sub {
		my ($a, $b) = @_;
		if ($avg_rank{$a} == $avg_rank{$b}) {
			return $count{$b} <=> $count{$a};
		}
		return $avg_rank{$a} <=> $avg_rank{$b};
	},
	brank_arank => sub {
		my ($a, $b) = @_;
		if ($best_rank{$a} == $best_rank{$b}) {
			return $avg_rank{$a} <=> $avg_rank{$a};
		}
		return $best_rank{$a} <=> $best_rank{$b};
	},
);

my $sort_func = $sort_func{$strategy};


my $line_count = 0;
LINE:
while (1) {
	my @lines = ();
	foreach my $fh (@file_handles) {
		my $line = <$fh>;
		last LINE if !defined $line;

		push @lines, $line;
	}

	# compute statistics
	%count      = ();
	%best_rank  = ();
	#my %worst_rank = ();
	my %rank_sum   = ();
	foreach my $line (@lines) {
		chomp $line;
		my @items = split / /, $line;
		if (scalar @items != $n) {
			die "Expected exactly $n items per user: '$line'\n";
		}
		for (my $i = 0; $i < scalar @items; $i++) {
			my $item = $items[$i];
			my $rank = $i + 1;
			$count{$item}++;
			$rank_sum{$item} += $rank;
			if (!exists $best_rank{$item} || $rank < $best_rank{$item}) {
				$best_rank{$item} = $rank;
			}
			#if (!exists $worst_rank{$item} || $rank > $worst_rank{$item}) {
			#	$worst_rank{$item} = $rank;
			#}
		}
	}
	%avg_rank = map { $_ => $rank_sum{$_} / $count{$_} } keys %rank_sum;

	# merge
	my @ranked_items = sort { $sort_func->($a, $b) } keys %count;
	my @top_items = @ranked_items[0 .. $n - 1];
	print join(' ', @top_items) . "\n";

	$line_count++;
	print STDERR '.'  if $line_count % 1000 == 0;
	print STDERR "\n" if $line_count % 50_000 == 0;
}
print STDERR "Processed $line_count lines.\n";


sub usage {
	my ($return_code) = @_;

	print << "END";
$PROGRAM_NAME

Blend submission files for the Million Song Dataset challenge

usage: $PROGRAM_NAME [OPTIONS] FILE

  options:
    --help              display this help
    --n                 the length of the item list per user (default is 500)
END
	exit $return_code;
}
