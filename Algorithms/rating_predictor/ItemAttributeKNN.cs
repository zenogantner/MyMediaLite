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
using System.Collections.Generic;
using MyMediaLite.correlation;
using MyMediaLite.data;
using MyMediaLite.data_type;
using MyMediaLite.util;


namespace MyMediaLite.rating_predictor
{
	/// <summary>
	/// This engine does NOT support online updates.
	/// </summary>
	public class ItemAttributeKNN : ItemKNN, ItemAttributeAwareRecommender
	{
		/// <inheritdoc />
		public SparseBooleanMatrix ItemAttributes			
		{
			set
			{
				this.item_attributes = value;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows);
			}
		}		
		private SparseBooleanMatrix item_attributes;
		
		/// <inheritdoc/>
	    public int NumItemAttributes { get;	set; }

        /// <inheritdoc />
        public override void Train()
        {
			base.Train();

			correlation.Cosine cosine_correlation = new Cosine(MaxItemID + 1);
			cosine_correlation.ComputeCorrelations(item_attributes);
			this.correlation = cosine_correlation;

			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
        }

        /// <inheritdoc />
		public override string ToString()
		{
			return String.Format("item-attribute-kNN k={0} reg_u={1} reg_i={2}",
			                     k == UInt32.MaxValue ? "inf" : k.ToString(), reg_u, reg_i);
		}
	}

}
