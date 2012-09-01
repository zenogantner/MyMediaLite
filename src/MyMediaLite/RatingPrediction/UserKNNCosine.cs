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
using System.Globalization;
using System.Linq;
using MyMediaLite.Correlation;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted user-based kNN with cosine similarity</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// </remarks>
	public class UserKNNCosine : UserKNN
	{
		IBinaryDataCorrelationMatrix BinaryDataCorrelation { get { return correlation as IBinaryDataCorrelationMatrix; } }
		
		///
		public override void Train()
		{
			baseline_predictor.Train();
			this.correlation = BinaryCosine.Create(data_user);
		}

		///
		protected override void RetrainUser(int user_id)
		{
			baseline_predictor.RetrainUser(user_id);
			if (UpdateUsers)
			{
				var user_items = new HashSet<int>(data_user[user_id]);
				for (int u = 0; u <= MaxUserID; u++)
					correlation[user_id, u] = BinaryDataCorrelation.ComputeCorrelation(user_items, new HashSet<int>(data_user[u]));
			}
		}

		///
		protected override IList<float> FoldIn(IList<Tuple<int, float>> rated_items)
		{
			var user_items = new HashSet<int>(from pair in rated_items select pair.Item1);

			var user_similarities = new float[MaxUserID + 1];
			for (int user_id = 0; user_id <= MaxUserID; user_id++)
				user_similarities[user_id] = BinaryDataCorrelation.ComputeCorrelation(user_items, new HashSet<int>(data_user[user_id]));

			return user_similarities;
		}

		///
		public override string ToString()
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} k={1} reg_u={2} reg_i={3} num_iter={4}",
				this.GetType().Name, K == uint.MaxValue ? "inf" : K.ToString(), RegU, RegI, NumIter);
		}
	}
}