<%@ WebService Language="C#" Class="MyMediaLite.RatingService" %>

using System;
using System.Collections.Generic;
using System.Web.Services;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;

// TODO
//  - database backend
//  - load model on startup
//  - string user and item IDs

namespace MyMediaLite
{
	[WebService (Namespace = "http://ismll.de/RatingService")]
	public class RatingService
	{
		RatingPredictor recommender = new BiasedMatrixFactorization();
		EntityMapping user_mapping  = new EntityMapping();
		EntityMapping item_mapping  = new EntityMapping();
	
		public RatingService()
		{
			recommender.Ratings = new Ratings();
			recommender.Train();
		}

		[WebMethod]
		public void AddBulkFeedback(int user_id, List<int> item_ids, List<double> scores)
		{
		}
		
		[WebMethod]
		public void AddFeedback(int user_id, int item_id, double score)
		{
			// TODO check whether score is in valid range
			recommender.AddRating(user_mapping.ToInternalID(user_id), item_mapping.ToInternalID(item_id), score);
		}
		
		[WebMethod]
		public double Predict(int user_id, int item_id)
		{
			return recommender.Predict(user_mapping.ToInternalID(user_id), item_mapping.ToInternalID(item_id));
		}
		
		[WebMethod]
		public void Train()
		{
			recommender.Train();
		}		
	}
}