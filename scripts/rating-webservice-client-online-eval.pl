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

my $add_method = SOAP::Data->name('AddFeedbackNoTraining')->attr({xmlns => 'http://ismll.de/RatingService'});
my $add = sub {
  my ($user, $item, $rating) = @_;
  
  my $result = $soap->call(
    $add_method,
    SOAP::Data->name(user_id => $user   ),
    SOAP::Data->name(item_id => $item   ),
    SOAP::Data->name(score   => $rating ),    
  );
  die join ', ', $result->faultcode, $result->faultstring if $result->fault;  
};

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

print "Evaluating ratings ... ";
my @test_lines = read_file('data/ml100k/u1.test');
my $error_sum = 0;
foreach my $line (@test_lines) {
  my ($user, $item, $rating) = split /\s+/, $line;

  my $prediction = $predict->($user, $item);
  print $prediction . "\n";
  $error_sum += ($rating - $prediction) ** 2;
  
  $add->($user, $item, $rating);
}
my $rmse = sqrt( $error_sum / scalar @test_lines );
print " RMSE $rmse (online)\n";
