#!/usr/bin/perl

#
# Evaluation script for KDD Cup 2012, track 1
# author: Zeno Gantner <zeno.gantner@gmail.com>
# This script is in the public domain.
#

use strict;
use warnings;

use Getopt::Long;

GetOptions(
    'prediction-file=s' 	=> \( my $prediction_file = 'pred' ),
    'groundtruth-file=s' 	=> \( my $ground_truth_file = 'groundtruth' ),
) or die "Could not understand command line parameters.\n";

my $sep = qr{,|\t};

print STDERR "Reading in predictions ...\n";
open my $fh, "<", $prediction_file 
	or die "cannot open < $prediction_file: $!";
my %predicted_items_by_user = ();
while (<$fh>) {
	my $line = $_;
	my ($user_id, $item_list) = split /$sep/, $line;
	my @item_list = split / /, $item_list;
	$predicted_items_by_user{$user_id} = [@item_list];
}
close $fh;

print STDERR "Reading in ground truth ...\n";
open my $fh2, "<", $ground_truth_file 
	or die "cannot open < $ground_truth_file: $!";
my %accessed_items_by_user = ();
while (<$fh2>) {
	my $line = $_;
	my ($user_id, $item_id, $result, $timestamp) = split /$sep/, $line;
	if (!exists $accessed_items_by_user{$user_id}) {
		$accessed_items_by_user{$user_id} = {};
	}
	if ($result == 1) {
		$accessed_items_by_user{$user_id}->{$item_id} = 1;
	}
}
close $fh2;

# consistency check
my $num_prediction_users = scalar keys %predicted_items_by_user;
my $num_validation_users = scalar keys %accessed_items_by_user;
if ($num_prediction_users != $num_validation_users) {
	warn "$num_prediction_users users with predictions vs. $num_validation_users users with ground truth\n";
}

print STDERR "Evaluating ...\n";
my $map_at_3 = 0;
foreach my $user_id (keys %accessed_items_by_user) {
	if (! exists $predicted_items_by_user{$user_id}) {
		die "Predictions for user $user_id are missing\n";
	}
	
	my $hits = 0;
	my $ap = 0;
	for (my $i = 0; $i < 3 && $i < scalar @{$predicted_items_by_user{$user_id}}; $i++) {
		my $item_id = $predicted_items_by_user{$user_id}->[$i];
		if (exists $accessed_items_by_user{$user_id}->{$item_id}) {
			$hits++;
			$ap += $hits / ($i + 1);
		}
	}
	my $num_accessed = scalar keys %{$accessed_items_by_user{$user_id}};
	if ($num_accessed > 0) {
		$ap /= $num_accessed;
	}
	$map_at_3 += $ap;
}
$map_at_3 /= $num_validation_users;
printf "%.6f\n",  $map_at_3;
