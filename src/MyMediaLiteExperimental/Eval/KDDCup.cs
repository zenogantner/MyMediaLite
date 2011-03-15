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

using System;
using System.Collections.Generic;
using System.IO;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation and prediction routines for the KDD Cup 2011</summary>
	public class KDDCup
	{
		/// <summary>Predict items for Track 2</summary>
		/// <param name="candidates">a mapping from user IDs to the candidate items</param>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="filename">the file to write the predictions to</param>
		public static void PredictTrack2(Dictionary<int, IList<int>> candidates, IRecommender recommender, string filename)
		{
			using (var writer = new StreamWriter(filename))
				PredictTrack2(candidates, recommender, writer);
		}

		/// <summary>Predict items for Track 2</summary>
		/// <param name="candidates">a mapping from user IDs to the candidate items</param>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="writer">the writer object to write the predictions to</param>
		public static void PredictTrack2(Dictionary<int, IList<int>> candidates, IRecommender recommender, TextWriter writer)
		{
			foreach (int user_id in candidates.Keys)
			{
				IList<int> user_candidates = candidates[user_id];

				var predictions = new double[user_candidates.Count];
				for (int i = 0; i < user_candidates.Count; i++)
					predictions[i] = recommender.Predict(user_id, user_candidates[i]);

				var positions = new List<int>(new int[] { 0, 1, 2, 3, 4, 5 });
				positions.Sort(delegate(int pos1, int pos2) { return predictions[pos2].CompareTo(predictions[pos1]); } );

				for (int i = 0; i < user_candidates.Count; i++)
					if (positions.IndexOf(i) < 3)
						writer.Write("1");
					else
						writer.Write("0");
			}
		}
	}
}

