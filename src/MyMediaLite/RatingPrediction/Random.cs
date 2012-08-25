// Copyright (C) 2011, 2012 Zeno Gantner
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
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Uses a random rating value for prediction</summary>
	/// <remarks>
	/// This recommender supports incremental updates.
	/// Updates are just ignored, because the predictions are always uniformly sampled from the interval of rating values.
	/// </remarks>
	public class Random : IncrementalRatingPredictor
	{
		///
		public override void Train() { }

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			return true;
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			return (float) (MinRating + MyMediaLite.Random.GetInstance().NextDouble() * (MaxRating - MinRating));
		}

		///
		public override void SaveModel(string filename) { /* do nothing */ }

		///
		public override void LoadModel(string filename) { /* do nothing */ }
	}
}