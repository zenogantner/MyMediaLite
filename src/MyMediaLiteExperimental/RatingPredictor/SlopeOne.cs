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
using System.Linq;
using MyMediaLite.Data;


namespace MyMediaLite.RatingPredictor
{
	/// <summary>
	/// Weighted Slope One
	/// </summary>
	/// <remarks>
	/// http://www.daniel-lemire.com/fr/documents/publications/SlopeOne.java
	/// </remarks>
	public class SlopeOne : Memory
	{
		// TODO use SparseMatrix<double> and SparseMatrix<int>
  		private Dictionary<int, double>[] diff_matrix;
  		private Dictionary<int, int>[] freq_matrix;

		private double global_average;

		/// <inheritdoc/>
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				return false;
			if (item_id > MaxItemID)
				return false;
			if (freq_matrix[item_id].Count == 0)
				return false;
			return true;
		}

		/// <inheritdoc/>
		public override double Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID)
				return global_average;

			double prediction = 0.0;
			int frequency = 0;

			foreach (RatingEvent r in Ratings.ByUser[user_id])
			{
				int f;
				if (freq_matrix[item_id].TryGetValue(r.item_id, out f))
				{
					prediction += ( diff_matrix[item_id][r.item_id] + r.rating ) * f;
					frequency += f;
				}
			}

			if (frequency == 0)
				return global_average;

    		return (double) prediction / frequency;
		}

		/// <inheritdoc/>
		public override void Train()
		{
			InitModel();

			// compute difference sums and frequencies
			foreach (var user_ratings in Ratings.ByUser)
				foreach (RatingEvent r in user_ratings)
	        		foreach (RatingEvent r2 in user_ratings)
					{
	          			if (!freq_matrix[r.item_id].ContainsKey(r2.item_id))
						{
							freq_matrix[r.item_id][r2.item_id] = 0;
	          				diff_matrix[r.item_id][r2.item_id] = 0.0;
						}

	          			freq_matrix[r.item_id][r2.item_id] += 1;
	          			diff_matrix[r.item_id][r2.item_id] += r.rating - r2.rating;
	        		}

			// compute average differences
			for (int i = 0; i <= MaxItemID; i++)
			{
				//var relevant_items = diff_matrix[i].Keys.ToList();
				foreach (int j in diff_matrix[i].Keys.ToList())
					diff_matrix[i][j] /= freq_matrix[i][j];
			}
		}

		private void InitModel()
		{
			// default value if no prediction can be made
			global_average = Ratings.Average;

			// create data structure
    		diff_matrix = new Dictionary<int, double>[MaxItemID + 1];
    		freq_matrix = new Dictionary<int, int>[MaxItemID + 1];
    		for (int i = 0; i <= MaxItemID; i++)
			{
      			diff_matrix[i] = new Dictionary<int, double>();
      			freq_matrix[i] = new Dictionary<int, int>();
			}
		}

		/// <inheritdoc/>
		public override void LoadModel(string file)
		{
			InitModel();
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void SaveModel(string file)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			 return "slope-one";
		}
	}
}