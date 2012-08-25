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
using MyMediaLite.RatingPrediction;

/*! \namespace MyMediaLite.Ensemble
 *  \brief This namespace contains recommender ensembles.
 */
namespace MyMediaLite.Ensemble
{
	/// <summary>Abtract class for combining several prediction methods</summary>
	public abstract class Ensemble : IRecommender
	{
		/// <summary>list of recommenders</summary>
		public IList<IRecommender> recommenders = new List<IRecommender>();

		private float max_rating_value = 5;
		private float min_rating_value = 1;

		/// <summary>create a shallow copy of the object</summary>
		public Object Clone()
		{
			return this.MemberwiseClone();
		}

		///
		public abstract IList<Tuple<int, float>> Recommend(
			int user_id, int n = 20,
			ICollection<int> ignore_items = null,
			ICollection<int> candidate_items = null);

		/// <summary>The max rating value</summary>
		/// <value>The max rating value</value>
		public float MaxRating
		{
			get { return this.max_rating_value; }
			set {
				this.max_rating_value = value;
				foreach (var recommender in recommenders)
					if (recommender is RatingPredictor)
						((RatingPredictor)recommender).MaxRating = value;
			}
		}

		/// <summary>The min rating value</summary>
		/// <value>The min rating value</value>
		public float MinRating
		{
			get { return this.min_rating_value; }
			set {
				this.min_rating_value = value;
				foreach (var recommender in recommenders)
					if (recommender is RatingPredictor)
						((RatingPredictor)recommender).MinRating = value;
			}
		}

		///
		public abstract float Predict(int user_id, int item_id);

		///
		public virtual bool CanPredict(int user_id, int item_id)
		{
			foreach (var recommender in recommenders)
				if (!recommender.CanPredict(user_id, item_id))
					return false;
			return true;
		}

		///
		public abstract void SaveModel(string file);
		///
		public abstract void LoadModel(string file);

		///
		public virtual void Train()
		{
			foreach (var recommender in recommenders)
				recommender.Train();
		}
	}
}
