#!/usr/bin/perl

# takes the output of MyMedia executables called with --find-iter and turns it into fancy graphics with the help of R

use 5.010;
use strict;
use warnings;

use File::Slurp;
use Getopt::Long;
use Regexp::Common qw /number/;

GetOptions(
	'height=f'      => \(my $img_height = 4.4),
	'width=f'       => \(my $img_width = 4.4),
	'plot-type=s'   => \(my $plot_type = 'b'),
	'fit'           => \(my $fit),
	'measure=s'     => \(my $measure = ''),
	'output-file=s' => \(my $output_file = 'out.pdf'),
) or die 'Error while parsing command line parameters';

my @files = @ARGV;

my %results = ();
my %epochs = ();
my %method = ();

foreach my $file (@files) {
	 my ($method, $results_ref, $epochs_ref) = process_file($file);
	 $results{$file} = $results_ref;
	 $epochs{$file}  = $epochs_ref;
	 $method{$file}  = $method;
}

my $i = 0;
foreach my $key (sort keys %results) {
	say "v$i <- " . as_r_vector($results{$key});
	say "e$i <- " . as_r_vector($epochs{$key});
	say "m$i <- '$method{$key}'";
	
	$i++;
}

say <<"END";
img_height <- $img_height
img_width  <- $img_width

colors <- rainbow($i)
pdf(file="$output_file", width=img_width, height=img_height)
plot(v0, e0, type="$plot_type", ylab="$measure", xlab="epoch", col=colors[1])
END

foreach my $j (1 .. $i - 1) {
	my $color_index = $j + 1;
	say "points(v$j, e$j, type=\"$plot_type\", col=colors[$color_index])";
}


# turns an array ref into an R vector
sub as_r_vector {
	my ($array_ref) = @_;
	
	return 'c(' . (join ', ', @$array_ref). ')';
}

# turns an array of array references (row-major) into an R matrix
sub as_r_matrix {
	my ($array_ref) = @_;

 	my $num_rows = scalar @$array_ref;
	my $num_cols = scalar @{$array_ref->[0]};
	
 	my $result = "matrix(\n";
	$result += "\tc(\n";
	$result +=  "\t\t";
	$result += join ",\n\t\t", map { join ', ', @$_ } @$array_ref;
	$result +=  "\n\t),";
	$result += "\tncol=$num_cols, nrow=$num_rows, byrow=TRUE\n";
	$result += ")\n";
	
	return $result;
}

sub process_file {
	my ($filename) = @_;
	
	my @lines = read_file $filename;
	
	my $method = '';
	my @epochs  = ();
	my @results = ();
	
	LINE:
	foreach my $line (@lines) {
		next LINE if $line =~ /^training data:/;
		next LINE if $line =~ /^test data:/;
		
		if ($line =~ /=/) {
			$line =~ s/,//;
			$method = $line;
		}
		
		if ($line =~ s/fit: //) {
			next LINE if !$fit;
		}
		else {
			next LINE if $fit;
		}

		my ($value, $it) = process_line($line);
		push @results, $value;
		push @epochs, $it;
	}
	
	return ($method, \@results, \@epochs);
}


sub process_line {
	my ($line) = @_;
	
	chomp $line;
	
	if ($line =~ m/$measure ($RE{num}{real}).*iteration ($RE{num}{int})\s*/) {
		my $value     = $1;
		my $iteration = $2;
		return ($iteration, $value);
	}
	else {
		warn "Could not parse line '$line' measure: '$measure'\n";
	}
}