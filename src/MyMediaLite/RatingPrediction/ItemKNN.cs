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
using MyMediaLite;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;
using MyMediaLite.Data;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted item-based kNN</summary>
	public class ItemKNN : KNN, IItemSimilarityProvider, IFoldInRatingPredictor
	{
		/// <summary>Matrix indicating which item was rated by which user</summary>
		protected SparseBooleanMatrix data_item;

		///
		public override IRatings Ratings
		{
			set {
				base.Ratings = value;

				data_item = new SparseBooleanMatrix();
				for (int index = 0; index < ratings.Count; index++)
					data_item[ratings.Items[index], ratings.Users[index]] = true;
			}
		}

		///
		protected override EntityType Entity { get { return EntityType.ITEM; } }

		///
		protected override IBooleanMatrix BinaryDataMatrix { get { return data_item; } }

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the recommender, a suitable average is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override float Predict(int user_id, int item_id)
		{
			float result = baseline_predictor.Predict(user_id, item_id);

			if ((user_id > MaxUserID) || (item_id > correlation.NumberOfRows - 1))
				return result;

			IList<int> correlated_items = correlation.GetPositivelyCorrelatedEntities(item_id);

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = K;
			foreach (int item_id2 in correlated_items)
				if (data_item[item_id2, user_id])
				{
					float rating  = ratings.Get(user_id, item_id2, ratings.ByUser[user_id]);
					double weight = correlation[item_id, item_id2];
					weight_sum += weight;
					sum += weight * (rating - baseline_predictor.Predict(user_id, item_id2));

					if (--neighbors == 0)
						break;
				}

			if (weight_sum != 0)
				result += (float) (sum / weight_sum);

			if (result > MaxRating)
				result = MaxRating;
			if (result < MinRating)
				result = MinRating;
			return result;
		}

		/// <summary>Retrain model for a given item</summary>
		/// <param name='item_id'>the item ID</param>
		protected virtual void RetrainItem(int item_id)
		{
			baseline_predictor.RetrainItem(item_id);

			if (UpdateItems)
			{
				if (correlation is IBinaryDataCorrelationMatrix)
				{
					var bin_cor = correlation as IBinaryDataCorrelationMatrix;
					var item_users = new HashSet<int>(BinaryDataMatrix[item_id]);
					for (int other_item_id = 0; other_item_id <= MaxItemID; other_item_id++)
						if (bin_cor.IsSymmetric)
						{
							correlation[item_id, other_item_id] = bin_cor.ComputeCorrelation(item_users, new HashSet<int>(BinaryDataMatrix[other_item_id]));
						}
						else
						{
							var other_item_users = new HashSet<int>(BinaryDataMatrix[other_item_id]);
							correlation[item_id, other_item_id] = bin_cor.ComputeCorrelation(item_users, other_item_users);
							correlation[other_item_id, item_id] = bin_cor.ComputeCorrelation(other_item_users, item_users);
						}
				}
				if (correlation is IRatingCorrelationMatrix)
				{
					var rat_cor = correlation as IRatingCorrelationMatrix;
					for (int other_item_id = 0; other_item_id <= MaxItemID; other_item_id++)
						correlation[item_id, other_item_id] = rat_cor.ComputeCorrelation(ratings, EntityType.ITEM, item_id, other_item_id);
				}
			}
		}

		///
		public override void AddRatings(IRatings ratings)
		{
			baseline_predictor.AddRatings(ratings);
			for (int index = 0; index < ratings.Count; index++)
				data_item[ratings.Items[index], ratings.Users[index]] = true;
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void UpdateRatings(IRatings ratings)
		{
			baseline_predictor.UpdateRatings(ratings);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void RemoveRatings(IDataSet ratings)
		{
			baseline_predictor.RemoveRatings(ratings);
			for (int index = 0; index < ratings.Count; index++)
				data_item[ratings.Items[index], ratings.Users[index]] = false;
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		protected override void AddItem(int item_id)
		{
			correlation.AddEntity(item_id);
		}

		float Predict(IList<Tuple<int, float>> rated_items, int item_id)
		{
			float result = baseline_predictor.Predict(int.MaxValue, item_id);

			if (item_id > correlation.NumberOfRows - 1)
				return result;

			IList<int> correlated_items = correlation.GetPositivelyCorrelatedEntities(item_id);
			var item_ratings = new Dictionary<int, float>();
			foreach (var t in rated_items)
				item_ratings.Add(t.Item1, t.Item2); // TODO handle several ratings of the same item

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = K;
			foreach (int item_id2 in correlated_items)
				if (item_ratings.ContainsKey(item_id2))
				{
					float rating  = item_ratings[item_id2];
					double weight = correlation[item_id, item_id2];
					weight_sum += weight;
					sum += weight * (rating - baseline_predictor.Predict(int.MaxValue, item_id2));

					if (--neighbors == 0)
						break;
				}

			if (weight_sum != 0)
				result += (float) (sum / weight_sum);

			if (result > MaxRating)
				result = MaxRating;
			if (result < MinRating)
				result = MinRating;
			return result;
		}

		///
		public IList<Tuple<int, float>> ScoreItems(IList<Tuple<int, float>> rated_items, IList<int> candidate_items)
		{
			// score the items
			var result = new Tuple<int, float>[candidate_items.Count];
			for (int i = 0; i < candidate_items.Count; i++)
			{
				int item_id = candidate_items[i];
				result[i] = Tuple.Create(item_id, Predict(rated_items, item_id));
			}
			return result;
		}

		///
		public float GetItemSimilarity(int item_id1, int item_id2)
		{
			return correlation[item_id1, item_id2];
		}

		///
		public IList<int> GetMostSimilarItems(int item_id, uint n = 10)
		{
			return correlation.GetNearestNeighbors(item_id, n);
		}
	}
}