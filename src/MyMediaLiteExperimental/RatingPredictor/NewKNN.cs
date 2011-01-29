// Copyright (C) 2010, 2011 Zeno Gantner
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
using System.Reflection;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Util;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>kNN-based rating predictors</summary>
	/// <remarks>
	/// The method is described in section 2.2 of
	/// <article>
	///   <author>Yehuda Koren</author>
	///   <title>Factor in the Neighbors: Scalable and Accurate Collaborative Filtering</title>
	///   <journal>Transactions on Knowledge Discovery from Data (TKDD)</journal>
	///   <year>2009</year>
	/// </article>
	///
	/// This engine does NOT support online updates.
	///
	/// <seealso cref="ItemRecommender.KNN"/>
	/// </remarks>
	public class NewKNN : UserItemBaseline
	{
		// TODO add possibility of _not_ using weights

		/// <summary>Shrinkage parameter</summary>
		public float Shrinkage { get; set; }

		public string Similarity { get; set; }

		public EntityType Entity { get; set; }

		/// <summary>Number of neighbors to take into account for predictions</summary>
		public uint K { get; set; }

        /// <summary>Correlation matrix over some kind of entity</summary>
        private CorrelationMatrix correlation;

		private SparseBooleanMatrix entity_data = new SparseBooleanMatrix();

		public NewKNN()
		{
			K = 60;
			Shrinkage = 10;
			Similarity = "Pearson";
			Entity = EntityType.USER;
		}

		private void CreateSimilarityMatrix(string typename)
		{
			Type type = Type.GetType("MyMediaLite.Correlation." + typename, true);

			if (type.IsSubclassOf(typeof(CorrelationMatrix)))
				correlation = (CorrelationMatrix) type.GetConstructor(new Type[] { typeof(int) } ).Invoke( new object[] { Entity == EntityType.USER ? MaxUserID + 1 : MaxItemID + 1 });
			else
				throw new Exception(typename + " is not a subclass of CorrelationMatrix");
		}

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the engine, a suitable average rating is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
        public override double Predict(int user_id, int item_id)
        {
            if (user_id < 0)
                throw new ArgumentException("user is unknown: " + user_id);
            if (item_id < 0)
                throw new ArgumentException("item is unknown: " + item_id);

            if ((user_id > MaxUserID) || (item_id > MaxItemID))
                return base.Predict(user_id, item_id);

			double sum = 0;
			double weight_sum = 0;

			uint neighbors = K;

			if (Entity == EntityType.USER)
			{
				IList<int> relevant_users = correlation.GetPositivelyCorrelatedEntities(user_id);

				foreach (int user_id2 in relevant_users)
					if (entity_data[user_id2, item_id])
					{
						RatingEvent r = ratings.ByUser[user_id2].FindRating(user_id2, item_id);
						double weight = correlation[user_id, user_id2];
						weight_sum += weight;
						sum += weight * (r.rating - base.Predict(user_id2, item_id));

						if (--neighbors == 0)
							break;
					}
			}
			else if (Entity == EntityType.ITEM)
			{
				IList<int> relevant_items = correlation.GetPositivelyCorrelatedEntities(item_id);

				foreach (int item_id2 in relevant_items)
					if (entity_data[item_id2, user_id])
					{
						RatingEvent r = ratings.ByItem[item_id2].FindRating(user_id, item_id2);
						double weight = correlation[item_id, item_id2];
						weight_sum += weight;
						sum += weight * (r.rating - base.Predict(user_id, item_id2));

						if (--neighbors == 0)
							break;
					}
			}
			else
			{
				throw new ArgumentException("Unknown entity type: " + Entity);
			}

			double result = base.Predict(user_id, item_id);
			if (weight_sum != 0)
				result += sum / weight_sum;

			if (result > MaxRating)
				result = MaxRating;
            if (result < MinRating)
				result = MinRating;
			return result;
        }

        /// <inheritdoc/>
        public override void Train()
        {
			base.Train();

			CreateSimilarityMatrix(Similarity);

			if (correlation is RatingCorrelationMatrix)
				((RatingCorrelationMatrix) correlation).ComputeCorrelations(ratings, Entity);

			if (correlation is BinaryDataCorrelationMatrix)
			{
				this.entity_data = new SparseBooleanMatrix();
				if (Entity == EntityType.USER)
					foreach (RatingEvent r in ratings)
	               		entity_data[r.user_id, r.item_id] = true;
				else if (Entity == EntityType.ITEM)
					foreach (RatingEvent r in ratings)
	               		entity_data[r.item_id, r.user_id] = true;
				else
					throw new ArgumentException("Unknown entity type: " + Entity);

				((BinaryDataCorrelationMatrix) correlation).ComputeCorrelations(entity_data);
			}
        }

		/// <inheritdoc/>
		public override void SaveModel(string filename)
		{
			// TODO extend
			using ( StreamWriter writer = Engine.GetWriter(filename, this.GetType()) )
				correlation.Write(writer);
		}

		/// <inheritdoc/>
		public override void LoadModel(string filename)
		{
			// TODO extend
            using ( StreamReader reader = Engine.GetReader(filename, this.GetType()) )
			{
				CorrelationMatrix correlation = CorrelationMatrix.ReadCorrelationMatrix(reader);

				base.Train(); // train baseline model
				this.correlation = new BinaryCosine(correlation);
			}
		}

        /// <inheritdoc/>
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni,
			                     "NewKNN k={0} entity_type={1}, similarity={2} shrinkage={3} reg_u={4} reg_i={5}",
			                     K == uint.MaxValue ? "inf" : K.ToString(), Entity, Similarity, Shrinkage, RegU, RegI);
		}
	}
}