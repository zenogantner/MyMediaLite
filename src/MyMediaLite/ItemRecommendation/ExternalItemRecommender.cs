// Copyright (C) 2012, 2013 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Uses externally computed predictions</summary>
	/// <remarks>
	/// <para>
	///   This recommender is for loading predictions made by external (non-MyMediaLite) recommenders,
	///   so that we can use MyMediaLite's evaluation framework to evaluate their accuracy.
	/// </para>
	/// <para>
	///   This recommender does NOT support incremental updates.
	/// </para>
	/// </remarks>
	public class ExternalItemRecommender : Recommender, INeedsMappings
	{
		/// <summary>the file with the stored ratings</summary>
		public string PredictionFile { get; set; }

		///
		public IMapping UserMapping { get; set; }
		///
		public IMapping ItemMapping { get; set; }

		private IInteractions external_scores;

		/// <summary>Default constructor</summary>
		public ExternalItemRecommender()
		{
			PredictionFile = "FILENAME";
		}

		///
		public override void Train()
		{
			external_scores = MyMediaLite.Data.Interactions.FromFile(PredictionFile, UserMapping, ItemMapping);
		}

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			return external_scores.ByUser(user_id).Items.Contains(item_id);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (!CanPredict(user_id, item_id))
				return float.MinValue;

			var reader = external_scores.ByUser(user_id);
			while (reader.Read())
			{
				if (reader.GetItem() == item_id)
					return reader.GetRating();
			}
			throw new Exception("Should not happen.");
		}

		///
		public override void SaveModel(string filename) { /* do nothing */ }

		///
		public override void LoadModel(string filename) { /* do nothing */ }

		///
		public override string ToString()
		{
			return string.Format(
				"{0} prediction_file={1}",
				this.GetType().Name, PredictionFile);
		}
	}
}