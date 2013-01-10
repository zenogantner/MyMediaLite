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
	/// This recommender supports incremental updates.
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
			ResizeNearestNeighbors(item_id + 1);
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
				double normalization = 0;
				if(nearest_neighbors[item_id] != null)
				{
					foreach (int neighbor in nearest_neighbors[item_id])
					{
						normalization += Math.Pow(correlation[item_id, neighbor], Q);
						if (Feedback.ItemMatrix[neighbor, user_id])
							sum += Math.Pow(correlation[item_id, neighbor], Q);
					}
				}
				if(sum == 0) return 0;
				return (float) (sum / normalization);
			}
			else
			{
				// roughly 10x faster
				// TODO: implement normalization
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
		/// Add positive feedback events and perform incremental training
		/// </summary>
		/// <param name='feedback'>
		/// collection of user id - item id tuples
		/// </param>
		public override void AddFeedback(ICollection<Tuple<int, int>> feedback)
		{
			base.AddFeedback (feedback);
			Dictionary<int,List<int>> feeddict = new Dictionary<int, List<int>>();
			
			// Construct a dictionary to group feedback by user
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			
			// For each user in new feedback update coocurrence 
			// and correlation matrices
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				List<int> rated_items = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> new_items = f.Value;
				foreach (int i in rated_items)
				{
					foreach (int j in new_items)
						cooccurrence[i, j]++;
					
					switch(Correlation) 
					{
					case BinaryCorrelationType.Cooccurrence:
						correlation = cooccurrence;
						break;
					case BinaryCorrelationType.Cosine:
						// Update correlations of each rated item by user 
						foreach (int j in Feedback.AllItems)
						{
							if (i == j)
								correlation[i, i] = 1;
							else
								correlation[i, j] = cooccurrence[i, j] / 
									(float) Math.Sqrt(cooccurrence[i, i] * cooccurrence[j, j]);
						}
						break;
					default:
						throw new NotImplementedException("Incremental updates with ItemKNN only work with cosine and coocurrence (so far)");
					}
				}
				// Recalculate neighbors as necessary
				RetrainItems(new_items);
			}
		}
		
		/// <summary>
		/// Selectively retrains items based on new items added to feedback.
		/// </summary>
		/// <param name='new_items'>
		/// New items.
		/// </param>
		protected void RetrainItems(IEnumerable<int> new_items)
		{
			float min;
			HashSet<int> retrain_items = new HashSet<int>(); 
			foreach (int item in Feedback.AllItems.Except(new_items))
			{
				// Get the correlation of the least correlated neighbor
				if(nearest_neighbors[item] == null) 
					min = 0;
				else if(nearest_neighbors[item].Count < k)
					min = 0;
				else 
					min = correlation[item, nearest_neighbors[item].Last()];
				
				// Check if any of the added items have a higher correlation
				// (requires retraining if it is a new neighbor or an existing one)
				foreach(int new_item in new_items)
					if(correlation[item, new_item] > min)
						retrain_items.Add(item);
			}
			// Recently added items also need retraining
			retrain_items.UnionWith(new_items);
			// Recalculate neighborhood of selected items
			foreach(int r_item in retrain_items)
				nearest_neighbors[r_item] = correlation.GetNearestNeighbors(r_item, k);
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
			Dictionary<int,List<int>> feeddict = new Dictionary<int, List<int>>();
			
			// Construct a dictionary to group feedback by user
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			
			// For each item in removed feedback update coocurrence 
			// and correlation matrices
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				List<int> rated_items = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> removed_items = f.Value;
				foreach (int i in rated_items)
				{
					foreach (int j in removed_items)
						cooccurrence[i, j] = (cooccurrence[i, j] >= 1 ? cooccurrence[i, j] - 1 : 0);
					
					switch(Correlation) 
					{
					case BinaryCorrelationType.Cooccurrence:
						correlation = cooccurrence;
						break;
					case BinaryCorrelationType.Cosine:
						// Update correlations of each rated item by user 
						foreach (int j in Feedback.AllItems)
						{
							if (i == j)
								correlation[i, i] = 1;
							else
								correlation[i, j] = cooccurrence[i, j] / 
									(float) Math.Sqrt(cooccurrence[i, i] * cooccurrence[j, j]);
						}
						break;
					default:
						throw new NotImplementedException("Incremental updates with ItemKNN only work with cosine and coocurrence (so far)");
					}
				}
				// Recalculate neighbors as necessary
				RetrainItemsRemoved(removed_items);
			}
		}

		/// <summary>
		/// Selectively retrains items based on new items removed from feedback.
		/// </summary>
		/// <param name='removed_items'>
		/// Removed items.
		/// </param>
		protected void RetrainItemsRemoved(IEnumerable<int> removed_items)
		{
			float min;
			HashSet<int> retrain_items = new HashSet<int>(); 
			foreach (int item in Feedback.AllItems.Except(removed_items))
				foreach(int r_item in removed_items)
					if(nearest_neighbors[item] != null)
						if(nearest_neighbors[item].Contains(r_item))
							retrain_items.Add(item);
			retrain_items.UnionWith(removed_items);
			foreach(int r_item in retrain_items)
				nearest_neighbors[r_item] = correlation.GetNearestNeighbors(r_item, k);
			Console.WriteLine("Updated "+ retrain_items.Count + " KNN lists");
		}

	}
}
