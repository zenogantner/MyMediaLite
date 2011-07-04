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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyMediaLite.Correlation;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted kNN recommender based on user attributes</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class UserAttributeKNN : UserKNN, IUserAttributeAwareRecommender
	{
		///
		public SparseBooleanMatrix UserAttributes
		{
			get { return this.user_attributes; }
			set	{
				this.user_attributes = value;
				this.NumUserAttributes = user_attributes.NumberOfColumns;
				this.MaxUserID = Math.Max(MaxUserID, user_attributes.NumberOfRows - 1);
			}
		}
		private SparseBooleanMatrix user_attributes;

		///
		public int NumUserAttributes { get; private set; }

		///
		public override void Train()
		{
			base.Train();
			this.correlation = BinaryCosine.Create(user_attributes);
		}

		///
		public override string ToString()
		{
			return string.Format("UserAttributeKNN k={0} reg_u={1} reg_i={2}",
								 K == uint.MaxValue ? "inf" : K.ToString(), RegU, RegI);
		}
	}

}

