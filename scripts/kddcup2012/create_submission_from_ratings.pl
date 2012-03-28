#!/usr/bin/perl

#
# Create a submission file from a normal MyMediaLite rating file
# Script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.
#

use strict;
use warnings;

use Getopt::Long;

GetOptions(
	'one-column' => \( my $one_column = 0 ),
) or die "Could not understand command line parameters.\n";

my %leaderboard_score = ();
my %final_score = ();

print STDERR "Reading in data ...\n";
# ... and assign to correct evaluation part
my $line_number = 0;
while (<>) {
	my ($user_id, $item_id, $score) = split /\t/;
	if (++$line_number < 19349609) {
		$leaderboard_score{$user_id}->{$item_id} = $score;
	}
	else {
		$final_score{$user_id}->{$item_id} = $score;
	}
}

print "id,clicks\n" if !$one_column;
sort_and_write('leaderboard', \%leaderboard_score);
sort_and_write('final',       \%final_score);

sub sort_and_write {
	my ($name, $data_ref) = @_;
	print STDERR "Sorting and writing out $name predictions ...\n";
	foreach my $user_id (sort { $a <=> $b } keys %$data_ref) {
		my @ranked_items = sort { $data_ref->{$user_id}->{$b} <=> $data_ref->{$user_id}->{$a} } keys %{$data_ref->{$user_id}};
		my @top_items = scalar @ranked_items >= 3 ? @ranked_items[0 .. 2] : @ranked_items;
		my $ranked_items = join ' ', @top_items;
		if ($one_column) {
			print "$ranked_items\n";
		}
		else {
			print "$user_id,$ranked_items\n";
		}
	}
}
