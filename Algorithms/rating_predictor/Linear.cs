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
	///
	/// This engine does support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class GlobalAverage : Memory
    {
        /// <inheritdoc />
        public override void Train()
        {
        }

        /// <inheritdoc />
		public override bool CanPredict(int user_id, int item_id)
		{
			return true;
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            return ratings.Average;
        }

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
				writer.WriteLine("All information you need is easily available in the rating data.");
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
			}
		}

		/// <summary>
		/// returns the name of the method
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString()
		{
			return "global-average";
		}
    }

    /// <summary>
    /// Abstract class that uses an average (by entity) rating value for predictions.
    /// This engine does support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public abstract class EntityAverage : Memory
    {
		/// <inheritdoc />
        public override void Train()
        {
        }

		/// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			using ( StreamWriter writer = EngineStorage.GetWriter(filePath, this.GetType()) )
				writer.WriteLine("All information you need is easily available in the rating data.");
		}

		/// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			using ( StreamReader reader = EngineStorage.GetReader(filePath, this.GetType()) )
			{
			}
		}
    }

    /// <summary>
    /// Uses the average rating value of a user for predictions.
    /// This engine does support online updates.
    /// </summary>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class UserAverage : EntityAverage
    {
        /// <inheritdoc />
		public override bool CanPredict(int user_id, int item_id)
		{
			return (user_id <= MaxUserID);
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (user_id < MaxUserID && ratings.ByUser[user_id].Count != 0)
                return ratings.ByUser[user_id].Average;
            else
				return ratings.Average;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return "user-average";
		}
    }

    /// <summary>
    /// Uses the average rating value of an item for prediction.
    /// This engine does support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class ItemAverage : EntityAverage
    {
        /// <inheritdoc />
		public override bool CanPredict(int user_id, int item_id)
		{
			return (item_id <= MaxItemID);
		}

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if (item_id < MaxItemID && ratings.ByItem[item_id].Count != 0)
                return ratings.ByItem[item_id].Average;
			else
				return ratings.Average;
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return "item-average";
		}
    }
}