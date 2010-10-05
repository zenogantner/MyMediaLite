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

namespace MyMediaLite.item_recommender
{
	/// <summary>
    /// Random predictions. Engine for use as experimental baseline.
    /// </summary>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class Random : ItemRecommender
    {
        /// <inheritdoc />
        public override void Train() { }

        /// <inheritdoc />
		public override double Predict(int user_id, int item_id)
		{
			return util.Random.GetInstance().NextDouble();
		}

        /// <inheritdoc />
		public override void SaveModel(string filePath)
		{
			throw new NotImplementedException();
		}

        /// <inheritdoc />
		public override void LoadModel(string filePath)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return "random";
		}
	}
}