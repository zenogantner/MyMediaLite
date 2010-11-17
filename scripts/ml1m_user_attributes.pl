#!/usr/bin/perl

# (c) 2009, 2010 by Zeno Gantner <zeno.gantner@gmail.com>
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

# ml1m_user_attributes.pl --help
# ml1m_user_attributes.pl users.dat > user-attributes-nozip.txt

use strict;
use warnings;

use English qw( -no_match_vars );
use File::Slurp;
use Getopt::Long;

GetOptions(
    'help'           => \( my $help           = 0 ),
    'age!'           => \( my $use_age        = 1 ),
    'occupation!'    => \( my $use_occupation = 1 ),
    'gender!'        => \( my $use_gender     = 1 ),
    'zipcode'        => \( my $use_zipcode    = 0 ),
    'sparse-output!' => \( my $sparse_output  = 1 ),
) or usage(-1);

usage(0) if $help;

my @age_values        = qw( 1 18 25 35 45 50 56 );
my %age_value         = ( 1 => 0, 18 => 1, 25 => 2, 35 => 3, 45 => 4, 50 => 5, 56 => 6);
my @occupation_values = 0 .. 20;
my @zipcode_values    = 01 .. 99;

my $counter = 0;

my $num_attributes;
while (<>) {
    my $line = $_;
    chomp $line;

    my @fields = split '::', $line;
    if ( @fields != 5 ) {
        die "Could not parse line: '$line'\n";
    }

    my ( $user_id, $gender, $age, $occupation, $zipcode ) = @fields;

    if ( $gender eq 'M' ) {
        $gender = 1;
    }
    elsif ( $gender eq 'F' ) {
        $gender = 0;
    }
    else {
        die "Invalid value for gender: '$gender', must be either 'M' or 'F'\n";
    }

    $zipcode = substr $zipcode, 0, 2;

    if ($sparse_output) {
        my $attr_id = 0;
        if ($use_gender) {
            print "$user_id\t$gender\n";
            $attr_id = 2;
        }
        if ($use_age) {
            my $age_id = $age_value{$age} + $attr_id;
            print "$user_id\t$age_id\n";
            $attr_id += scalar keys %age_value;
        }
        if ($use_occupation) {
            my $occupation_id = $attr_id + $occupation;
            print "$user_id\t$occupation_id\n";
            $attr_id += scalar @occupation_values;
        }
        if ($use_zipcode) {
            my $zipcode_id = $attr_id + $zipcode;
            print "$user_id\t$zipcode_id\n";
            $attr_id += 100;
        }
        $num_attributes = $attr_id;
    }
    else {
        my @encoded_age = map { $_ == $age ? '1' : '0' } @age_values;
        my @encoded_occupation =
          map { $_ == $occupation ? '1' : '0' } @occupation_values;
        my @encoded_zipcode = map { $_ == $zipcode ? '1' : '0' } @zipcode_values;

        my @attributes = (
            $use_gender     ? $gender             : (),
            $use_age        ? @encoded_age        : (),
            $use_occupation ? @encoded_occupation : (),
            $use_zipcode    ? @encoded_zipcode    : (),
        );
        my $attributes = join ' ', @attributes;
        $num_attributes = scalar @attributes;
        print "$user_id\t$attributes\n";
    }
}

print STDERR "Number of attributes: $num_attributes\n";

sub usage {
    my ($return_code) = @_;

    print << "END";
$PROGRAM_NAME

generate a binary user attribute file for the MovieLens-1M dataset

usage: $PROGRAM_NAME [OPTIONS] [INPUT]

    --help                     display this usage information
    --no-age
    --no-occupation
    --no-gender
    --zipcode
    --no-sparse-output
END
    exit $return_code;
}
