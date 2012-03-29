#!/usr/bin/perl

# Dataset statistics script
#
# needs around 100 minutes for 30 GB data set containing
# 1,312,919,147 events, 1,327,406 users, 43,964,319 items
# (with --save-memory)

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

use strict;
use warnings;

use English qw( -no_match_vars );
use Getopt::Long;
use POSIX qw(strftime);

my $USER_COLUMN  = 0;
my $ITEM_COLUMN  = 1;
my $EVENT_COLUMN = 2;
my $DATE_COLUMN  = 3;
my $TIME_COLUMN  = 4;

GetOptions(
	   'help'                => \(my $help              = 0),
	   'ignore-lines=i'      => \(my $ignore_lines      = 0),
	   'user-file=s'         => \(my $user_file         = ''),
	   'item-file=s'         => \(my $item_file         = ''),
	   'number-of-columns=i' => \(my $number_of_columns = 0),
	   'user-count-file=s'   => \(my $user_count_file   = ''),
	   'item-count-file=s'   => \(my $item_count_file   = ''),
	   'event-count-file=s'  => \(my $comb_count_file   = ''),
           'save-memory'         => \(my $save_memory       = 0),
	   'separator=s'         => \(my $separator         = '\\s+'),
	   'user-column=i'       => \(my $user_column       = $USER_COLUMN),
	   'item-column=i'       => \(my $item_column       = $ITEM_COLUMN),
           'event-column=i'      => \(my $event_column      = $EVENT_COLUMN),
           'date-column=i'       => \(my $date_column       = $DATE_COLUMN),
           'time-column=i'       => \(my $time_column       = $TIME_COLUMN),
           'hour-offset=i'       => \(my $hour_offset       = 0),
	  ) or usage(-1);

usage(0) if $help;

my $separator_regex = qr{$separator};

my $event_count = 0;
my %event_count_by_user = ();
my %event_count_by_item = ();
my %event_count_by_comb = ();
my %event_level_count   = ();

my %month_count = ();
# TODO time statistics

if ($user_file || $item_file) {
	require File::Slurp;
}
my %watch_user = ();
my %watch_item = ();
%watch_user = map { chomp $_; $_ => 1 } read_file($user_file) if $user_file;
%watch_item = map { chomp $_; $_ => 1 } read_file($item_file) if $item_file;

# skip lines
for (my $i = 0; $i < $ignore_lines; $i++) { <>; }

my $first_date = '9999-99-99';
my $last_date  = '0000-00-00';
my $first_timestamp = 2500000000;
my $last_timestamp  = 0;

LINE:
while (<>) {
    my $line = $_;
    chomp $line;

    # ignore empty lines
    next LINE if $line eq '';

    my @fields = split $separator_regex, $line;
    if ($number_of_columns && scalar @fields != $number_of_columns) {
	print STDERR "Could not parse line: '$line'\n";
	next LINE;
    }

    my $user = $fields[$user_column];
    my $item = $fields[$item_column];

    if (!defined $user || !defined $item) {
	print STDERR "Could not parse line: '$line'\n";
	next LINE;
    }

    next LINE if ($user_file) && !exists $watch_user{$user};
    next LINE if ($item_file) && !exists $watch_item{$item};

    $event_count++;

    if (! exists $event_count_by_user{$user}) {
	$event_count_by_user{$user} = 0;
    }
    if (! exists $event_count_by_item{$item}) {
	$event_count_by_item{$item} = 0;
    }

    $event_count_by_user{$user}++;
    $event_count_by_item{$item}++;

    $event_count_by_comb{"$user $item"}++ if !$save_memory;
    $event_level_count{$fields[$event_column]}++ if !$save_memory && defined $fields[$event_column];

    if ($date_column != -1 && defined $fields[$date_column]) {
	my $date = $fields[$date_column];
	if ($date =~ /^(\d\d\d\d)-(\d\d)-(\d\d)$/) {
	    my ($year, $month, $day) = ($1, $2, $3);
	    $month_count{"$year-$month"}++;

	    $first_date = $date if $date le $first_date;
	    $last_date  = $date if $date ge $last_date;
	}
	elsif ($date =~ /^\d+$/) {
	    my ($sec, $min, $hour, $day_of_month, $month, $year) = gmtime $date;
	    $year += 1900;
	    $month++;
	    $month_count{sprintf "%04d-%02d", $year, $month}++;

	    $first_timestamp = $date if $date < $first_timestamp;
	    $last_timestamp  = $date if $date > $last_timestamp;
	}
	else {
	    die "Could not parse date '$date'. Expected format: YYYY-MM-DD or timestamp\n";
	}
    }

    if ($event_count % 10_000_000 == 0) {
	mini_stats();
    }
}

