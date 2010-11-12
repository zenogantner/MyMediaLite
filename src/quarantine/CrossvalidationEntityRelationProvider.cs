using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MyMediaProject.RecommenderSystem.Framework.EntityRelationAlgorithm;

namespace MyMediaLite.util
{
	/// <description>Not thread safe</description>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class CrossvalidationSplit
    {
		protected WP2Backend data; // TODO implement for IEntityRelationProvider
        protected DataSetMemoryBased[] splitted_ratings;
		protected List<int>[] relevant_items;

        public CrossvalidationSplit(WP2Backend data, int k) : this(data, k, true) { }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="data">
		/// A <see cref="WP2Backend"/>
		/// </param>
		/// <param name="k">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="split_by_rating">
		/// split by rating (line, event) or by item
		/// </param>
        public CrossvalidationSplit(WP2Backend data, int k, bool split_by_rating) // TODO think about nicer, self-explanatory interface
        {
			this.data = data;
			this.splitted_ratings = new DataSetMemoryBased[k];
			this.relevant_items = new List<int>[k];

			DataSetMemoryBased ratings = (DataSetMemoryBased) data.GetRelation(RelationType.Rated); // TODO make configurable

			// TODO support more splitting schemes
			if (split_by_rating)
				SplitRatings(ratings, k);
			else
				SplitItems(data, k);
        }

        void SplitRatings (DataSetMemoryBased ratings, int k)
		{
			int num_rows_split  = ratings.RowCount / k;
			for (int i = 0; i < k; i++)
            	this.splitted_ratings[i] = new DataSetMemoryBased(
                	new DataSetMemoryBased.DataType[] {
        				DataSetMemoryBased.DataType.dt_int,
        				DataSetMemoryBased.DataType.dt_int,
        				DataSetMemoryBased.DataType.dt_double
        			},
			 		num_rows_split + (i < ratings.RowCount % k ? 1 : 0)
				);			
			
			for (int i = 0; i < ratings.RowCount; i++)
			{
				ratings.Read();
				int user_id = ratings.GetInt32(0);
				int item_id = ratings.GetInt32(1);
				double val  = ratings.GetDouble(2);

				DataSetMemoryBased target = this.splitted_ratings[i % k];

				int index = i / k;
				((DataSetMemoryBased.DataArrayInteger) target.data.columns[0]).data[ index ] = user_id;
				((DataSetMemoryBased.DataArrayInteger) target.data.columns[1]).data[ index ] = item_id;
				((DataSetMemoryBased.DataArrayDouble)  target.data.columns[2]).data[ index ] = val;
			}
			
			List<int> items = new List<int>(GetItems(data));
			for (int i = 0; i < k; i++)
				relevant_items[i] = items;
		}

		static int[] GetItems(WP2Backend data) // TODO this is used so often, move it to a utility library
		{
			IEntityRelationDataReader items_reader = data.GetEntity(EntityType.CatalogItem);
            List<int> items = new List<int>();
            items_reader.Open();
            while (items_reader.Read())
                items.Add(items_reader.GetInt32(0));
			return items.ToArray();			
		}
		
		// TODO let it know the relevant items ...
        void SplitItems(WP2Backend data, int k)
		{
			int[] items = GetItems(data);
			// Fisher-Yates shuffle                             // TODO: move to a library
			Random random = util.Random.GetInstance();			
			for (int i = items.Length - 1; i >= 0; i--)
			{
				int r = random.Next(0, i + 1);

				int tmp = items[i];
				items[i] = items[r];
				items[r] = tmp;
			}

			for (int i = 0; i < k; i++)
				relevant_items[i] = new List<int>();
			
			Dictionary<int, int> item_to_split = new Dictionary<int, int>();
			for (int i = 0; i < items.Length; i++)
			{
				item_to_split[items[i]] = i % k;
				relevant_items[i % k].Add(items[i]);
			}


			DataSetMemoryBased ratings = (DataSetMemoryBased) data.GetRelation(RelationType.Rated);
			
			// determine the size of the splits
			int[] split_sizes = new int[k];
			for (int i = 0; i < ratings.RowCount; i++)
			{
				ratings.Read();
				int item_id = ratings.GetInt32(1);
				
				int split_id = item_to_split[item_id];
				split_sizes[split_id]++;
			}
			ratings.Open(); // re-open for second iteration
			
			for (int i = 0; i < k; i++)
            	this.splitted_ratings[i] = new DataSetMemoryBased(
                	new DataSetMemoryBased.DataType[] {
        				DataSetMemoryBased.DataType.dt_int,
        				DataSetMemoryBased.DataType.dt_int,
        				DataSetMemoryBased.DataType.dt_double
        			},
			 		split_sizes[i]
				);			
			
			int[] split_counters = new int[k];
			for (int i = 0; i < ratings.RowCount; i++)
			{
				ratings.Read();
				int user_id = ratings.GetInt32(0);
				int item_id = ratings.GetInt32(1);
				double val  = ratings.GetDouble(2);

				int split_id = item_to_split[item_id];
				DataSetMemoryBased target = this.splitted_ratings[split_id];

				int index = split_counters[split_id]++;
				((DataSetMemoryBased.DataArrayInteger) target.data.columns[0]).data[ index ] = user_id;
				((DataSetMemoryBased.DataArrayInteger) target.data.columns[1]).data[ index ] = item_id;
				((DataSetMemoryBased.DataArrayDouble)  target.data.columns[2]).data[ index ] = val;
			}
		}

		public IEntityRelationDataProvider GetTrainingSet(int i)
		{
			return new CrossvalidationEntityRelationProvider(splitted_ratings, i, this.data);
		}

		public IEntityRelationDataReader GetTestSet(int i)
		{
			return splitted_ratings[i];
		}
		
		public int[] GetRelevantItems(int i)
		{
			//TODO optimize and store as array instead of list
			return relevant_items[i].ToArray();
		}
    }

