# load assembly
CLR::System::Reflection::Assembly.LoadFrom("../../src/MyMediaLite/bin/Debug/MyMediaLite.dll");

# get types from assembly
#constant EntityMapping       = CLR::("MyMediaLite.Data.EntityMapping,MyMediaLite");
#constant RatingData          = CLR::("MyMediaLite.IO.RatingData,MyMediaLite");
#constant MatrixFactorization = CLR::("MyMediaLite.RatingPrediction.MatrixFactorization,MyMediaLite");
#constant EvalRatings         = CLR::("MyMediaLite.Eval.Ratings,MyMediaLite");

# load the data
my $user_mapping = CLR::("MyMediaLite.Data.EntityMapping,MyMediaLite").new;
my $item_mapping = CLR::("MyMediaLite.Data.EntityMapping,MyMediaLite").new;

my $train_data   = CLR::("MyMediaLite.IO.RatingData,MyMediaLite").Read("../../data/ml100k/u1.base", $user_mapping, $item_mapping);
#my $test_data    = CLR::("MyMediaLite.IO.RatingData,MyMediaLite").Read("../../data/ml100k/u1.test", $user_mapping, $item_mapping);

# set up the recommender
#my $recommender = MatrixFactorization.new;
#$recommender.Ratings = $train_data;
#$recommender.Train();

# measure the accuracy on the test data set
#print EvalRatings.Evaluate($recommender, $test_data);

# make a prediction for a certain user and item
#print $recommender.Predict($user_mapping.ToInternalID(1), $item_mapping.ToInternalID(1));

