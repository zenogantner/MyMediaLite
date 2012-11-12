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
				this.nearest_neighbors = new int[num_items][];
				for (int i = 0; i < num_items; i++)
					nearest_neighbors[i] = correlation.GetNearestNeighbors(i, k);
			}
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
			Dictionary<int,List<int>> feeddict = new Dictionary<int, List<int>>();
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				List<int> rated_items = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> new_items = f.Value;
				foreach (int i in rated_items)
				{
					foreach (int j in new_items)
					{
						cooccurrence[i, j]--;
						switch(Correlation) 
						{
						case BinaryCorrelationType.Cooccurrence:
							correlation = cooccurrence;
							break;
						case BinaryCorrelationType.Cosine:
							if (i == j)
								correlation[i, i] = 1;
							else
								correlation[i, j] =  cooccurrence[i, j] / 
									(float) Math.Sqrt(cooccurrence[i, i] * cooccurrence[j, j]);
							break;
						default:
							throw new NotImplementedException("Incremental updates with ItemKNN only work with cosine and coocurrence (so far)");
						}
					}
				}
			}
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
			foreach (var tpl in feedback)
			{
				if (!feeddict.ContainsKey(tpl.Item1))
					feeddict.Add(tpl.Item1, new List<int>());
				feeddict[tpl.Item1].Add(tpl.Item2);
			}
			foreach (KeyValuePair<int, List<int>> f in feeddict)
			{
				List<int> rated_items = DataMatrix.GetEntriesByColumn(f.Key).ToList();
				List<int> removed_items = f.Value;
				foreach (int i in rated_items)
				{
					foreach (int j in removed_items)
					{
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
				}
			}
		}
	}
}