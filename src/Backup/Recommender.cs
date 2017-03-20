// Copyright (C) 2012 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Data;
using MyMediaLite;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Data;
using MyMediaLite.Diversification;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using MyMediaLite.Correlation;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.IO;

namespace JSONAPI
{
	public class Recommender
	{
		//public TextWriter writer = new StreamWriter("addedratings.log");
		public static Recommender Instance = new Recommender();
		public EntityMapping user_mapping = new EntityMapping();
		public EntityMapping item_mapping = new EntityMapping();
		public LatentFeatureDiversfication diversifier;
		private static MatrixFactorization recommender;
		private static Timer _timer;
		private static MyMediaLite.Data.StackableRatings r = new MyMediaLite.Data.StackableRatings();

		private static Object _locker = new Object();
		static void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			StackableRatings stack;
			lock (_locker) {
				stack = r;
				r = new StackableRatings ();
			}
			_timer.Enabled = false;
			Console.WriteLine (DateTime.Now + " Adding " + stack.Count + " ratings");
			recommender.AddRatings (stack);
			Console.WriteLine (DateTime.Now + " Added " + stack.Count + " ratings");
			_timer.Enabled = true;
    	}

		public void init()
		{
			recommender = new MatrixFactorization ();
			Console.WriteLine (DateTime.Now + " Started Reading Ratings");
			/*
			using (TextReader moviereader = new StreamReader("movies.dat")) {
				string line = moviereader.ReadLine ();
				user_mapping.ToInternalID (line);
			}
			*/

			var training_data = RatingData.Read("new5Mratings.tsv", user_mapping, item_mapping);
			//var training_data = RatingData.Read("tiny5Mratings.tsv", user_mapping, item_mapping);
			recommender.Ratings = training_data;
			Console.WriteLine(DateTime.Now + " Finished Reading Ratings");
			Console.WriteLine(DateTime.Now + " Started Training");

//	EITHER TRAIN
//			recommender.Train(); 
//			recommender.SaveModel ("model.bin");
// 	OR LOAD
			recommender.LoadModel ("model.bin");
			Console.WriteLine(DateTime.Now + " Finished Training");
			//Console.WriteLine (DateTime.Now + " Starting calculating distances");
			//List<Tuple<int, IList<float>>> allvectors = recommender.Ratings.AllItems.Select (x => Tuple.Create (x, recommender.GetItemVector(x))).ToList();
			//EuclideanMatrix matrix = EuclideanMatrix.CalculateEuclideanMatrix (allvectors);
			//StreamWriter handle = new StreamWriter("matrix.text");
			//matrix.Write (handle);
			//handle.Close ();

			StreamReader handle = new StreamReader ("matrix.text");
			EuclideanMatrix matrix = EuclideanMatrix.ReadCorrelationMatrix (handle);
			handle.Close ();
			diversifier = new LatentFeatureDiversfication(matrix);

			Console.WriteLine (DateTime.Now + " Finished calculating distances");
			_timer = new Timer(1000); // Set up the timer for 3 seconds
			_timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
			_timer.Enabled = true; // Enable it

		}
		
		public StatusResponse stats()
		{
			var items = recommender.Ratings.AllItems.Count;
			var users = recommender.Ratings.AllUsers.Count;
			var ratings = recommender.Ratings.Count;
			//var allitems = recommender.Ratings.AllItems;
			//var allusers = recommender.Ratings.AllUsers;
			/*
			TextWriter writer = new StreamWriter ("storedratings.log");
			foreach (var item in allitems) {
				foreach (var user in allusers) {
					float value;
					if(recommender.Ratings.TryGet (user, item, out value))
					{
						writer.WriteLine (user + " " + item + " " + value);
					}
				}
			}
			writer.Close ();
			item_mapping.SaveMapping ("itemmapping");
			user_mapping.SaveMapping ("usermapping");
			*/
			return new StatusResponse { Result = "Items: " + items + " Users: " + users + " Ratings: " + ratings};
		}
		public List<Prediction> predict(int userid, int length)
		{
			//recommender.RetrainUser (userid);
			//recommender.Train ();
			var allpredictions = recommender.ScoreItems(userid, recommender.Ratings.AllItems).OrderByDescending(prediction => prediction.Second);
			//Recommendation recommendation = new Recommendation{ID = 2, prediction = 3.5, vector = new double[] {0.4, 0.5}};
			List<Prediction> returnpredictions = new List<Prediction>();
			returnpredictions.Add(new Prediction("-1", -1, recommender.GetUserVector(userid)));

			foreach(MyMediaLite.DataType.Pair<int,float> prediction in allpredictions)
			{
				Prediction newprediction = new Prediction();
				newprediction.itemid = item_mapping.ToOriginalID(prediction.First);
				newprediction.value = prediction.Second;
				newprediction.vector = recommender.GetItemVector(prediction.First);
				returnpredictions.Add(newprediction);
			}
			return returnpredictions.Take (length).ToList ();
		}

