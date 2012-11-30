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

		/// <summary>
		/// For collecting update time statistics.
		/// </summary>
		protected List<TimeSpan> update_times;
		protected List<TimeSpan> nb_update_times;
		protected List<TimeSpan> mx_update_times;
		protected List<int> updated_neighbors_count;

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
			update_times = new List<TimeSpan>();
			nb_update_times = new List<TimeSpan>();
			mx_update_times = new List<TimeSpan>();
			updated_neighbors_count = new List<int>();
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
		
		/// <summary>
		/// Resizes the nearest neighbors list if necessary.
		/// </summary>
		/// <param name='new_size'>
		/// New_size.
		/// </param>
		protected void ResizeNearestNeighbors(int new_size)
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
				//Console.WriteLine("Adding feedback: " + tpl.Item1 + " " + tpl.Item2);
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			DateTime start;
			DateTime mx_upd_time;
			DateTime nb_upd_time;
			int num_updated_neighbors;
			// For each user in new feedback update coocurrence 
			// and correlation matrices
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				start = DateTime.Now;
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
				mx_upd_time = DateTime.Now; 

				// Recalculate neighbors as necessary
				num_updated_neighbors = RetrainItems(new_items);
				nb_upd_time = DateTime.Now;

				// Collect statistics
				update_times.Add(nb_upd_time - start);
				mx_update_times.Add(mx_upd_time - start);
				nb_update_times.Add(nb_upd_time - mx_upd_time);
				updated_neighbors_count.Add(num_updated_neighbors);
			}
		}

		/// <summary>
		/// Selectively retrains items based on new items added to feedback.
		/// </summary>
		/// <returns>
		/// Number of updated neighbor lists.
		/// </returns>
		/// <param name='new_items'>
		/// Recently added items.
		/// </param>
		protected int RetrainItems(IEnumerable<int> new_items)
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

			return retrain_items.Count;
		}

		/// <summary>
		/// Remove all feedback events by the given user-item combinations
		/// </summary>
		/// <param name='feedback'>
		/// collection of user id - item id tuples
		/// </param>
		public override void RemoveFeedback(ICollection<Tuple<int, int>> feedback)
		{
			DateTime start = DateTime.Now;
			base.RemoveFeedback (feedback);
			Dictionary<int,List<int>> feeddict = new Dictionary<int, List<int>>();
			
			// Construct a dictionary to group feedback by user
			foreach (var tpl in feedback)
			{
				Console.WriteLine("Removing feedback: " + tpl.Item1 + " " + tpl.Item2);
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			
			// For each user in new feedback update coocurrence 
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
			TimeSpan update_time = DateTime.Now - start;
			update_times.Add(update_time);
			Console.WriteLine("Update Time: " + update_time.Milliseconds);
		}

		/// <summary>
		/// Selectively retrains items based on new items removed from feedback.
		/// </summary>
		/// <param name='removed_items'>
		/// Removed items.
		/// </param>
		protected void RetrainItemsRemoved(IEnumerable<int> removed_items)
		{
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
		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MyMediaLite.ItemRecommendation.ItemKNN"/> is reclaimed by garbage collection.
		/// </summary>
		~ItemKNN()
		{
			double sum_total = 0;
			double max_total = 0;
			double sum_nb = 0;
			double max_nb = 0;
			double max_mx = 0;
			double sum_mx = 0;
			int max_ct = 0;
			int sum_ct = 0;
			int reg_ct;

			Console.WriteLine("Registry count:" + (reg_ct = update_times.Count));
			Console.WriteLine();

			for(int i = 0; i < reg_ct; i++)
			{
				TimeSpan tt_upd_time = update_times[i];
				TimeSpan mx_upd_time = mx_update_times[i];
				TimeSpan nb_upd_time = nb_update_times[i];
				int nb_count = updated_neighbors_count[i];

				max_total = Math.Max(max_total, tt_upd_time.TotalMilliseconds);
				sum_total += tt_upd_time.Milliseconds;

				max_mx = Math.Max(max_mx, mx_upd_time.TotalMilliseconds);
				sum_mx += mx_upd_time.Milliseconds;

				max_nb = Math.Max(max_nb, nb_upd_time.TotalMilliseconds);
				sum_nb += nb_upd_time.Milliseconds;

				max_ct = Math.Max(max_ct, nb_count);
				sum_ct += nb_count;

				Console.WriteLine(tt_upd_time.TotalMilliseconds + "\t" 
				                  + mx_upd_time.TotalMilliseconds + "\t"
				                  + nb_upd_time.TotalMilliseconds + "\t"
				                  + nb_count);
			}
			Console.WriteLine();

			Console.WriteLine("Avg update time: " + sum_total/reg_ct);
			Console.WriteLine("Max update time: " + max_total);
			Console.WriteLine("Avg mx update time: " + sum_mx/reg_ct);
			Console.WriteLine("Max mx update time: " + max_mx);
			Console.WriteLine("Avg nb update time: " + sum_nb/reg_ct);
			Console.WriteLine("Max nb update time: " + max_nb);
			Console.WriteLine("Avg nb count: " + sum_ct/reg_ct);
			Console.WriteLine("Max nb count: " + max_ct);
		}
	}
}