using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MyMediaLite.data_type;
using MyMediaLite.util;


namespace MyMediaLite
{
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class MemoryRecommender : RecommenderEngine
    {
        public double MaxRatingValue { get; set; }
	    public double MinRatingValue { get; set; }
		
		protected IEntityRelationDataReader prediction_reader;
		
		protected double default_score;
		protected Dictionary<int, List<WeightedItem>> ratings;
		
		public MemoryRecommender(IEntityRelationDataReader prediction_reader, double default_score)
		{
			// TODO think about how to use this stuff, whether it is necessary, etc.
			MinRatingValue = 0;
			MaxRatingValue = 5;
			
			this.prediction_reader = prediction_reader;
			this.default_score     = default_score;
			if (default_score < MinRatingValue)
				MinRatingValue = default_score;
		}

        /// <inheritdoc />
        public void Train()
        {
			Console.Error.Write("Reading in data for MemoryRecommender ...");
			ReadData();
			Console.Error.WriteLine(" done.");
        }

        /// <inheritdoc />
        public double Predict(int user_id, int item_id)
        {
			if (ratings.ContainsKey(user_id))
				foreach (WeightedItem w in ratings[user_id])
					if (w.item_id == item_id)
						return w.weight;
				
			return default_score;
		}

		public List<WeightedItem> GetWeightedItems(int user_id)
		{
			return ratings[user_id];
		}
		
		protected virtual void ReadData()
		{
            // create data structure
			ratings = new Dictionary<int, List<WeightedItem>>();

			prediction_reader.Open();
            while (prediction_reader.Read())
            {
                int user_id = (int) prediction_reader.GetInt32(0);
				WeightedItem weighted_item = new WeightedItem( (int) prediction_reader.GetInt32(1), prediction_reader.GetDouble(2));				
				
				if (!ratings.ContainsKey(user_id))
					ratings[user_id] = new List<WeightedItem>();
				ratings[user_id].Add(weighted_item);
            }
			prediction_reader.Close();
		}		
		
		/// <inheritdoc />
		public void SaveModel(string filePath)
		{
			throw new NotImplementedException(); // TODO
		}

		/// <inheritdoc />
		public void LoadModel(string filePath)
		{
			throw new NotImplementedException(); // TODO
		}

        /// <inheritdoc />
        public void AddRelation(RelationType relation_id, int[] key, object[] values)
        {
			// TODO
        }

        /// <inheritdoc />
        public void UpdateRelation(RelationType relation_id, int[] key, object[] values)
        {
			// TODO
        }

        /// <inheritdoc />
        public void RemoveRelation(RelationType relation_id, int[] key)
        {
			// TODO
        }

        /// <inheritdoc />
        public void AddEntity(EntityType entity_type, int entity_id, object[] values)
        {
			// TODO
        }

        /// <inheritdoc />
        public void UpdateEntity(EntityType entity_type, int entity_id, object[] values)
        {
			// TODO
        }

        /// <inheritdoc />
        public void RemoveEntity(EntityType entity_type, int entity_id)
        {
			// TODO
        }

        /// <inheritdoc />
        public void SubmitRelationFeedback(RelationType relationId, int[] key, double predictedValue, double actualValue)
        {
			// TODO
        }

        /// <inheritdoc />
        public string Description
        {
            get { return "MemoryRecommender"; }
        }

        private MyMediaProject.RecommenderSystem.Core.EngineId engineId;

        /// <inheritdoc />
        public MyMediaProject.RecommenderSystem.Core.EngineId Id
        {
            get
            {
                if (engineId == null)
                {
                    engineId = new MyMediaProject.RecommenderSystem.Core.EngineId(new Guid());
                }
                return engineId;
            }
        }

        /// <inheritdoc />
        public EventHandler<EventArgs> TrainingCompleted
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
