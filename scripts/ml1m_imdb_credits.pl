#!/usr/bin/perl

# Copyright 2007, 2008, 2009, 2010 Zeno Gantner
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

# ml1m_imdb_credits.pl --help
# ml1m_imdb_credits.pl --imdb-dir=/tmp --imdb-credit=directors --ml-movie-file=movies.dat

use strict;
use warnings;
use utf8;
binmode STDOUT, ':utf8';
binmode STDERR, ':utf8';
use Carp;
use English qw( -no_match_vars ); $OUTPUT_AUTOFLUSH = 1;
use Getopt::Long;

# TODO write code to extract keywords, languages, genres, etc.
# TODO fix all UTF-8/encoding problems
# TODO use a Perl HTTP method instead of wget
# TODO use a Perl unzip method instead of zcat

my $IMDB_URL   = 'ftp://ftp.sunet.se/pub/tv+movies/imdb/';
my $IMDB_DIR   = '/tmp';
my $OUTPUT_DIR = './';

# Global variables
my @imdb_credit_files = ();

GetOptions(
	   'help'                 => \(my $help               = 0),
	   'verbose'              => \(my $verbose            = 0),
	   'ml-movie-file=s'      => \(my $ml_movie_file      = 'movies.dat'),
	   'splitter=s'           => \(my $splitter           = '::'),
	   'imdb-credits=s'       => \@imdb_credit_files,
	   'output-person-file=s' => \(my $output_person_file = 'imdb-attributes.txt'),
	   'attribute-file=s'     => \(my $attribute_file     = 'item-attributes-imdb.txt'),
	   'n=i'                  => \(my $ignore_n_persons   = 1),
	   'imdb-dir=s'           => \(my $imdb_dir           = $IMDB_DIR),
	   'imdb-url=s'           => \(my $imdb_url           = $IMDB_URL),
	   'output-dir=s'         => \(my $output_dir         = $OUTPUT_DIR),
	   'sparse-output!'       => \(my $sparse_output      = 1),
	   'auto-download'        => \(my $auto_download      = 0),
	  ) or usage(-1);

usage(0) if $help;

my %movie_ids = (); # %movie_ids is a hash of list references, because there are duplicate movies in the ML dataset
open my $ML_FILE, '<', $ml_movie_file
    or croak "Can't open '$ml_movie_file': $!"; # TODO use English
READ_ML_FILE:
while (<$ML_FILE>) {
    my $line = $_;

    next if $line =~ /^(\s)*$/;
    chomp $line;

    print "$line\n" if $verbose;

    my @fields = split /$splitter/, $line;

    if (scalar @fields >= 2) {
        my ($movie_id, $movie_title) = @fields;

        if (exists $movie_ids{$movie_title}) {
            push @{$movie_ids{$movie_title}}, $movie_id
        }
        else {
            $movie_ids{$movie_title} = [$movie_id];
        }
    }
    else {
        croak "Could not parse line '$line'\n";
    }
}
close $ML_FILE;

my %person_movies = ();
foreach my $imdb_credit_file (@imdb_credit_files) {
    $imdb_credit_file = "$imdb_credit_file.list.gz";
    my $IMDB_FILE; # file handle

  IMDB_SEEK_DATA:
    while (1) {
	open ($IMDB_FILE, "zcat $imdb_dir/$imdb_credit_file |")
	  or croak "Can't open ' $imdb_dir/$imdb_credit_file': $!";

	while (<$IMDB_FILE>) {
	    my $line = $_;

	    if ($line =~ /^THE .+ LIST$/) {
		# read four more lines
		$line = <$IMDB_FILE>;    # ===
		$line = <$IMDB_FILE>;    #
		$line = <$IMDB_FILE>;    # Name   Titles
		$line = <$IMDB_FILE>;    # ----
		last IMDB_SEEK_DATA;
	    }
	}

        if (!$auto_download) {
            print STDERR "Failed to find data in '$imdb_credit_file'.\n";
            print STDERR "Press ENTER to download (requires wget), Ctrl-C to abort.\n";
            <STDIN>;
        }
	my $command = "wget --directory-prefix=$imdb_dir $imdb_url/$imdb_credit_file";
	print STDERR "$command\n";
	my $result = system $command;
	if ($result != 0) {
	    die "Error while downloading.";
	}
    }

    print STDERR "Reading in $imdb_credit_file ...\n" if $verbose;

    my $line_counter = 0;
    my $person;
  IMDB_READ_DATA:
    while (<$IMDB_FILE>) {
        next IMDB_READ_DATA if /^\s*$/;
        last IMDB_READ_DATA if /^--------/;

        $line_counter++;
        if ($line_counter % 20000 == 0) {
            print STDERR '.' if $verbose;
        }
        if ($line_counter % 1200000 == 0) {
            print STDERR "\n" if $verbose;
        }

        my $line = $_;
        chomp $line;

        # TODO: use appropriate module
        # TODO: extended regex
        # note: there are some errors in the IMDB data (tabs that should not be there)
        #       thus, the string movie_data will not always carry _all_ data.
        my $movie_data;
        if ($line =~ /^([^\t]+)\t+([^\t]+)/) {
            $person     = $1;
            $movie_data = $2;
        }
        elsif ($line =~ /^\t+([^\t]+)/) {
            $movie_data = $1;
        }
        else {
            print STDERR "Could not parse line '$line'\n";
            next IMDB_READ_DATA;
        }

        # ignore TV shows
        if (substr($movie_data, 0, 1) eq '"') {
            next IMDB_READ_DATA;
        }

        my $movie_title;
        # TODO: change into extended regex
        if ($movie_data =~ /^(.+\((\d\d\d\d|\?\?\?\?)(\/[IVX]+)?\))((  .+)|\(\w+\))?/) {
            $movie_title = $1;
        }
        else {
            warn "Could not get movie title from this string: '$movie_data'\n";
            next IMDB_READ_DATA;
        }

        if (exists $movie_ids{$movie_title}) {
            my @movie_ids = @{$movie_ids{$movie_title}};
            foreach my $movie_id (@movie_ids) {
                $person_movies{$person}->{$movie_id} = 1;
            }
        }
    }
    print STDERR "\n" if $verbose;
    close $IMDB_FILE;
}

