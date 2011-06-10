#!/usr/bin/perl

use strict;
use warnings;

use English qw( -no_match_vars );
use File::Slurp;
use SOAP::Lite;

$OUTPUT_AUTOFLUSH = 1;

print "Preparing SOAP object ... ";
my $soap = SOAP::Lite
-> uri('http://ismll.de/RatingService')
-> on_action( sub { join '/', 'http://ismll.de/RatingService', $_[1] } )
-> proxy('http://localhost:8080/RatingService.asmx');
print "done.\n";


print "Uploading ratings ... ";
foreach my $line (read_file('data/ml100k/u1.base')) {
  my ($user, $item, $rating) = split /\s+/, $line;
  
  my $result = $soap->AddFeedbackNoTraining($user, $item, $rating);
  die join ', ', $result->faultcode, $result->faultstring if $result->fault;
}
print "done.\n";

print "training ... ";
my $result = $soap->Train;
die join ', ', $result->faultcode, $result->faultstring if $result->fault;
print "done.\n";

print "Evaluating ratings ... ";
my @test_lines = read_file('data/ml100k/u1.test');
my $error_sum = 0;
foreach my $line (@test_lines) {
  my ($user, $item, $rating) = split /\s+/, $line;
  
  my $result = $soap->Predict($user, $item);
  die join ', ', $result->faultcode, $result->faultstring if $result->fault;
  
  $error_sum += ($rating - $result->result) ** 2;
}
my $rmse = sqrt( $error_sum / scalar @test_lines );
print " RMSE $rmse\n";
