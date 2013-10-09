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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.IO;
using MyMediaLite.Taxonomy;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Abstract class that uses an average (by entity) rating value for predictions</summary>
	public abstract class EntityAverage : RatingPredictor
	{
		/// <summary>The average rating for each entity</summary>
		protected IList<float> entity_averages;

		/// <summary>The global average rating (default prediction if there is no data about an entity)</summary>
		protected float global_average;

		/// <summary>Train the recommender according to the given entity type</summary>
		protected void Train(EntityType entity_type)
		{
			int max_entity_id = (entity_type == EntityType.USER) ? Interactions.MaxUserID : Interactions.MaxItemID;
			
			var rating_sums   = new List<double>();
			var rating_counts = new List<int>();
			entity_averages = new List<float>();
			for (int i = 0; i <= max_entity_id; i++)
			{
				rating_sums.Add(0);
				rating_counts.Add(0);
				entity_averages.Add(0);
			}

			var reader = Interactions.Sequential;
			while (reader.Read())
			{
				int entity_id = (entity_type == EntityType.USER) ? reader.GetUser() : reader.GetItem();
				rating_counts[entity_id]++;
				rating_sums[entity_id] += reader.GetRating();
			}

			global_average = Interactions.AverageRating();

			for (int i = 0; i <= max_entity_id; i++)
				if (rating_counts[i] > 0)
					entity_averages[i] = (float) (rating_sums[i] / rating_counts[i]);
				else
					entity_averages[i] = global_average;
		}

		///
		public override void SaveModel(string filename)
		{
			/*
				writer.WriteLine(global_average.ToString(CultureInfo.InvariantCulture));
				writer.WriteVector(entity_averages);
			*/
		}

		///
		public override void LoadModel(string filename)
		{
		}
	}
}