// Copyright (C) 2011, 2012 Zeno Gantner
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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Abstract class that uses an average (by entity) rating value for predictions</summary>
	public abstract class EntityAverage : IncrementalRatingPredictor
	{
		/// <summary>The average rating for each entity</summary>
		protected IList<float> entity_averages;

		/// <summary>The global average rating (default prediction if there is no data about an entity)</summary>
		protected float global_average;

		/// <summary>return the average rating for a given entity</summary>
		/// <param name="index">the entity index</param>
		public float this [int index] {
			get {
				if (index < entity_averages.Count)
					return entity_averages[index];
				else
					return global_average;
			}
		}

		/// <summary>Train the recommender according to the given entity type</summary>
		/// <param name="entity_ids">list of all entity IDs in the training data (per rating)</param>
		/// <param name="max_entity_id">the maximum entity ID</param>
		protected void Train(IList<int> entity_ids, int max_entity_id)
		{
			var rating_sums   = new List<double>();
			var rating_counts = new List<int>();
			entity_averages = new List<float>();
			for (int i = 0; i <= max_entity_id; i++)
			{
				rating_sums.Add(0);
				rating_counts.Add(0);
				entity_averages.Add(0);
			}

			for (int i = 0; i < ratings.Count; i++)
			{
				int entity_id = entity_ids[i];
				rating_counts[entity_id]++;
				rating_sums[entity_id] += ratings[i];
			}

			global_average = ratings.Average;

			for (int i = 0; i <= max_entity_id; i++)
				if (rating_counts[i] > 0)
					entity_averages[i] = (float) (rating_sums[i] / rating_counts[i]);
				else
					entity_averages[i] = global_average;
		}

		/// <summary>Retrain the recommender according to the given entity type</summary>
		/// <param name="entity_id">the ID of the entity to update</param>
		/// <param name="indices">list of indices to use for retraining</param>
		protected void Retrain(int entity_id, IList<int> indices)
		{
			double sum = 0;
			int count = 0;

			foreach (int i in indices)
			{
				count++;
				sum += ratings[i];
			}

			if (count > 0)
				entity_averages[entity_id] = (float) (sum / count);
			else
				entity_averages[entity_id] = global_average;
		}

		///
		public override void SaveModel(string filename)
		{
			using ( StreamWriter writer = Model.GetWriter(filename, this.GetType(), "2.03") )
			{
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
				writer.WriteVector(entity_averages);
			}
		}

		///
		public override void LoadModel(string filename)
		{
			using ( StreamReader reader = Model.GetReader(filename, this.GetType()) )
			{
				this.global_average = float.Parse(reader.ReadLine(), CultureInfo.InvariantCulture);
				this.entity_averages = reader.ReadVector();
			}
		}
	}
}