# create IDs for all the relevant persons
my $person_id     = -1;
my %person_id     = ();
my %movie_persons = ();
foreach my $id_list_ref (values %movie_ids) {
    foreach my $movie_id (@$id_list_ref) {
        $movie_persons{$movie_id} = [];
    }
}
foreach my $person (sort(keys %person_movies)) {
    if (scalar keys %{$person_movies{$person}} > $ignore_n_persons) {
        $person_id{$person} = ++$person_id;

        foreach my $movie_id (keys %{$person_movies{$person}}) {
            push @{$movie_persons{$movie_id}}, $person_id;
        }
    }
}

# write person data to file
$output_person_file = "$output_dir/$output_person_file" if $output_dir;
open my $PERSON_FILE, '>', $output_person_file
    or croak "Can't open '$output_person_file' for writing: $!"; # TODO: use English
my %person_occurrences  = ();
foreach my $person (sort(keys %person_id)) {
    my $person_count = scalar(keys %{$person_movies{$person}});
    print {$PERSON_FILE} "$person\t$person_id{$person}\t$person_count\n";
    $person_occurrences{$person_count}++;
}
close $PERSON_FILE;

# write movie-attribute mappings to file
$attribute_file = "$output_dir/$attribute_file" if $output_dir;
open my $MATRIX_FILE, '>', $attribute_file
    or croak "Can't open '$attribute_file' for writing: $!";
foreach my $movie_id (sort({$a <=> $b} keys %movie_persons)) {
    if (not $sparse_output) {
	my @attributes = @{$movie_persons{$movie_id}};

	my @encoded_attributes = (0) x scalar keys %person_id;
	if (scalar @{$movie_persons{$movie_id}} == 0) {
	    print STDERR "Warning: Movie $movie_id has no associated persons.\n";
	}
	foreach my $person_id (@{$movie_persons{$movie_id}}) {
	    $encoded_attributes[$person_id] = 1;
	}
	
	my $encoded_attributes = join ' ', @encoded_attributes;
	#print STDERR scalar @encoded_attributes . "\n";
	print {$MATRIX_FILE} "$movie_id\t$encoded_attributes\n";
    }
    else {
	foreach my $person_id (@{$movie_persons{$movie_id}}) {
	    print {$MATRIX_FILE} "$movie_id\t$person_id\n";
	}
    }
}
close $MATRIX_FILE;


my $number_of_different_movies = scalar keys %movie_ids;
my $number_of_persons          = scalar keys %person_id;
print STDERR "$number_of_different_movies movies and $number_of_persons persons.\n";

foreach my $number (sort({$a <=> $b} keys %person_occurrences)) {
    print STDERR "$person_occurrences{$number} occur(s) $number times\n";
}

sub usage {
    my ($return_code) = @_;

    print << "END";
$PROGRAM_NAME

generate an attribute file for the MovieLens-1M dataset from IMDB data files

usage: $PROGRAM_NAME [OPTIONS] [INPUT]

    --help                     display this usage information
    --verbose                  verbose output
    --ml-movie-file=FILE'      default: 'movies.dat'
    --splitter=REGEX           default: /::/
    --imdb-credits=FILE        (can be used several times)
    --output-person-file=FILE  default: 'imdb-attributes.txt'
    --attribute-file=FILE      default: 'item-attributes-imdb.txt'
    --n=i                      minimum number of times an attribute has to occur (-1), default: 1
    --imdb-dir=DIR             default: '$IMDB_DIR'
    --imdb-url=URL             default: '$IMDB_URL'
    --auto-download            don't ask before downloading data from IMDB
    --output-dir=DIR           default: '$OUTPUT_DIR'
    --no-sparse-output
END
    exit $return_code;
}
