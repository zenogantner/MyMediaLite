# Text filter for Padre, the Perl IDE

# Input is in $_
$_ = $_;
my @pairs = split /\s+(?=\D)/;
@pairs = map { [ split /\s+/] } @pairs;
$_ = join ' ', map { "$_->[0] => '$_->[1]'" } @pairs;
# Output goes to $_
