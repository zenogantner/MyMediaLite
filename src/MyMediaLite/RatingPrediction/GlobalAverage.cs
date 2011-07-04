// Copyright (C) 2010 Zeno Gantner, Steffen Rendle
// Copyright (C) 2011 Zeno Gantner
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
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Uses the average rating value over all ratings for prediction</summary>
	/// <remarks>
	/// This recommender does NOT support incremental updates.
	/// </remarks>
	public class GlobalAverage : RatingPredictor
	{
		private double global_average = 0;

		///
		public override void Train()
		{
			global_average = Ratings.Average;
		}

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			return true;
		}

		///
		public override double Predict(int user_id, int item_id)
		{
			return global_average;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Recommender.GetWriter(filename, this.GetType()) )
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Recommender.GetReader(filename, this.GetType()) )
				this.global_average = double.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
		}
	}
}