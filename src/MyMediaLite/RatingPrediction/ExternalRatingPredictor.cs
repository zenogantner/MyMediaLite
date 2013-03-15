// Copyright (C) 2012 Zeno Gantner
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
using MyMediaLite.Data;
using MyMediaLite.IO;

namespace MyMediaLite.RatingPrediction
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
	public class ExternalRatingPredictor : RatingPredictor, INeedsMappings
	{
		/// <summary>the file with the stored ratings</summary>
		public string PredictionFile { get; set; }

		///
		public IMapping UserMapping { get; set; }
		///
		public IMapping ItemMapping { get; set; }

		private IRatings external_ratings;

		/// <summary>Default constructor</summary>
		public ExternalRatingPredictor()
		{
			PredictionFile = "FILENAME";
		}

		///
		public override void Train()
		{
			external_ratings = RatingData.Read(PredictionFile, UserMapping, ItemMapping);
		}

		///
		public override bool CanPredict(int user_id, int item_id)
		{
			float rating;
			return external_ratings.TryGet(user_id, item_id, out rating);
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			float rating;
			if (external_ratings.TryGet(user_id, item_id, out rating))
				return rating;
			else
				return float.MinValue;
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