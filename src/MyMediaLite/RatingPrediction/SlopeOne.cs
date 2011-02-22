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
using System.Globalization;
using System.IO;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Frequency-weighted Slope-One rating prediction</summary>
	/// <remarks>
	/// Daniel Lemire, Anna Maclachlan:
	/// Slope One Predictors for Online Rating-Based Collaborative Filtering.
	/// SIAM Data Mining (SDM 2005)
	/// http://www.daniel-lemire.com/fr/abstracts/SDM2005.html
	///
	/// This recommender does NOT support online updates. They would be easy to implement, though.
	/// 
	/// Memory use between float and double difference matrices:
	/// ml100k: 152 MB vs. 165 MB
	/// ml1m:   905 MB vs. 980 MB
	/// </remarks>
	public class SlopeOne : RatingPredictor
	{
  		private SparseMatrix<double> diff_matrix;
  		private SparseMatrix<int> freq_matrix;

		private double global_average;

		private void InitModel()
		{
			// default value if no prediction can be made
			global_average = Ratings.Average;

			// create data structure
			diff_matrix = new SparseMatrix<double>(MaxItemID + 1, MaxItemID + 1);
			freq_matrix = new SparseMatrix<int>(MaxItemID + 1, MaxItemID + 1);
		}

		/// <inheritdoc/>
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				return false;
			if (item_id > MaxItemID)
				return false;
			foreach (RatingEvent r in Ratings.ByUser[user_id])
				if (freq_matrix[item_id, r.item_id] != 0)
					return true;
			return false;
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
				int f = freq_matrix[item_id, r.item_id];
				if (f != 0)
				{
					prediction += ( diff_matrix[item_id, r.item_id] + r.rating ) * f;
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
			{
				Console.Error.WriteLine("num_entries {0} mem {1}", freq_matrix.NonEmptyEntryIDs.Count, Memory.Usage);
				foreach (RatingEvent r in user_ratings)
					foreach (RatingEvent r2 in user_ratings)
					{
			  			freq_matrix[r.item_id, r2.item_id] += 1;
			  			diff_matrix[r.item_id, r2.item_id] += (double) (r.rating - r2.rating);
					}
			}

			// compute average differences
			for (int i = 0; i <= MaxItemID; i++)
				foreach (int j in freq_matrix[i].Keys)
					diff_matrix[i, j] /= freq_matrix[i, j];
		}

		/// <inheritdoc/>
		public override void LoadModel(string file)
		{
			InitModel();

			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamReader reader = Recommender.GetReader(file, this.GetType()) )
			{
				var global_average = double.Parse(reader.ReadLine(), ni);

				var diff_matrix = (SparseMatrix<double>) IMatrixUtils.ReadMatrix(reader, this.diff_matrix);
				var freq_matrix = (SparseMatrix<int>) IMatrixUtils.ReadMatrix(reader, this.freq_matrix);

				// assign new model
				this.global_average = global_average;
				this.diff_matrix = diff_matrix;
				this.freq_matrix = freq_matrix;
			}
		}

		/// <inheritdoc/>
		public override void SaveModel(string file)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Recommender.GetWriter(file, this.GetType()) )
			{
				writer.WriteLine(global_average.ToString(ni));
				IMatrixUtils.WriteSparseMatrix(writer, diff_matrix);
				IMatrixUtils.WriteSparseMatrix(writer, freq_matrix);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			 return "SlopeOne";
		}
	}
}