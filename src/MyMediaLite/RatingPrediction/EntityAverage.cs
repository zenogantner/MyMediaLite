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
	public abstract class EntityAverage : RatingPredictor
	{
		/// <summary>The average rating for each entity</summary>
		protected List<double> entity_averages = new List<double>();

		/// <summary>The global average rating (default prediction if there is no data about an entity)</summary>
		protected double global_average = 0;

		/// <summary>return the average rating for a given entity</summary>
		/// <param name="index">the entity index</param>
		public double this [int index] {
			get {
				if (index < entity_averages.Count)
					return entity_averages[index];
				else
					return global_average;
				}
		}

		/// <summary>Train the recommender according to the given entity type</summary>
		/// <param name="entity_ids">list of the relevant entity IDs in the training data</param>
		/// <param name="max_entity_id">the maximum entity ID</param>
		protected void Train(IList<int> entity_ids, int max_entity_id)
		{
			var rating_counts = new List<int>();
			for (int i = 0; i <= max_entity_id; i++)
			{
				rating_counts.Add(0);
				entity_averages.Add(0);
			}

			for (int i = 0; i < Ratings.Count; i++)
			{
				int entity_id = entity_ids[i];
				rating_counts[entity_id]++;
				entity_averages[entity_id] += Ratings[i];
			}

			global_average = Ratings.Average;

			for (int i = 0; i <= max_entity_id; i++)
				if (rating_counts[i] > 0)
					entity_averages[i] /= rating_counts[i];
				else
					entity_averages[i] = global_average;
		}

		///
		public override void SaveModel(string file)
		{
			// do nothing
		}

		///
		public override void LoadModel(string file)
		{
			Train();
		}
	}
}