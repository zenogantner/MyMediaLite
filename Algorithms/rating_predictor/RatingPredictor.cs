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
using System.Text;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
    /// <summary>
    /// Abstract class for rating predictors
    /// </summary>
    /// <author>Steffen Rendle, University of Hildesheim</author>
    public abstract class RatingPredictor : RecommenderEngine
    {
        /// <inheritdoc />
		public abstract bool CanPredict(int user_id, int item_id);
		/// <inheritdoc />
        public abstract double Predict(int user_id, int item_id);
        /// <inheritdoc />
        public virtual void AddRating(int user_id, int item_id, double rating) { }
        /// <inheritdoc />
        public virtual void UpdateRating(int user_id, int item_id, double rating) { }
        /// <inheritdoc />
        public virtual void RemoveRating(int user_id, int item_id) { }
        /// <inheritdoc />
        public virtual void AddUser(int user_id) { }
        /// <inheritdoc />
        public virtual void AddItem(int item_id) { }
        /// <inheritdoc />
        public virtual void RemoveUser(int user_id) { }
        /// <inheritdoc />
        public virtual void RemoveItem(int item_id) { }

		/// <inheritdoc />
		public abstract void SaveModel(string filePath);
		/// <inheritdoc />
		public abstract void LoadModel(string filePath);
        /// <inheritdoc />
        protected double max_data_value;
        /// <inheritdoc />
        protected double min_data_value;

        /// <inheritdoc />
        public abstract void Train();

		public override string ToString()
		{
			return "RatingPredictor";
		}

        /// <summary>
        /// Gets or sets the max rating value.
        /// </summary>
        /// <value>The max rating value.</value>
        public virtual double MaxRatingValue
        {
            get { return this.max_data_value;  }
            set { this.max_data_value = value; }
        }

        /// <summary>
        /// Gets or sets the min rating value.
        /// </summary>
        /// <value>The min rating value.</value>
        public virtual double MinRatingValue
        {
            get { return this.min_data_value;  }
            set { this.min_data_value = value; }
        }
    }
}