mini_stats();

if ($user_count_file) {
    write_hash($user_count_file, \%event_count_by_user);
}
if ($item_count_file) {
    write_hash($item_count_file, \%event_count_by_item);
}
if (!$save_memory && $comb_count_file) {
    write_hash($comb_count_file, \%event_count_by_comb);
}

sub mini_stats {
    my $user_count = scalar keys %event_count_by_user;
    my $item_count = scalar keys %event_count_by_item;
    my $comb_count = scalar keys %event_count_by_comb;
    if ($save_memory) {
        print STDERR "$event_count events, $user_count users, $item_count items\n";
    }
    else {
        print STDERR "$comb_count unique events (user-item combinations) out of $event_count, $user_count users, $item_count items\n";
	my $sparsity = 100 * ( $user_count * $item_count - $comb_count ) / ($user_count * $item_count);
	printf STDERR "sparsity: %.4f percent\n", $sparsity;
    }

    if ($date_column && $first_date ne '9999-99-99') {
	print STDERR "first event on $first_date, last event on $last_date\n";
    }

    if ($date_column && $first_timestamp < 2500000000) {
	my ($sec, $min, $hour, $mday, $mon, $year, $wday, $yday, $isdst) = gmtime $first_timestamp;
	$hour += $hour_offset;
	my $first_datetime = strftime '%F %T', $sec, $min, $hour, $mday, $mon, $year;
	($sec, $min, $hour, $mday, $mon, $year, $wday, $yday, $isdst) = gmtime $last_timestamp;
	$hour += $hour_offset;
	my $last_datetime = strftime '%F %T', $sec, $min, $hour, $mday, $mon, $year;
	print STDERR "first event: $first_datetime, last event: $last_datetime\n";
    }

    if (scalar keys %event_level_count > 0) {
	my %event_level_freq = map { $_ => sprintf("%s\t(%.4f)", $event_level_count{$_}, $event_level_count{$_} / $event_count) } keys %event_level_count;
	print STDERR "Event levels and frequencies:\n";
	print_hash(\%event_level_freq);
    }

    if (scalar keys %month_count > 0) {
	print STDERR "Event frequencies by month:\n";
	print_hash(\%month_count);
    }

}

sub contains_only_numbers {
    foreach my $scalar (@_) {
	return 0 if $scalar !~ /^-?\d+\.?\d*$/
    }
    return 1;
}

sub print_hash {
    my ($hash_ref) = @_;

    my @keys = sort keys %$hash_ref;
    if (contains_only_numbers(@keys)) {
	@keys = sort { $a <=> $b } @keys
    }
    foreach my $key (@keys) {
	print STDERR "  $key:\t$hash_ref->{$key}\n";
    }
}

sub write_hash {
    my ($filename, $hash_ref) = @_;

    open(my $FH, '>', $filename) or die $!;

    foreach my $key (sort { $hash_ref->{$b} <=> $hash_ref->{$a} } keys %$hash_ref) {
	print $FH "$key\t$hash_ref->{$key}\n";
    }
}

sub usage {
    my ($return_code) = @_;

    print << "END";
$PROGRAM_NAME

compute and show basic dataset statistics

usage: $PROGRAM_NAME [OPTIONS] [INPUT]

    --help                   display this usage information
    --ignore-lines=N         ignore the first N lines
    --number-of-columns=N    check whether there are exactly N columns in each line
    --user-count-file=FILE   write user IDs sorted by event count to FILE
    --item-count-file=FILE   write item IDs sorted by event count to FILE
    --event-count-file=FILE  write user/item IDs sorted by combined event count to FILE
    --save-memory            save memory use by not counting unique events, deactivates --event-count-file
    --separator=REGEX        the separator regex used to split the lines into columns,
                             default is \\s+ (one or more whitespace characters)
    --user-column=N          specifies the user column (0-based), default is $USER_COLUMN
    --item-column=N          specifies the item column (0-based), default is $ITEM_COLUMN
    --event-column=N         specifies the event column (0-based), default is $EVENT_COLUMN
    --date-column=N          specifies the date column (0-based), default is $DATE_COLUMN; -1 if no date column
    --time-column=N          specifies the time column (0-based), default is $TIME_COLUMN
    --user-file=FILE         only get statistics for users listed in FILE (requires File::Slurp)
    --item-file=FILE         only get statistics for items listed in FILE (requires File::Slurp)
    --hour-offset            difference of the times to GMT
END
    exit $return_code;
}
