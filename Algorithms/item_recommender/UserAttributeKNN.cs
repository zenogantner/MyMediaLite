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
using MyMediaLite.data;
using MyMediaLite.data_type;


namespace MyMediaLite.item_recommender
{
	/// <remarks>
    /// k-nearest neighbor user-based collaborative filtering using cosine-similarity over the user attibutes
    /// k=\infty.
    ///
    /// This engine does not support online updates.
    /// </remarks>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class UserAttributeKNN : UserKNN, UserAttributeAwareRecommender
    {
		/// <inheritdoc />
		public SparseBooleanMatrix UserAttributes
		{
			set
			{
				this.user_attributes = value;
				//this.MaxUserID = Math.Max(MaxUserID, user_attributes.GetNumberOfRows());
			}
		}
		private SparseBooleanMatrix user_attributes;

		/// <inheritdoc />
		public int NumUserAttributes { get; set; }		
		
        /// <inheritdoc />
        public override void Train()
        {
            int num_users = MaxUserID + 1;
			correlation = new Cosine(num_users);
			correlation.ComputeCorrelations(user_attributes);

			nearest_neighbors = new int[MaxUserID + 1][];
			for (int u = 0; u < num_users; u++)
				nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
        }

        /// <inheritdoc />
		public override string ToString()
		{
			return "user-attribute-kNN";
		}
    }
}