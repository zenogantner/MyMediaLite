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
  		private SkewSymmetricSparseMatrix  diff_matrix_like;
  		private SymmetricSparseMatrix<int> freq_matrix_like;
  		private SkewSymmetricSparseMatrix  diff_matrix_dislike;
  		private SymmetricSparseMatrix<int> freq_matrix_dislike;

		private double global_average;
		private UserAverage user_average = new UserAverage();

		/// <inheritdoc/>
		public override bool CanPredict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return false;

			foreach (int index in Ratings.ByUser[user_id])
			{
				if (freq_matrix_like[item_id, Ratings.Items[index]] != 0)
					return true;
				if (freq_matrix_dislike[item_id, Ratings.Items[index]] != 0)
					return true;
			}
			return false;
		}

		/// <inheritdoc/>
		public override double Predict(int user_id, int item_id)
		{
			if (item_id > MaxItemID || user_id > MaxUserID)
				return global_average;

			double prediction = 0.0;
			int frequencies = 0;

			foreach (int index in Ratings.ByUser[user_id])
				{
					if (Ratings[index] > user_average[user_id])
					{
						int f = freq_matrix_like[item_id, Ratings.Items[index]];
						if (f != 0)
						{
							prediction  += ( diff_matrix_like[item_id, Ratings.Items[index]] + Ratings[index] ) * f;
							frequencies += f;
						}
					}
					else
					{
						int f = freq_matrix_dislike[item_id, Ratings.Items[index]];
						if (f != 0)
						{
							prediction  += ( diff_matrix_dislike[item_id, Ratings.Items[index]] + Ratings[index] ) * f;
							frequencies += f;
						}
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

			user_average.Ratings = Ratings;
			user_average.Train();

			// compute difference sums and frequencies
			foreach (int user_id in Ratings.AllUsers)
			{
				double user_avg = user_average[user_id];
				foreach (int index in Ratings.ByUser[user_id])
					foreach (int index2 in Ratings.ByUser[user_id])
						if (Ratings[index] > user_avg && Ratings[index2] > user_avg)
						{
							freq_matrix_like[Ratings.Items[index], Ratings.Items[index2]] += 1;
							diff_matrix_like[Ratings.Items[index], Ratings.Items[index2]] += (float) (Ratings[index] - Ratings[index2]);
						}
						else if (Ratings[index] < user_avg && Ratings[index2] < user_avg)
						{
							freq_matrix_dislike[Ratings.Items[index], Ratings.Items[index2]] += 1;
							diff_matrix_dislike[Ratings.Items[index], Ratings.Items[index2]] += (float) (Ratings[index] - Ratings[index2]);
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

		void InitModel()
		{
			// default value if no prediction can be made
			global_average = Ratings.Average;

			// create data structure
			diff_matrix_like = new SkewSymmetricSparseMatrix(MaxItemID + 1);
			freq_matrix_like = new SymmetricSparseMatrix<int>(MaxItemID + 1);
			diff_matrix_dislike = new SkewSymmetricSparseMatrix(MaxItemID + 1);
			freq_matrix_dislike = new SymmetricSparseMatrix<int>(MaxItemID + 1);
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

				var diff_matrix_like = (SkewSymmetricSparseMatrix) IMatrixUtils.ReadMatrix(reader, this.diff_matrix_like);
				var freq_matrix_like = (SymmetricSparseMatrix<int>) IMatrixUtils.ReadMatrix(reader, this.freq_matrix_like);
				var diff_matrix_dislike = (SkewSymmetricSparseMatrix) IMatrixUtils.ReadMatrix(reader, this.diff_matrix_dislike);
				var freq_matrix_dislike = (SymmetricSparseMatrix<int>) IMatrixUtils.ReadMatrix(reader, this.freq_matrix_dislike);

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

			using ( StreamWriter writer = Recommender.GetWriter(file, this.GetType()) )
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