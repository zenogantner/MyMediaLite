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
using MyMediaLite.Correlation;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted user-based kNN with cosine similarity</summary>
	public class UserKNNCosine : UserKNN
	{
		///
		public UserKNNCosine() : base() { }

		///
		public override void Train()
		{
			base.Train();
			this.correlation = BinaryCosine.Create(data_user);
		}

		///
		protected override void RetrainUser(int user_id)
		{
			base.RetrainUser(user_id);
			if (UpdateUsers)
				for (int i = 0; i <= MaxUserID; i++)
					correlation[user_id, i] = BinaryCosine.ComputeCorrelation(new HashSet<int>(data_user[user_id]), new HashSet<int>(data_user[i]));
		}

		///
		public override string ToString()
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			return string.Format(ni,
								 "UserKNNCosine k={0} reg_u={1} reg_i={2}",
								 K == uint.MaxValue ? "inf" : K.ToString(), RegU, RegI);
		}
	}
}