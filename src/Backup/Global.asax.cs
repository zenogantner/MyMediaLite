using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.Common.Web;
using MyMediaLite;
using MyMediaLite.Data;
using MyMediaLite.RatingPrediction;
using MyMediaLite.IO;
using System.Collections.Generic;

namespace JSONAPI
{
	[DataContract]
	[Description("MyMediaLite Web API.")]
	[RestService("/rating/{userid}/{itemid}/{value}")] 
	public class Rating
	{
		[DataMember]
		public float value {get;set;}
		[DataMember]
		public string itemid {get;set;}
		[DataMember]
		public string userid {get;set;}
	}
	/*
	public class Prediction
	{
		public int itemid {get;set;}
		public float value {get;set;}
		public float[] vector {get;set;}
	}
	*/
	public class RecommendationList
	{
		[DataMember]
		public string userid {get;set;}
		[DataMember]
		public string level {get;set;}
		[DataMember]
		public string length {get;set;}
	}

    [RestService("/mu-668f8cbc-c9764138-f6fc077c-bc5b36b0")]
    public class Blitz {}

    public class BlitzService : IService<Blitz>
    {
        public object Execute(Blitz request)
        {
            return new HttpResult("42", "text");              
        }
    }

	public class Prediction
	{
		public string itemid { get; set; }
		public double value {get;set;}
		public IList<float> vector {get;set;}

		public Prediction(string itemid, double value, IList<float> vector){
			this.itemid = itemid;
			this.value = value;
			this.vector = vector;
		}

		public Prediction(){
		}
	}

	public class StatusResponse
	{
		public string Result {get;set;}
	}

	public class Status
	{
		public string Result {get;set;}
	}

	public class MMAPIHost : AppHostBase
	{

		public MMAPIHost() 
			: base("MyMediaLite API", typeof(RecommenderService).Assembly) { }

		public override void Configure(Funq.Container container)
		{
			base.SetConfig(new EndpointHostConfig
			               {
				GlobalResponseHeaders = {
					{ "Access-Control-Allow-Origin", "*" },
					{ "Access-Control-Allow-Methods", "GET" },
					{ "Access-Control-Allow-Headers", "Content-Type" },
				},
			});

			Recommender.Instance.init();
			container.Register(Recommender.Instance);

			Routes
				.Add<Rating>("/rating/{Userid}/{Itemid}/{Value}")
				.Add<Status>("/status")
				.Add<RecommendationList>("/recommendation")
				.Add<RecommendationList>("/recommendation/{userid}")	
				.Add<RecommendationList>("/recommendation/{userid}/{length}/{level}")
				.Add<RecommendationList>("/training/{userid}")
				.Add<RecommendationList>("/training/{userid}/{level}");
		}
	}

	public class Global : System.Web.HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			new MMAPIHost().Init();
		}
	}
}


