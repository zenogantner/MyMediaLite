// Copyright (C) 2011, 2012, 2013 Zeno Gantner
// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
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
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Uses the average rating value over all ratings for prediction</summary>
	public class GlobalAverage : RatingPredictor
	{
		private float global_average = 0;

		///
		public override void Train()
		{
			global_average = Interactions.AverageRating();
		}

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			return true;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			return global_average;
		}

		///
		public override void SaveModel(string filename)
		{
		}

		///
		public override void LoadModel(string filename)
		{
		}
	}
}