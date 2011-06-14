#!/usr/bin/perl

use strict;
use warnings;

use English qw( -no_match_vars );
use File::Slurp;
use SOAP::Lite;

$OUTPUT_AUTOFLUSH = 1;

my $soap = SOAP::Lite
-> uri('http://ismll.de/RatingService')
-> on_action( sub { join '/', 'http://ismll.de/RatingService', $_[1] } )
-> proxy('http://localhost:8080/RatingService.asmx');

my $predict_method = SOAP::Data->name('Predict')->attr({xmlns => 'http://ismll.de/RatingService'});
my $predict = sub {
  my ($user, $item) = @_;
  
  my $result = $soap->call(
    $predict_method,
    SOAP::Data->name(user_id => $user   ),
    SOAP::Data->name(item_id => $item   ),  
  );
  die join ', ', $result->faultcode, $result->faultstring if $result->fault;  

  return $result->result;
};

print "Evaluating ratings (takes about 3.5 minutes) ... ";
my @test_lines = read_file('data/ml100k/u1.test');
my $error_sum = 0;
foreach my $line (@test_lines) {
  my ($user, $item, $rating) = split /\s+/, $line;

  my $prediction = $predict->($user, $item);
  $error_sum += ($rating - $prediction) ** 2;
}
my $rmse = sqrt( $error_sum / scalar @test_lines );
print " RMSE $rmse\n";