		public IList<int> training(int userid)
		{
			System.Random rand = new System.Random();

			if (recommender.Ratings.Users.Contains (userid)) {
				var returnratings = 
						from r in recommender.Ratings.AllItems
						where !(from i in recommender.Ratings.GetItems (recommender.Ratings.ByUser [userid]) select i).Contains (r)
						orderby rand.Next()
						select r;
				return returnratings.Take (10).ToList ();
			} else {
				return new List<int> ();
			}
		}
		/*
		public List<Recommendation> predict(int userid, Diversifier diversifier, float level)
		{
			//recommender.RetrainUser (userid);
			//recommender.Train ();
			var allpredictions = recommender.ScoreItems(userid, recommender.Ratings.AllItems);

			//Recommendation recommendation = new Recommendation{ID = 2, prediction = 3.5, vector = new double[] {0.4, 0.5}};
			List<Recommendation> returnrecommendations = new List<Recommendation>();
			returnrecommendations.Add(new Recommendation(-1, -1, recommender.GetUserVector(userid)));

			foreach(MyMediaLite.DataType.Pair<int,float> prediction in allpredictions)
			{
				Recommendation newrecommendation = new Recommendation();
				newrecommendation.ID = prediction.First;
				newrecommendation.prediction = prediction.Second;
				newrecommendation.vector = recommender.GetItemVector(prediction.First);
				returnrecommendations.Add(newrecommendation);
			}
			return returnrecommendations;
		}
		*/
		public StatusResponse AddRating(int userid, int itemid, float value)
		{
			//System.Random rnd = new System.Random();
			//userid = rnd.Next(1,200000);
			//itemid = rnd.Next(1,200000);
			//value = rnd.Next (0,4);
			r.AddAsync (userid, itemid, value, _locker);
			return new StatusResponse { Result = "OK"};

		}
	}

	public class RecommenderService : RestServiceBase<Status>
	{
		public Recommender recommender { get; set; }

		public override object OnGet(Status request)
		{
			return recommender.stats ();
		}
	}
	public class PredictionService : RestServiceBase<RecommendationList>
	{
		public Recommender recommender { get; set; }

		public override object OnGet(RecommendationList request)
		{
			int userid = Convert.ToInt32(recommender.user_mapping.ToInternalID(request.userid));
			float level; 
			int length;
			if (request.length == "") {
				length = 20;
			} else{
				length = Int16.Parse (request.length);
			}
			if(request.level == ""){
				return recommender.predict (userid, length);
			} else {
				if(request.level == "training"){
					var trainingitems = recommender.training(userid);
					return trainingitems;
				}
				else{
					level = float.Parse(request.level);
					List<Prediction> recommendations = recommender.predict (userid,200);

					List<int> recommendation_ids = new List<int>();
					foreach(Prediction prediction in recommendations){
						recommendation_ids.Add (recommender.item_mapping.ToInternalID(prediction.itemid));
					}
					Console.WriteLine ("Diversifying " + recommendation_ids.Count + " Items @ " + level);
					for (int i = 0; i < 30; i++)
						Console.Write (recommendation_ids[i]);
					List<int> recommended_ids = recommender.diversifier.DiversifySequential(recommendation_ids, double.Parse(request.level), length);
					List<string> movie_ids = new List<string> ();
					foreach (int movie_id in recommended_ids) {
						movie_ids.Add (recommender.item_mapping.ToOriginalID(movie_id));
					}
					Console.WriteLine ("returning " + movie_ids.Count + " movies");
					return recommendations.Where (x => movie_ids.Contains (x.itemid));
				}
			}
		}
	}

	public class RatingService : RestServiceBase<Rating>
	{	
		public Recommender recommender { get; set; }
		private static object _locker2 = new object();

		public override object OnGet(Rating request)
		{
			/*
			lock (_locker2) {
				recommender.writer.WriteLine (request.userid + " " + request.itemid + " " + request.value);
				recommender.writer.Flush ();
			}
			*/
			int userid = recommender.user_mapping.ToInternalID(request.userid);
			int itemid = recommender.item_mapping.ToInternalID(request.itemid);
			float value = Convert.ToSingle(request.value);
			return recommender.AddRating(userid, itemid, value);
		}
	}
}

