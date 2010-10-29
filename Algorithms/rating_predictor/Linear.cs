// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
using System.Linq;
using System.Text;
using MyMediaLite.data;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
    /// <summary>
    /// Uses the average rating value over all ratings for prediction.
    /// </summary>
    /// <remarks>
    /// This engine does NOT support online updates.
    /// </remarks>
    public class GlobalAverage : Memory
    {
		private double global_average = 0;		
		
        /// <inheritdoc />
        public override void Train()
		{
			foreach (RatingEvent r in Ratings.All)
				global_average += r.rating;
			global_average /= Ratings.All.Count;			
		}

        /// <inheritdoc />
		public override bool CanPredict(int user_id, int item_id)
		{
			return true;
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            return global_average;
        }

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			// TODO
			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
				writer.WriteLine("All information you need is easily available in the rating data.");
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			// TODO
			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
			}
		}

		/// <summary>returns the name of the method</summary>
		/// <returns>A <see cref="System.String"/></returns>
		public override string ToString()
		{
			return "global-average";
		}
    }

    /// <summary>
    /// Abstract class that uses an average (by entity) rating value for predictions.
    /// </summary>
    /// <remarks>
    /// This engine does NOT support online updates.
    /// </remarks>
    public abstract class EntityAverage : Memory
    {
		/// <summary>The average rating for each entity</summary>
		protected List<double> entity_averages = new List<double>();

		/// <summary>The global average rating (default prediction if there is no data about an entity)</summary>
		protected double global_average = 0;

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			// TODO
			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
				writer.WriteLine("All information you need is easily available in the rating data.");
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			// TODO
			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
			}
		}
    }

    /// <summary>
    /// Uses the average rating value of a user for predictions.
    /// </summary>
    /// <remarks>
    /// This engine does NOT support online updates.
    /// </remarks>
    public class UserAverage : EntityAverage
    {
		/// <inheritdoc />
        public override void Train()
        {
			List<int> rating_counts = new List<int>();

			foreach (RatingEvent r in Ratings.All)
			{
				rating_counts[r.user_id]++;
				entity_averages[r.user_id] += r.rating;
				global_average += r.rating;
			}

			global_average /= Ratings.All.Count;

			for (int i = 0; i < entity_averages.Count; i++)
				if (rating_counts[i] != 0)
					entity_averages[i] /= rating_counts[i];
				else
					entity_averages[i] = global_average;
        }

        /// <inheritdoc />
		public override bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID);
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (user_id <= MaxUserID)
                return entity_averages[user_id];
			else
				return global_average;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return "user-average";
		}
    }

    /// <summary>
    /// Uses the average rating value of an item for prediction.
    /// </summary>
    /// <remarks>
    /// This engine does NOT support online updates.
    /// </remarks>
    public class ItemAverage : EntityAverage
    {
		/// <inheritdoc />
        public override void Train()
        {
			List<int> rating_counts = new List<int>();

			foreach (RatingEvent r in Ratings.All)
			{
				rating_counts[r.item_id]++;
				entity_averages[r.item_id] += r.rating;
				global_average += r.rating;
			}

			global_average /= Ratings.All.Count;

			for (int i = 0; i < entity_averages.Count; i++)
				if (rating_counts[i] != 0)
					entity_averages[i] /= rating_counts[i];
				else
					entity_averages[i] = global_average;
        }

        /// <inheritdoc />
		public override bool CanPredict(int user_id, int item_id)
		{
			return (item_id <= MaxItemID);
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (item_id <= MaxItemID)
                return entity_averages[item_id];
			else
				return global_average;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return "item-average";
		}
    }
}