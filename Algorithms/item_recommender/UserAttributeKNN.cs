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
		protected BinaryAttributes user_attributes;
		/// <inheritdoc />
	    public int NumUserAttributes { get;	set; }


        /// <inheritdoc />
        public override void Train()
        {
            int num_users = max_user_id + 1;
			correlation = new Cosine(num_users);
			correlation.ComputeCorrelations(user_attributes.GetAttributes());

			nearest_neighbors = new int[max_user_id + 1][];
			for (int u = 0; u < num_users; u++)
				nearest_neighbors[u] = correlation.GetNearestNeighbors(u, k);
        }

		/// <inheritdoc />
		public void SetUserAttributeData(SparseBooleanMatrix matrix, int num_attr)
		{
			this.user_attributes = new BinaryAttributes(matrix);
			this.NumUserAttributes = num_attr;

			// TODO check whether there is a match between num. of entities here and in the collaborative data
		}

        /// <inheritdoc />
		public override string ToString()
		{
			return "user-attribute-kNN";
		}
    }
}