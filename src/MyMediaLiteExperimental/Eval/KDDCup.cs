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
	public class KDDCup
	{
		public static void PredictTrack2(Dictionary<int, int[]> candidates, IRecommender recommender, string filename)
		{
			using (var writer = new StreamWriter(filename))
				PredictTrack2(candidates, recommender, writer);
		}

		public static void PredictTrack2(Dictionary<int, int[]> candidates, IRecommender recommender, TextWriter writer)
		{
			foreach (int user_id in candidates.Keys)
			{
				int[] user_candidates = candidates[user_id];
				
				var predictions = new double[user_candidates.Length];
				for (int i = 0; i < user_candidates.Length; i++)
					predictions[i] = recommender.Predict(user_id, user_candidates[i]);
				
				
				var positions = new List<int>(new int[] { 0, 1, 2, 3, 4, 5 });
				positions.Sort(delegate(int pos1, int pos2) { return predictions[pos2].CompareTo(predictions[pos1]); } );
				
				for (int i = 0; i < user_candidates.Length; i++)
					if (positions.IndexOf(i) < 3)
						writer.WriteLine("1");
						//writer.WriteLine("1 {0}", predictions[i]);
					else
						writer.WriteLine("0");
						//writer.WriteLine("0 {0}", predictions[i]);
			}
		}
	}
}

