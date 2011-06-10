#!/usr/bin/perl

# TODO add license
# TODO create CPAN module

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

print "Uploading ratings ... ";
my @lines = read_file('data/ml100k/u1.base');
foreach my $line (@lines) {
  my ($user, $item, $rating) = split /\s+/, $line;
  $add->($user, $item, $rating);
}
print "done.\n";

print "training ... ";
my $result = $soap->Train;
die join ', ', $result->faultcode, $result->faultstring if $result->fault;
print "done.\n";
