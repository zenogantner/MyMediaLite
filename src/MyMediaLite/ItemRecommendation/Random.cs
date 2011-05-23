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

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Random item recommender for use as experimental baseline</summary>
	/// <remarks>
	/// It would not be necessary for Random to inherit from ItemRecommender, but it is done nonetheless for convenience.
	/// </remarks>
	public class Random : ItemRecommender
	{
		///
		public override void Train() { }

		///
		public override double Predict(int user_id, int item_id)
		{
			return Util.Random.GetInstance().NextDouble();
		}

		///
		public override void SaveModel(string filename)
		{
			// do nothing
		}

		///
		public override void LoadModel(string filename)
		{
			// do nothing
		}

		///
		public override string ToString()
		{
			return "Random";
		}
	}
}