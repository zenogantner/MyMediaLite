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
using System.Globalization;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using MyMediaLite.Data;

namespace MyMediaLite.Eval
{
	/// <summary>Evaluation and prediction routines for the KDD Cup 2011</summary>
	public class KDDCup
	{
		/// <summary>Predict items for Track 2</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="candidates">a mapping from user IDs to the candidate items</param>
		/// <param name="filename">the file to write the predictions to</param>
		public static void PredictTrack2(IRecommender recommender, Dictionary<int, IList<int>> candidates, string filename)
		{
       		//using (FileStream file_stream = File.Create(filename + ".gz"))
			using (FileStream file_stream = File.Create(filename))
				//using (var compressed_stream = new GZipStream(file_stream, CompressionMode.Compress))
            	//	using (var writer = new StreamWriter(compressed_stream))
					using (var writer = new StreamWriter(file_stream))
						PredictTrack2(recommender, candidates, writer);
		}

		/// <summary>Predict item scores for Track 2</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="candidates">a mapping from user IDs to the candidate items</param>
		/// <param name="filename">the file to write the predictions to</param>
		public static void PredictScoresTrack2(IRecommender recommender, Dictionary<int, IList<int>> candidates, string filename)
		{
       		//using (FileStream file_stream = File.Create(filename + ".gz"))
			using (FileStream file_stream = File.Create(filename))
				//using (var compressed_stream = new GZipStream(file_stream, CompressionMode.Compress))
            	//	using (var writer = new BinaryWriter(compressed_stream))
					using (var writer = new BinaryWriter(file_stream))
						PredictScoresTrack2(recommender, candidates, writer);
		}

		/// <summary>Predict item scores for Track 2</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="candidates">a mapping from user IDs to the candidate items</param>
		/// <param name="writer">the writer to write the scores to</param>
		public static void PredictScoresTrack2(IRecommender recommender, Dictionary<int, IList<int>> candidates, BinaryWriter writer)
		{
			foreach (int user_id in candidates.Keys)
				foreach (int item_id in candidates[user_id])
					writer.Write(recommender.Predict(user_id, item_id));
		}

		/// <summary>Predict items for Track 2</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="candidates">a mapping from user IDs to the candidate items</param>
		/// <param name="writer">the writer object to write the predictions to</param>
		public static void PredictTrack2(IRecommender recommender, Dictionary<int, IList<int>> candidates, TextWriter writer)
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

		/// <summary>Evaluate Track 2 on a validation set</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="candidates">the candidate items (per user)</param>
		/// <param name="hits">the real items (per user)</param>
		/// <returns>the error rate on this validation split</returns>
		public static double EvaluateTrack2(IRecommender recommender, Dictionary<int, IList<int>> candidates, Dictionary<int, IList<int>> hits)
		{
			int hit_count = 0;

			foreach (int user_id in candidates.Keys)
			{
				IList<int> user_candidates = candidates[user_id];

				var predictions = new double[user_candidates.Count];
				for (int i = 0; i < user_candidates.Count; i++)
					predictions[i] = recommender.Predict(user_id, user_candidates[i]);

				var positions = new List<int>(new int[] { 0, 1, 2, 3, 4, 5 });
				positions.Sort(delegate(int pos1, int pos2) { return predictions[pos2].CompareTo(predictions[pos1]); } );

				var user_true_items = new HashSet<int>(hits[user_id]);

				for (int i = 0; i < user_true_items.Count; i++)
					if (user_true_items.Contains(user_candidates[positions[i]]))
						hit_count++;
			}

			int num_pos = hits.Keys.Sum(u => hits[u].Count);
			return 1 - (double) hit_count / num_pos;
		}

		/// <summary>Evaluate Track 2 on a validation set</summary>
		/// <param name="predictions">the predictions for all candidates as one list</param>
		/// <param name="candidates">the candidate items (per user)</param>
		/// <param name="hits">the real items (per user)</param>
		/// <returns>the error rate on this validation split</returns>
		public static double EvaluateTrack2(IList<byte> predictions, Dictionary<int, IList<int>> candidates, Dictionary<int, IList<int>> hits)
		{
			int position  = 0;
			int hit_count = 0;

			foreach (int user_id in candidates.Keys)
			{
				var user_true_items = new HashSet<int>(hits[user_id]);

				foreach (int item_id in candidates[user_id])
					if (predictions[position++] == 1 && user_true_items.Contains(item_id))
						hit_count++;
			}

			int num_positive = candidates.Count * 3;
			return 1 - (double) hit_count / num_positive;
		}

		/// <summary>Predict ratings for Track 1</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="ratings">the ratings to predict</param>
		/// <param name="filename">the file to write the predictions to</param>
		public static void PredictRatings(IRecommender recommender, IRatings ratings, string filename)
		{
			using (var stream = new FileStream(filename, FileMode.Create))
				using (var writer = new BinaryWriter(stream))
					PredictRatings(recommender, ratings, writer);
		}

		/// <summary>Predict ratings for Track 1</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="ratings">the ratings to predict</param>
		/// <param name="writer">the writer object to write the predictions to</param>
		public static void PredictRatings(IRecommender recommender, IRatings ratings, BinaryWriter writer)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			for (int i = 0; i < ratings.Count; i++)
			{
				double prediction = recommender.Predict(ratings.Users[i], ratings.Items[i]);
				byte encoded_prediction = (byte) (2.55 * prediction + 0.5);
				writer.Write(encoded_prediction);
			}
		}

		/// <summary>Predict ratings (double precision)</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="ratings">the ratings to predict</param>
		/// <param name="filename">the file to write the predictions to</param>
		public static void PredictRatingsDouble(IRecommender recommender, IRatings ratings, string filename)
		{
			using (var stream = new FileStream(filename, FileMode.Create))
				using (var writer = new BinaryWriter(stream))
					PredictRatingsDouble(recommender, ratings, writer);
		}

		/// <summary>Predict ratings (double precision)</summary>
		/// <param name="recommender">the recommender to use</param>
		/// <param name="ratings">the ratings to predict</param>
		/// <param name="writer">the writer object to write the predictions to</param>
		public static void PredictRatingsDouble(IRecommender recommender, IRatings ratings, BinaryWriter writer)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			for (int i = 0; i < ratings.Count; i++)
				writer.Write(recommender.Predict(ratings.Users[i], ratings.Items[i]));
		}
	}
}