	public class CrossvalidationEntityRelationProvider : IEntityRelationDataProvider
	{
		protected IEntityRelationDataProvider data;
		protected CombinedEntityRelationDataReader training_data;

		public CrossvalidationEntityRelationProvider(DataSetMemoryBased[] splitted_ratings, int i, WP2Backend data)
		{
			this.data = data;
			this.training_data = new CombinedEntityRelationDataReader();
			for (int j = 0; j < splitted_ratings.Length; j++)
				if (j != i)
					training_data.Add(splitted_ratings[j]);
		}

        public IEntityRelationDataReader GetRelation(RelationType relation_id)
        {
            return GetRelation(relation_id, new int[] { });
        }

        public IEntityRelationDataReader GetRelation(RelationType relation_id, int[] attributes)
        {
            if (relation_id == RelationType.Rated || relation_id == RelationType.Viewed)
            {
				training_data.Open();
                return training_data;
            }
            else
            {
                throw new ArgumentException(String.Format("Unknown relation ID ", relation_id));
            }
        }

        public void RegisterForRelationUpdates(RelationType relation_id, IEntityRelationEngine engine)
        {
			// do nothing
        }

        public void RegisterForEntityUpdates(EntityType entity_id, IEntityRelationEngine engine)
        {
			// do nothing
        }

        public void UnregisterForRelationUpdates(RelationType relation_id, IEntityRelationEngine engine)
        {
			// do nothing
        }

        public void UnregisterForEntityUpdates(EntityType entity_id, IEntityRelationEngine engine)
        {
			// do nothing
        }

        public IEntityRelationDataReader GetEntity(EntityType entityType)
        {
            return data.GetEntity(entityType, new int[] {});
        }

        public IEntityRelationDataReader GetEntity(EntityType entityType, int[] attributes)
        {
			return data.GetEntity(entityType, attributes);
        }

	}
}

