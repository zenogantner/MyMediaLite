<%@ WebService Language="C#" Class="MyMediaLite.RatingService" %>

using System;
using System.Collections.Generic;
using System.Web.Services;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Util;

// TODO
//  - RESTful service instead of SOAP
//  - database backend
//  - load model on startup
//  - string user and item IDs

namespace MyMediaLite
{
	[WebService (Namespace = "http://ismll.de/RatingService")]
	public class RatingService
	{
		static RatingPredictor recommender;
		static EntityMapping user_mapping;
		static EntityMapping item_mapping;
	
		static int access_counter;
	
		public RatingService()
		{
			if (recommender == null)
			{
				recommender  = new BiasedMatrixFactorization();
				lock(recommender)
				{
					Console.Error.Write("Setting up recommender ... ");	
					access_counter = 0;
				
					user_mapping = new EntityMapping();
					item_mapping = new EntityMapping();
					
					recommender.Ratings = new Ratings();
					recommender.MinRating = 1;
					recommender.MaxRating = 5; // expose this to API/configuration
	
					Recommender.Configure(recommender, "bias_reg=0.007 reg_u=0.1 reg_i=0.12 learn_rate=0.05 num_iter=100 bold_driver=true");
	
					//recommender.Train();
					Console.Error.WriteLine("done.");
				}
			}
		}

		[WebMethod]
		public void AddBulkFeedback(int user_id, List<int> item_ids, List<double> scores)
		{
			for (int i = 0; i < item_ids.Count; i++)
				lock (recommender)
				{
					// TODO improve IRatingPredictor API
					recommender.AddRating(user_mapping.ToInternalID(user_id), item_mapping.ToInternalID(item_ids[i]), scores[i]);
				}
		}

		[WebMethod]
		public void AddFeedbackNoTraining(int user_id, int item_id, double score)
		{
			lock (recommender)
			{
				if (access_counter % 100 == 99)
					Console.Error.Write(".");
				if (access_counter % 8000 == 7999)						
					Console.Error.WriteLine();
				access_counter++;						
			
				// TODO check whether score is in valid range
				recommender.Ratings.Add(user_mapping.ToInternalID(user_id), item_mapping.ToInternalID(item_id), score);
			}
		}		
						
		[WebMethod]
		public void AddFeedback(int user_id, int item_id, double score)
		{
			// TODO check whether score is in valid range
			lock (recommender)
				recommender.AddRating(user_mapping.ToInternalID(user_id), item_mapping.ToInternalID(item_id), score);
		}
		
		[WebMethod]
		public double Predict(int user_id, int item_id)
		{
			double pred = recommender.Predict(user_mapping.ToInternalID(user_id), item_mapping.ToInternalID(item_id));
			//Console.Error.WriteLine("p({0}, {1}, {2})", user_id, item_id, pred);
			
			return pred;
		}
		
		[WebMethod]
		public void Train()
		{
			// TODO proper re-training; call it re-train
			// TODO perform asynchronously; copy all data
			// TODO allow migration to different machine/batch job?
			lock (recommender)
			{
				Utils.DisplayDataStats(recommender.Ratings, null, recommender);
				Console.Error.WriteLine(recommender.ToString());
				recommender.Train();
			}
		}		
	}
}
