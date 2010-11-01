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

using System;
using MyMediaLite.correlation;
using MyMediaLite.data_type;


namespace MyMediaLite.item_recommender
{
	/// <summary>
    /// k-nearest neighbor user-based collaborative filtering using cosine-similarity over the user attibutes
    /// </summary>
	/// <remarks>
    /// This engine does not support online updates.
    /// </remarks>
    public class UserAttributeKNN : UserKNN, IUserAttributeAwareRecommender
    {
		/// <inheritdoc />
		public SparseBooleanMatrix UserAttributes
		{
			set
			{
				this.user_attributes = value;
				this.MaxUserID = Math.Max(MaxUserID, user_attributes.NumberOfRows);
			}
		}
		private SparseBooleanMatrix user_attributes;

		/// <inheritdoc />
		public int NumUserAttributes { get; set; }		
		
        /// <inheritdoc />
        public override void Train()
        {
			correlation = Cosine.Create(user_attributes);

			int num_users = user_attributes.NumberOfRows;
			nearest_neighbors = new int[num_users][];
			for (int u = 0; u < num_users; u++)
				nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
			
			Console.WriteLine("MaxUserID: {0}", MaxUserID);
        }

        /// <inheritdoc />
		public override string ToString()
		{
			return string.Format("user-attribute-kNN k={0}", k == uint.MaxValue ? "inf" : k.ToString());
		}
    }
}