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
using System.Globalization;
using System.Collections.Generic;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted user-based kNN with Pearson correlation</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// </remarks>
	public class UserKNNPearson : UserKNN
	{
		/// <summary>shrinkage (regularization) parameter</summary>
		public float Shrinkage { get { return shrinkage; } set { shrinkage = value; } }
		private float shrinkage = 10;

		///
		public override void Train()
		{
			baseline_predictor.Train();
			this.correlation = Pearson.Create(ratings, EntityType.USER, Shrinkage);
		}

		///
		protected override void RetrainUser(int user_id)
		{
			baseline_predictor.RetrainUser(user_id);
			if (UpdateUsers)
				for (int u = 0; u <= MaxUserID; u++)
					correlation[user_id, u] = Pearson.ComputeCorrelation(ratings, EntityType.USER, user_id, u, Shrinkage);
		}

		///
		protected override IList<float> FoldIn(IList<Tuple<int, float>> rated_items)
		{
			var user_similarities = new float[MaxUserID + 1];
			for (int u = 0; u <= MaxUserID; u++)
				user_similarities[u] = Pearson.ComputeCorrelation(ratings, EntityType.USER, rated_items, u, Shrinkage);

			return user_similarities;
		}
		
		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} k={1} shrinkage={2} reg_u={3} reg_i={4} num_iter={5}",
				this.GetType().Name, K == uint.MaxValue ? "inf" : K.ToString(), Shrinkage, RegU, RegI, NumIter);
		}
	}
}