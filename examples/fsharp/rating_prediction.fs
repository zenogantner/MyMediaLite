open System
open MyMediaLite.IO
open MyMediaLite.RatingPrediction
open MyMediaLite.Eval

(* load the data *)
let train_data = RatingData.Read "u1.base"
let test_data  = RatingData.Read "u1.test"

(* set up the recommender *)
let recommender = new UserItemBaseline(Ratings=train_data)
recommender.Train()

(* measure the accuracy on the test data set *)
let result = recommender.Evaluate(test_data)
Console.WriteLine(result)

(* make a prediction for a certain user and item *)
let prediction = recommender.Predict(1, 1)
Console.WriteLine(prediction)
