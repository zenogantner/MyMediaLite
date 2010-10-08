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

using MyMediaLite.correlation;
using MyMediaLite.data_type;
using MyMediaLite.taxonomy;


namespace MyMediaLite.rating_predictor
{
	/// <summary>
	/// user-based kNN with Pearson correlation
	/// </summary>
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

		/// <inheritdoc />
		protected override void RetrainUser(int user_id)
		{
			base.RetrainUser(user_id);
			if (UpdateUsers)
			{
				for (int i = 0; i < MaxUserID; i++)
				{
					float cor = Pearson.ComputeCorrelation(ratings.ByUser[user_id], ratings.ByUser[i], EntityType.USER, user_id, i, (float) shrinkage);
					correlation.data.Set(user_id, i, cor);
					correlation.data.Set(i, user_id, cor);
				}
			}
		}

        /// <inheritdoc />
		public override string ToString()
		{
			return string.Format("user-kNN-pearson k={0}, shrinkage={1}, reg_u={2}, reg_i={3}",
			                     k == uint.MaxValue ? "inf" : k.ToString(), shrinkage, reg_u, reg_i);
		}
	}
}