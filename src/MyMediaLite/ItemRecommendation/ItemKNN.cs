// Copyright (C) 2011, 2012 Zeno Gantner
// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
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
using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>k-nearest neighbor (kNN) item-based collaborative filtering</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class ItemKNN : KNN, IItemSimilarityProvider
	{
		///
		protected override IBooleanMatrix DataMatrix { get { return Feedback.ItemMatrix; } }

		///
		public override void Train()
		{
			base.Train();

			int num_items = MaxItemID + 1;
			if (k != uint.MaxValue)
			{
				this.nearest_neighbors = new List<IList<int>>(num_items);
				for (int i = 0; i < num_items; i++)
					nearest_neighbors.Add(correlation.GetNearestNeighbors(i, k));
			}
		}

		/// <summary>
		/// Adds the item.
		/// </summary>
		/// <param name='item_id'>
		/// Item_id.
		/// </param>
		protected override void AddItem(int item_id)
		{
			base.AddItem(item_id);
			resizeNearestNeighbors(item_id + 1);
		}
		
		/// <summary>
		/// Resizes the nearest neighbors.
		/// </summary>
		/// <param name='new_size'>
		/// New_size.
		/// </param>
		protected void resizeNearestNeighbors(int new_size)
		{
			if(new_size > nearest_neighbors.Count)
				for(int i = nearest_neighbors.Count; i < new_size; i++)
					nearest_neighbors.Add(null);
		}
		
		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				return float.MinValue;
			if (item_id > MaxItemID)
				return float.MinValue;

			if (k != uint.MaxValue)
			{
				double sum = 0;
				foreach (int neighbor in nearest_neighbors[item_id])
					if (Feedback.ItemMatrix[neighbor, user_id])
						sum += Math.Pow(correlation[item_id, neighbor], Q);
				return (float) sum;
			}
			else
			{
				// roughly 10x faster
				return (float) correlation.SumUp(item_id, Feedback.UserMatrix[user_id], Q);
			}
		}

		///
		public float GetItemSimilarity(int item_id1, int item_id2)
		{
			return correlation[item_id1, item_id2];
		}

		///
		public IList<int> GetMostSimilarItems(int item_id, uint n = 10)
		{
			if (n <= k)
				return nearest_neighbors[item_id].Take((int) n).ToArray();
			else
				return correlation.GetNearestNeighbors(item_id, n);
		}

		/// <summary>
		/// Remove all feedback events by the given user-item combinations
		/// </summary>
		/// <param name='feedback'>
		/// collection of user id - item id tuples
		/// </param>
		public override void RemoveFeedback(ICollection<Tuple<int, int>> feedback)
		{
			base.RemoveFeedback (feedback);
			var items = from t in feedback select t.Item1;
			retrainItems(new HashSet<int>(items));
		}
 
		/// <summary>
		/// Add positive feedback events and perform incremental training
		/// </summary>
		/// <param name='feedback'>
		/// collection of user id - item id tuples
		/// </param>
		public override void AddFeedback(ICollection<Tuple<int, int>> feedback)
		{
			base.AddFeedback (feedback);
			foreach(int item in Feedback.AllItems)
				Console.Write (item + " ");
			Dictionary<int,List<int>> feeddict = new Dictionary<int, List<int>>();
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				Console.WriteLine("User: "+f.Key+":");
				List<int> rated_items = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> new_items = f.Value;
				foreach (int i in rated_items)
				{
					Console.WriteLine("Rated item: "+i); 
					foreach (int j in new_items)
					{
						Console.Write(j+" ");
						cooccurrence[i, j]++;
						switch(Correlation) 
						{
							case BinaryCorrelationType.Cooccurrence:
								correlation = cooccurrence;
								break;
							case BinaryCorrelationType.Cosine:
								if (i == j)
									correlation[i, i] = 1;
								else
									correlation[i,j] = cooccurrence[i, j] / 
										(float) Math.Sqrt(cooccurrence[i, i] * cooccurrence[j, j]);
								break;
							default:
							throw new NotImplementedException("Incremental updates with ItemKNN only work with cosine and coocurrence (so far)");
						}
					}
					Console.WriteLine();
				}
				retrainItems(new_items);
			}
		}

		/// <summary>
		/// Retrains the items.
		/// </summary>
		/// <param name='item_ids'>
		/// Item_ids.
		/// </param>
		protected void retrainItems(IEnumerable<int> item_ids)
		{
			foreach (int item_id in item_ids)
			{
				nearest_neighbors[item_id] = correlation.GetNearestNeighbors(item_id, k);
				// Find items that have item_id in neighbourhood
				List<int> retrain_also = getItemsWithNeighbor(item_id);
				foreach (int item in retrain_also)
					nearest_neighbors[item] = correlation.GetNearestNeighbors(item, k);
			}
		}
		
		/// <summary>
		/// Gets the items with neighbor.
		/// </summary>
		/// <returns>
		/// The items with neighbor.
		/// </returns>
		/// <param name='neighbor_id'>
		/// Neighbor_id.
		/// </param>
		protected List<int> getItemsWithNeighbor(int neighbor_id)
		{
			List<int> item_list = new List<int>();
			for(int i = 0; i < nearest_neighbors.Count; i++)
				if(nearest_neighbors[i].Contains(neighbor_id))
					item_list.Add(i);
			return item_list;
		}
	}
}