#!/usr/bin/perl

# (c) 2009, 2010, 2011 by Zeno Gantner
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

# ml1m_genres.pl movies.dat > item-attributes-genres.txt

use strict;
use warnings;

use Carp;
use English qw( -no_match_vars );
use Getopt::Long;

GetOptions(
	   'help'           => \(my $help          = 0),
	   'sparse-output!' => \(my $sparse_output = 1),
	  ) or usage(-1); ## TODO implement usage

my @genres = (
	      'Action', 'Adventure', 'Animation',
	      "Children's", 'Comedy', 'Crime',
	      'Documentary', 'Drama',
	      'Fantasy', 'Film-Noir', 'Horror',
	      'Musical', 'Mystery', 'Romance', 'Sci-Fi', 'Thriller', 'War', 'Western'
	     );
my $counter = 0;
my %genre_id = map { $_ => $counter++ } @genres;

# fix for MovieLens 10M
$genre_id{Children} = $genre_id{"Children's"};
$genre_id{IMAX}     = $counter++;

while (<>) {
    my $line = $_;
    chomp $line;

    my @fields = split '::', $line;
    if (scalar @fields != 3) {
	croak "Could not parse line: '$line'\n";
    }

    my ($movie_id, $movie_title, $movie_genres) = @fields;

    next if $movie_genres eq '(no genres listed)';

    my @movie_genres    = split /\|/, $movie_genres;
    my @movie_genre_ids = map { $genre_id{$_} } @movie_genres;

    if ($sparse_output) {
	foreach my $genre_id (@movie_genre_ids) {
	    if (defined $genre_id) {
		print "$movie_id\t$genre_id\n";
	    }
	    else {
		print STDERR "Unknown genre in line '$line'\n";
	    }
	}
    }
    else {
	my @encoded_genres = (0) x scalar(@genres);
	foreach my $genre_id (@movie_genre_ids) {
	    $encoded_genres[$genre_id] = 1;
	}
	my $encoded_genres = join ' ', @encoded_genres;

	print "$movie_id\t$encoded_genres\n";
    }
}
