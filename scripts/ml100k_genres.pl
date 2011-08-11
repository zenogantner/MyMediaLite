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

# ml100k_genres.pl u.item > item-attributes-genres.txt

use strict;
use warnings;

use Carp;
use English qw( -no_match_vars );

while (<>) {
    my $line = $_;
    chomp $line;

    my @fields = split /\|/, $line;
    
    if (scalar @fields != 24) {
        croak "Could not parse line: '$line'\n";
    }

    my ($movie_id, $movie_title, $movie_date, $movie_url, @movie_genres) = @fields;

    for (my $i = 0; $i < scalar @movie_genres; $i++) {
	print "$movie_id\t$i\n" if $movie_genres[$i] eq '1';
    }
}
