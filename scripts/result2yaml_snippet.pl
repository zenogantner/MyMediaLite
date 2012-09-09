#!/usr/bin/perl

use strict;
use warnings;

# AUC 0.93478 prec@5 0.66375 prec@10 0.64199 MAP 0.48095 recall@5 0.06493 recall@10 0.12464 NDCG 0.79952 MRR 0.77968
while (<>) {
    my $line = $_;
    chomp $line;
    $line =~ s/@//g;
    
    my @fields = split / /, $line;
    my %result = @fields;
    print join(', ', map { "$_ => '$result{$_}'" } (sort keys %result)) . "\n";
}