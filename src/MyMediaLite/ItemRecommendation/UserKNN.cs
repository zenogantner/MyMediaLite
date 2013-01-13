// Copyright (C) 2013 João Vinagre, Zeno Gantner
// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
	/// <summary>k-nearest neighbor user-based collaborative filtering</summary>
	/// <remarks>
	/// This recommender supports incremental updates for the Cosine and Cooccurrence similarities.
	/// </remarks>
	public class UserKNN : KNN, IUserSimilarityProvider, IFoldInItemRecommender
	{
		///
		protected override IBooleanMatrix DataMatrix { get { return Feedback.UserMatrix; } }

		///
		public override void Train()
		{
			base.Train();

			int num_users = MaxUserID + 1;
			this.nearest_neighbors = new List<IList<int>>(num_users);
			for (int u = 0; u < num_users; u++)
				nearest_neighbors.Add(correlation.GetNearestNeighbors(u, k));
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if ((user_id > MaxUserID) || (item_id > MaxItemID))
				return float.MinValue;

			if (k != uint.MaxValue)
			{
				double sum = 0;
				double normalization = 0;
				if (nearest_neighbors[user_id] != null)
				{
					foreach (int neighbor in nearest_neighbors[user_id])
					{
						normalization += Math.Pow(correlation[user_id, neighbor], Q);
						if (Feedback.UserMatrix[neighbor, item_id])
							sum += Math.Pow(correlation[user_id, neighbor], Q);
					}
				}
				if (sum == 0) return 0;
				return (float) (sum / normalization);
			}
			else
			{
				// roughly 10x faster
				return (float) correlation.SumUp(user_id, Feedback.ItemMatrix[item_id], Q);
			}
		}

		///
		public float GetUserSimilarity(int user_id1, int user_id2)
		{
			return correlation[user_id1, user_id2];
		}

		///
		public IList<int> GetMostSimilarUsers(int user_id, uint n = 10)
		{
			if (n <= k)
				return nearest_neighbors[user_id].Take((int) n).ToArray();
			else
				return correlation.GetNearestNeighbors(user_id, n);
		}

		float Predict(IList<float> user_similarities, IList<int> nearest_neighbors, int item_id)
		{
			if ((item_id < 0) || (item_id > MaxItemID))
				return float.MinValue;

			if (k != uint.MaxValue)
			{
				double sum = 0;
				foreach (int neighbor in nearest_neighbors)
					if (Feedback.UserMatrix[neighbor, item_id])
						sum += Math.Pow(user_similarities[neighbor], Q);
				return (float) sum;
			}
			else
			{
				double sum = 0;
				foreach (int user_id in Feedback.ItemMatrix[item_id])
					sum += Math.Pow(user_similarities[user_id], Q);
				return (float) sum;
			}
		}

		/// <summary>Fold in one user, identified by their items</summary>
		/// <returns>a vector containing the similarities to all users</returns>
		/// <param name='items'>the items representing the user</param>
		protected virtual IList<float> FoldIn(IList<int> items)
		{
			var user_similarities = new float[MaxUserID + 1];

			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				user_similarities[user_id] = correlation.ComputeCorrelation(Feedback.UserMatrix[user_id], new HashSet<int>(items));

			return user_similarities;
		}

		///
		public IList<Tuple<int, float>> ScoreItems(IList<int> accessed_items, IList<int> candidate_items)
		{
			var user_similarities = FoldIn(accessed_items);

			IList<int> nearest_neighbors = null;
			if (k != uint.MaxValue)
			{
				var users = Enumerable.Range(0, MaxUserID - 1).ToList();
				users.Sort(delegate(int i, int j) { return user_similarities[j].CompareTo(user_similarities[i]); });

				if (k < users.Count)
					nearest_neighbors = users.GetRange(0, (int) k).ToArray();
				else
					nearest_neighbors = users.ToArray();
			}

			// score the items
			var result = new Tuple<int, float>[candidate_items.Count];
			for (int i = 0; i < candidate_items.Count; i++)
			{
				int item_id = candidate_items[i];
				result[i] = Tuple.Create(item_id, Predict(user_similarities, nearest_neighbors, item_id));
			}
			return result;
		}

		/// <summary>
		/// Add positive feedback events and perform incremental training
		/// </summary>
		/// <param name='feedback'>
		/// collection of user id - item id tuples
		/// </param>
		public override void AddFeedback(ICollection<Tuple<int, int>> feedback)
		{
			base.AddFeedback(feedback);
			Dictionary<int,List<int>> feeddict = new Dictionary<int, List<int>>();

			// Construct a dictionary to group feedback by item
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item2))
					feeddict.Add(tpl.Item2, new List<int>());
				feeddict[tpl.Item2].Add(tpl.Item1);
			}
			// For each user in new feedback update coocurrence
			// and correlation matrices
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				List<int> rating_users = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> new_users = f.Value;
				foreach (int i in rating_users)
				{
					foreach (int j in new_users)
						cooccurrence[i, j]++;

					switch(Correlation)
					{
					case BinaryCorrelationType.Cooccurrence:
						correlation = cooccurrence;
						break;
					case BinaryCorrelationType.Cosine:
						// Update correlations of each user in feedback
						foreach (int j in Feedback.AllUsers)
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
				RetrainUsers(new_users);
			}
		}

		/// <summary>
		/// Selectively retrains users based on new users added to feedback.
		/// </summary>
		/// <returns>
		/// Number of updated neighbor lists.
		/// </returns>
		/// <param name='new_users'>
		/// Recently added users.
		/// </param>
		protected int RetrainUsers(IEnumerable<int> new_users)
		{
			float min;
			HashSet<int> retrain_users = new HashSet<int>();
			foreach (int user in Feedback.AllUsers.Except(new_users))
			{
				// Get the correlation of the least correlated neighbor
				if (nearest_neighbors[user] == null)
					min = 0;
				else if (nearest_neighbors[user].Count < k)
					min = 0;
				else
					min = correlation[user, nearest_neighbors[user].Last()];

				// Check if any of the added users have a higher correlation
				// (requires retraining if it is a new neighbor or an existing one)
				foreach (int new_user in new_users)
					if (correlation[user, new_user] > min)
						retrain_users.Add(user);
			}
			// Recently added users also need retraining
			retrain_users.UnionWith(new_users);
			// Recalculate neighborhood of selected users
			foreach (int r_user in retrain_users)
				nearest_neighbors[r_user] = correlation.GetNearestNeighbors(r_user, k);

			return retrain_users.Count;
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

			// Construct a dictionary to group feedback by item
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item2))
					feeddict.Add(tpl.Item2, new List<int>());
				feeddict[tpl.Item2].Add(tpl.Item1);
			}

			// For each user in removed feedback update coocurrence
			// and correlation matrices
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				List<int> rating_users = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> removing_users = f.Value;
				foreach (int i in rating_users)
				{
					foreach (int j in removing_users)
						cooccurrence[i, j] = (cooccurrence[i, j] >= 1 ? cooccurrence[i, j] - 1 : 0);

					switch(Correlation)
					{
					case BinaryCorrelationType.Cooccurrence:
						correlation = cooccurrence;
						break;
					case BinaryCorrelationType.Cosine:
						foreach (int j in Feedback.AllUsers)
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
				RetrainUsersRemoved(removing_users);
			}
		}

		/// <summary>
		/// Selectively retrains users based on removed feedback.
		/// </summary>
		/// <param name='removing_users'>
		/// Users with removed feedback.
		/// </param>
		protected void RetrainUsersRemoved(IEnumerable<int> removing_users)
		{
			HashSet<int> retrain_users = new HashSet<int>();
			foreach (int user in Feedback.AllUsers.Except(removing_users))
				foreach (int r_user in removing_users)
					if (nearest_neighbors[user] != null)
						if (nearest_neighbors[user].Contains(r_user))
							retrain_users.Add(user);
			retrain_users.UnionWith(removing_users);
			foreach (int r_user in retrain_users)
				nearest_neighbors[r_user] = correlation.GetNearestNeighbors(r_user, k);
		}

		/// <summary>
		/// Adds the user.
		/// </summary>
		/// <param name='user_id'>
		/// User_id.
		/// </param>
		protected override void AddUser(int user_id)
		{
			base.AddUser(user_id);
			ResizeNearestNeighbors(user_id + 1);
		}

	}
}