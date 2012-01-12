#!/usr/bin/perl

# takes the output of MyMedia executables called with --find-iter and turns it into fancy graphics with the help of R

use 5.010;
use strict;
use warnings;

use File::Slurp;
use Getopt::Long;
use List::Util qw(min max);
use Regexp::Common qw(number);

my @legend_filter = ();

GetOptions(
	'height=f'         => \(my $img_height = 10),
	'width=f'          => \(my $img_width = 10),
	'plot-type=s'      => \(my $plot_type = 'b'),
	'fit'              => \(my $fit),
	'measure=s'        => \(my $measure = ''),
	'output-file=s'    => \(my $output_file = 'out.pdf'),
	'legend-filter=s'  => \@legend_filter,
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

say scalar @legend_filter;
if (@legend_filter) {
	my $legend_filter_regex = '(?:' . (join '|', @legend_filter) . ')=(?:\w+|$RE{num}{real})';
	
	foreach my $key (keys %method) {
		my @filtered_method = ();
		while ($method{$key} =~ s/($legend_filter_regex)//) {
			push @filtered_method, $1;
		}
		$method{$key} = join ' ', @filtered_method;
	}
}
my $methods = '"' . (join '", "', map { $method{$_} } (sort keys %method)) . '"';

my $measure_max = max (map @$_, (values %results));
my $start_epoch = min (map @$_, (values %epochs));
my $end_epoch   = max (map @$_, (values %epochs));

say <<"END";
img_height <- $img_height
img_width  <- $img_width

methods <- c($methods)
colors <- rainbow($i)
pdf(file="$output_file", width=img_width, height=img_height)
plot(e0, v0, type="$plot_type", ylab="$measure", xlab="epoch", col=colors[1], xlim=c($start_epoch, $end_epoch), ylim=c(0, $measure_max))
END

foreach my $j (1 .. $i - 1) {
	my $color_index = $j + 1;
	say "points(e$j, v$j, type=\"$plot_type\", col=colors[$color_index])";
}

say 'legend(x="topright", methods, col=colors)';

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
		chomp $line;
		next LINE if $line =~ /^\s*$/;
		next LINE if $line =~ /^training data:/;
		next LINE if $line =~ /^test data:/;
		
		if ($line =~ /=/) {
			$line =~ s/,//;
			$method = $line;
			next LINE;
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
		return ($value, $iteration);
	}
	else {
		warn "Could not parse line '$line' measure: '$measure'\n";
	}
}