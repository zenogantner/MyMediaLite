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
using MyMediaLite.util;


namespace MyMediaLite.item_recommender
{
	// TODO implement non-weighted, non-inf kNN

	/// <summary>
    /// k-nearest neighbor item-based collaborative filtering using cosine-similarity
    /// k=\infty.
    ///
    /// This engine does not support online updates.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class ItemKNN : KNN
    {
        /// <inheritdoc />
        public override void Train()
        {
            int num_items = max_item_id + 1;
			correlation = new Cosine(num_items);
			correlation.ComputeCorrelations(data_item);
        }

        /// <inheritdoc />
        public override double Predict(int user_id, int item_id)
        {
            if ((user_id < 0) || (user_id > max_user_id))
                throw new ArgumentException("user is unknown: " + user_id);
            if ((item_id < 0) || (item_id > max_item_id))
                throw new ArgumentException("item is unknown: " + item_id);

			return correlation.SumUp(item_id, data_user.GetRow(user_id));
        }

		/// <inheritdoc />
		public override string ToString()
		{
			return String.Format("item-kNN, k={0}" , k == UInt32.MaxValue ? "inf" : k.ToString());
		}
    }
}