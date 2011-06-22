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

using System.Globalization;
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
		public UserKNNPearson() : base() { }

		///
		public override void Train()
		{
			base.Train();
			this.correlation = Pearson.Create(ratings, EntityType.USER, Shrinkage);
		}

		///
		protected override void RetrainUser(int user_id)
		{
			base.RetrainUser(user_id);
			if (UpdateUsers)
				for (int i = 0; i <= MaxUserID; i++)
					correlation[user_id, i] = Pearson.ComputeCorrelation(ratings, EntityType.USER, user_id, i, Shrinkage);
		}

		///
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
								 "UserKNNPearson k={0} shrinkage={1} reg_u={2} reg_i={3}",
								 K == uint.MaxValue ? "inf" : K.ToString(), Shrinkage, RegU, RegI);
		}
	}
}