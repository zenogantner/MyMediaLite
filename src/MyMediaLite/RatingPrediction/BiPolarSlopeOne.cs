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
	/// <summary>Bi-polar frequency-weighted Slope-One rating prediction</summary>
	/// <remarks>
	/// Daniel Lemire, Anna Maclachlan:
	/// Slope One Predictors for Online Rating-Based Collaborative Filtering.
	/// SIAM Data Mining (SDM 2005)
	/// http://www.daniel-lemire.com/fr/abstracts/SDM2005.html
	/// 
	/// This engine does NOT support online updates. They would be easy to implement, though.
	/// </remarks>
	public class BiPolarSlopeOne : RatingPredictor
	{
  		private SparseMatrix<double> diff_matrix_like;
  		private SparseMatrix<int>    freq_matrix_like;
  		private SparseMatrix<double> diff_matrix_dislike;
  		private SparseMatrix<int>    freq_matrix_dislike;

		private double global_average;

		/// <inheritdoc/>
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID)
				return false;
			if (item_id > MaxItemID)
				return false;
			foreach (RatingEvent r in Ratings.ByUser[user_id])
			{
				if (freq_matrix_like[item_id, r.item_id] != 0)
					return true;
				if (freq_matrix_dislike[item_id, r.item_id] != 0)
					return true;
			}
			return false;
		}

		/// <inheritdoc/>
		public override double Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID)
				return global_average;

			double prediction = 0.0;
			int frequencies = 0;

			double user_avg = Ratings.ByUser[user_id].Average;
			foreach (RatingEvent r in Ratings.ByUser[user_id])
				if (r.rating > user_avg)
				{
					int f = freq_matrix_like[item_id, r.item_id];
					if (f != 0)
					{
						prediction  += ( diff_matrix_like[item_id, r.item_id] + r.rating ) * f;
						frequencies += f;
					}
				}
				else
				{
					int f = freq_matrix_dislike[item_id, r.item_id];
					if (f != 0)
					{
						prediction  += ( diff_matrix_dislike[item_id, r.item_id] + r.rating ) * f;
						frequencies += f;
					}
				}

			if (frequencies == 0)
				return global_average;

			double result = (double) (prediction / frequencies);

			if (result > MaxRating)
				return MaxRating;
			if (result < MinRating)
				return MinRating;
			return result;
		}

		/// <inheritdoc/>
		public override void Train()
		{
			InitModel();

			// compute difference sums and frequencies
			foreach (var user_ratings in Ratings.ByUser)
			{
				double user_avg = user_ratings.Average;
				foreach (RatingEvent r in user_ratings)
					foreach (RatingEvent r2 in user_ratings)
						if (r.rating > user_avg && r2.rating > user_avg)
						{
							freq_matrix_like[r.item_id, r2.item_id] += 1;
							diff_matrix_like[r.item_id, r2.item_id] += r.rating - r2.rating;
						}
						else if (r.rating < user_avg && r2.rating < user_avg)
						{
							freq_matrix_dislike[r.item_id, r2.item_id] += 1;
							diff_matrix_dislike[r.item_id, r2.item_id] += r.rating - r2.rating;
						}
			}

			// compute average differences
			for (int i = 0; i <= MaxItemID; i++)
			{
				foreach (int j in freq_matrix_like[i].Keys)
					diff_matrix_like[i, j] /= freq_matrix_like[i, j];
				foreach (int j in freq_matrix_dislike[i].Keys)
					diff_matrix_dislike[i, j] /= freq_matrix_dislike[i, j];
			}
		}

		private void InitModel()
		{
			// default value if no prediction can be made
			global_average = Ratings.Average;

			// create data structure
    		diff_matrix_like = new SparseMatrix<double>(MaxItemID + 1, MaxItemID + 1);
    		freq_matrix_like = new SparseMatrix<int>(MaxItemID + 1, MaxItemID + 1);
    		diff_matrix_dislike = new SparseMatrix<double>(MaxItemID + 1, MaxItemID + 1);
    		freq_matrix_dislike = new SparseMatrix<int>(MaxItemID + 1, MaxItemID + 1);
		}

		/// <inheritdoc/>
		public override void LoadModel(string file)
		{
			InitModel();

			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamReader reader = Engine.GetReader(file, this.GetType()) )
			{
				double global_average = Double.Parse(reader.ReadLine(), ni);

				var diff_matrix_like = (SparseMatrix<double>) IMatrixUtils.ReadMatrix(reader, this.diff_matrix_like);
				var freq_matrix_like = (SparseMatrix<int>) IMatrixUtils.ReadMatrix(reader, this.freq_matrix_like);
				var diff_matrix_dislike = (SparseMatrix<double>) IMatrixUtils.ReadMatrix(reader, this.diff_matrix_dislike);
				var freq_matrix_dislike = (SparseMatrix<int>) IMatrixUtils.ReadMatrix(reader, this.freq_matrix_dislike);

				// assign new model
				this.global_average = global_average;
				this.diff_matrix_like = diff_matrix_like;
				this.freq_matrix_like = freq_matrix_like;
				this.diff_matrix_dislike = diff_matrix_dislike;
				this.freq_matrix_dislike = freq_matrix_dislike;
			}
		}

		/// <inheritdoc/>
		public override void SaveModel(string file)
		{
			var ni = new NumberFormatInfo();
			ni.NumberDecimalDigits = '.';

			using ( StreamWriter writer = Engine.GetWriter(file, this.GetType()) )
			{
				writer.WriteLine(global_average.ToString(ni));
				IMatrixUtils.WriteSparseMatrix(writer, diff_matrix_like);
				IMatrixUtils.WriteSparseMatrix(writer, freq_matrix_like);
				IMatrixUtils.WriteSparseMatrix(writer, diff_matrix_dislike);
				IMatrixUtils.WriteSparseMatrix(writer, freq_matrix_dislike);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			 return "BipolarSlopeOne";
		}
	}
}