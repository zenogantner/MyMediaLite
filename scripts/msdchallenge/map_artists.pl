#!/usr/bin/perl

# ./map_artists.pl --mappings=artist_mapping.csv < song_int_id2artist4 > song_int_id2artist_mapped4
# /home/zgantner/src/kaggle/msdchallenge/scripts/map_artists.pl --mappings=$HOME/src/kaggle/msdchallenge/data/artist_mapping.csv < song_int_id2artist4 > song_int_id2artist_mapped4

use File::Slurp;
use Getopt::Long;

GetOptions(
	'mappings=s' => \(my $mapping_file = ''),
) or die "Wrong args\n";

my %artist_map = read_hash($mapping_file);

my $map_count = 0;
while (<>) {
	my $line = $_;
	chomp $line;
	my ($song_id, $artist) = split / /, $line;

	if (exists $artist_map{$artist}) {
		$artist = $artist_map{$artist};
		$map_count++;
	}
	print "$song_id $artist\n";
}
print STDERR "$map_count replacements\n";


sub read_hash {
	my ($filename) = @_;

	my @lines = read_file($filename);
	my %hash = ();
	LINE:
	foreach my $line (@lines) {
		chomp $line;
		next LINE if $line eq '';
		next LINE if $line =~ /^#/;
		
		my @fields = split /,/, $line;
		if (scalar @fields == 2) {
			$hash{$fields[0]} = $fields[1];
		}
		else {
			die "Could not parse line '$line'\n";
		}
	}

	return %hash;
}
