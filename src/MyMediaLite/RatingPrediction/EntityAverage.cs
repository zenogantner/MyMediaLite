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

using System.Collections.Generic;

namespace MyMediaLite.RatingPrediction
{
    /// <summary>Abstract class that uses an average (by entity) rating value for predictions</summary>
    /// <remarks>
    /// This engine does NOT support online updates.
    /// </remarks>
    public abstract class EntityAverage : Memory
    {
		/// <summary>The average rating for each entity</summary>
		protected List<double> entity_averages = new List<double>();

		/// <summary>The global average rating (default prediction if there is no data about an entity)</summary>
		protected double global_average = 0;

		/// <inheritdoc/>
		public override void SaveModel(string file)
		{
			// do nothing
		}

		/// <inheritdoc/>
		public override void LoadModel(string file)
		{
			Train();
		}
    }
}