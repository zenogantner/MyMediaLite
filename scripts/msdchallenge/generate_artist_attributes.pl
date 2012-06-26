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

use utf8;
binmode(STDOUT, ':utf8');
binmode(STDIN,  ':utf8');

while (<>) {
	my @fields = split/<SEP>/;

	my $song_id = $fields[1];
	my $artist  = $fields[2];

	$artist =~ tr/ /_/;
	$artist =~ tr/ÉÈÊÁÀÂÓÒÔÚÙÛÍÌÎ/EEEAAAOOOUUUIII/;
	$artist =~ tr/ëéèêáàâóòôøúùûíìîçñ/eeeeaaaoooouuuiiicn/;
	$artist =~ s/æ/ae/g;

	$artist = lc $artist;
	$artist =~ s/&/and/g;

	$artist =~ s/d\.j\./dj/g;
	$artist =~ s/all[ -]stars/allstars/g;
	$artist =~ s/_\(?\[?(introducing|duet|duett|featuring|feat\.|ft\.|w\/|w\.|mit|with|avec|con|prod\._by|remixed_by)_.*//;
	$artist =~ s/^the_//;
	$artist =~ s/_jr\./_jr/g;
	$artist =~ s/_\(karaoke\)$//;
	$artist =~ s/;.*//g;
	$artist =~ s/_\/_.*//;
	$artist =~ s/_-_.*//;
	$artist =~ s/"/_/g;
	$artist =~ s/'_/_/g;
	$artist =~ s/_'/_/g;
	$artist =~ s/(\w)'s/$1s/g;
	$artist =~ s/_+/_/g;
	$artist =~ s/_$//;

	print "$song_id $artist\n";
}
