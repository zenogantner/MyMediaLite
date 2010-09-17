// Copyright (C) 2010 Zeno Gantner
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
using System.Globalization;
using System.IO;
using MyMediaLite.correlation;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.taxonomy;


namespace MyMediaLite.rating_predictor
{
	/// <summary>
	/// This engine supports online updates.
	/// </summary>
	/// <author>Zeno Gantner, University of Hildesheim</author>
	public abstract class UserKNN : KNN
	{
		protected SparseBooleanMatrix data_user;

		public override void SetCollaborativeData(RatingData ratings)
		{
			base.SetCollaborativeData(ratings);

            data_user = new SparseBooleanMatrix();
			foreach (RatingEvent r in ratings.all)
               	data_user.AddEntry(r.user_id, r.item_id);
		}

		/// <summary>
		/// Predict the rating of a given user for a given item.
		///
		/// If the user or the item are not known to the engine, the global average is returned.
		/// To avoid this behavior for unknown entities, use CanPredictRating() to check before.
		/// </summary>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
        public override double Predict(int user_id, int item_id)
        {
			IList<int> relevant_users = correlation.GetPositivelyCorrelatedEntities(user_id);

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = k;
			foreach (int user_id2 in relevant_users)
			{
				if (data_user.GetRow(user_id2).Contains(item_id))
				{
					RatingEvent r = ratings.byUser[user_id2].FindRating(user_id2, item_id);
					double weight = correlation.Get(user_id, user_id2);
					weight_sum += weight;
					sum += weight * (r.rating - base.Predict(user_id2, item_id));

					if (--neighbors == 0)
						break;
				}
			}

			double result = base.Predict(user_id, item_id);
			//Console.Error.Write(result);
			if (weight_sum != 0)
			{
				double modification = sum / weight_sum;
				//Console.Error.WriteLine("{0} + {1}", modification, ratings.byUser[user_id].Average);
				result += modification;
				//Console.Error.Write(" {0} ", modification);
			}

			if (result > MaxRatingValue)
				result = MaxRatingValue;
            if (result < MinRatingValue)
				result = MinRatingValue;
			return result;
        }

        /// <inheritdoc/>
        public override void AddRating(int user_id, int item_id, double rating)
        {
			base.AddRating(user_id, item_id, rating);
			data_user.AddEntry(user_id, item_id);
            RetrainUser(user_id);
        }

        /// <inheritdoc/>
        public override void UpdateRating(int user_id, int item_id, double rating)
        {
			base.UpdateRating(user_id, item_id, rating);
            RetrainUser(user_id);
        }

        /// <inheritdoc/>
        public override void RemoveRating(int user_id, int item_id)
        {
			base.RemoveRating(user_id, item_id);
			data_user.RemoveEntry(user_id, item_id);
            RetrainUser(user_id);
        }

        /// <inheritdoc/>
        public override void AddUser(int user_id)
        {
            base.AddUser(user_id);
			correlation.AddEntity(user_id);
        }
	}

	public class UserKNNCosine : UserKNN
	{
        /// <inheritdoc />
        public override void Train()
        {
			base.Train();

			correlation.Cosine cosine_correlation = new Cosine(MaxUserID + 1);
			cosine_correlation.ComputeCorrelations(data_user);
			this.correlation = cosine_correlation;


        }

		protected override void RetrainUser(int user_id)
		{
			base.RetrainUser(user_id);
			if (UpdateUsers)
			{
				for (int i = 0; i < MaxUserID; i++)
				{
					if (i == user_id)
						continue;

					float cor = Cosine.ComputeCorrelation(data_user.GetRow(user_id), data_user.GetRow(i));
					correlation.data.Set(user_id, i, cor);
					correlation.data.Set(i, user_id, cor);
				}
			}
		}

        /// <inheritdoc />
		public override string ToString()
		{
			return String.Format("user-kNN-cosine k={0} reg_u={1} reg_i={2}",
			                     k == UInt32.MaxValue ? "inf" : k.ToString(), reg_u, reg_i);
		}
	}

	public class UserKNNPearson : UserKNN
	{
        /// <inheritdoc />
        public override void Train()
        {
			base.Train();

			correlation.Pearson pearson_correlation = new Pearson(MaxUserID + 1);
			pearson_correlation.shrinkage = (float) this.shrinkage;
			pearson_correlation.ComputeCorrelations(ratings, EntityType.USER);
			this.correlation = pearson_correlation;
        }

		protected override void RetrainUser(int user_id)
		{
			base.RetrainUser(user_id);
			if (UpdateUsers)
			{
				for (int i = 0; i < MaxUserID; i++)
				{
					float cor = Pearson.ComputeCorrelation(ratings.byUser[user_id], ratings.byUser[i], EntityType.USER, user_id, i, (float) shrinkage);
					correlation.data.Set(user_id, i, cor);
					correlation.data.Set(i, user_id, cor);
				}
			}
		}

        /// <inheritdoc />
		public override string ToString()
		{
			return String.Format("user-kNN-pearson k={0}, shrinkage={1}, reg_u={2}, reg_i={3}",
			                     k == UInt32.MaxValue ? "inf" : k.ToString(), shrinkage, reg_u, reg_i);
		}
	}
}