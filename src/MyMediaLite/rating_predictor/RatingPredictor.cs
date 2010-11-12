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


namespace MyMediaLite.rating_predictor
{
    /// <summary>Abstract class for rating predictors</summary>
    public abstract class RatingPredictor : IRecommenderEngine
    {
        /// <summary>The max rating value</summary>
        public virtual double MaxRating { get { return max_rating; } set { max_rating = value; } }
        /// <summary>The max rating value</summary>
        protected double max_rating;

		/// <summary>The min rating value</summary>
        public virtual double MinRating { get { return min_rating;  } set { min_rating = value; } }
	    /// <summary>The min rating value</summary>
	    protected double min_rating;

        /// <inheritdoc/>
		public abstract bool CanPredict(int user_id, int item_id);
		/// <inheritdoc/>
        public abstract double Predict(int user_id, int item_id);
        /// <inheritdoc/>
        public virtual void AddRating(int user_id, int item_id, double rating) { }
        /// <inheritdoc/>
        public virtual void UpdateRating(int user_id, int item_id, double rating) { }
        /// <inheritdoc/>
        public virtual void RemoveRating(int user_id, int item_id) { }
        /// <inheritdoc/>
        public virtual void AddUser(int user_id) { }
        /// <inheritdoc/>
        public virtual void AddItem(int item_id) { }
        /// <inheritdoc/>
        public virtual void RemoveUser(int user_id) { }
        /// <inheritdoc/>
        public virtual void RemoveItem(int item_id) { }

		/// <inheritdoc/>
		public abstract void SaveModel(string filePath);
		/// <inheritdoc/>
		public abstract void LoadModel(string filePath);

        /// <inheritdoc/>
        public abstract void Train();

		/// <summary>Return a string representation of the engine</summary>
		/// <remarks>
		/// ToString() method of recommender engines should list all hyperparameters, separated by space characters.
		/// </remarks>
		public abstract override string ToString();
    }
}
